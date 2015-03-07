#define USE_POLYGON_COLLIDER_2D

using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Polygon))]
public class PolygonEditor : Editor {

	// Easy access to the polygon target
	Polygon polygon{
		get { return (Polygon) target; }
	}

	// The different states the polygon editor can be in
	public enum State
	{
		Normal,
		Add,
		Modify,
		Dragging,
		SelectionBox,
	}

	public State CurrentState;

	// Set of selected points
	public HashSet<int> Selected
	{
		get { return polygon.Selected; }
		set { polygon.Selected = value; }
	}
	
	private Vector3 m_mouseStartPosition;

	private Texture2D m_nodeTexture;
	private Texture2D m_selectedNodeTexture;

	private bool m_hideWireframe = false;

	private bool m_hasDragged; // Used so we only create an undo when theres a drag once

	private HashSet<EventType> m_eventsToRepaint = new HashSet<EventType>();
	
	// Constructor
	public PolygonEditor()
	{
		m_nodeTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets\\PolygonTool\\Editor\\Node.png", typeof(Texture2D));
		m_selectedNodeTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets\\PolygonTool\\Editor\\SelectedNode.png", typeof(Texture2D));

		m_eventsToRepaint.Add(EventType.KeyDown);
		m_eventsToRepaint.Add(EventType.KeyUp);
		m_eventsToRepaint.Add(EventType.MouseMove);
		m_eventsToRepaint.Add(EventType.MouseDrag);
		m_eventsToRepaint.Add(EventType.MouseDown);
		m_eventsToRepaint.Add(EventType.MouseUp);
	}

	// We use this to detect if we are holding the left mouse button in add mode
	private bool m_leftMouseDownAdd = false;

	// Draws and handles messages in the Unity scene view
	public void OnSceneGUI()
	{
		Renderer renderer = polygon.GetComponent<Renderer>();
		if (renderer)
			EditorUtility.SetSelectedWireframeHidden(renderer, CurrentState != State.Normal && m_hideWireframe);

		if(renderer && renderer.sharedMaterial == null){
			renderer.sharedMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets\\PolygonTool\\ExampleMaterial.mat", typeof(Material));
		}

		// If we are normal, exit now
		if (CurrentState == State.Normal)
			return;
		
		// This prevents us from selecting other objects in the scene
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		HandleUtility.AddDefaultControl(controlID);

		// Hide and show wireframe if we press control and W
		if (Event.current.control){
			if (KeyDown(KeyCode.W)){
				Event.current.Use();
				m_hideWireframe = !m_hideWireframe;
				SceneView.RepaintAll();
			}
		}

		// For certain messages, repaint
		if( m_eventsToRepaint.Contains(Event.current.type))
			SceneView.RepaintAll();

		// If we are holding alt, allow normal controls to happen
		if(Event.current.alt)
			return;

		// Draw the outline
		DrawPolygonOutline();
		
		Rect selectionBoxArea = GetSelectionBoxArea();
		HashSet<int> selected = GetHighlightedPoints(selectionBoxArea);

		// Draw the rectangles at the edges
		DrawPoints(ref selected);

		switch (CurrentState)
		{
			case State.Add:
				if(LeftMouseDown){
					m_leftMouseDownAdd = true;
				}
				if (LeftMouseUp){
					m_leftMouseDownAdd = false;
					Vector3 position = WorldMousePosition;
                    Undo.RecordObjects(polygon.GetComponents<Component>(), "Add points");
					
					polygon.Points.Insert(polygon.InsertBefore, WorldToLocal(position));
					polygon.InsertBefore++;

					polygon.UpdateComponents();
				}
				break;

			case State.Modify:
				if (LeftMouseDown)
				{
					int i = ClosetPoint(Event.current.mousePosition);

					if (i == -1)
					{
						m_mouseStartPosition = WorldMousePosition;
						CurrentState = State.SelectionBox;
					}
					else{
						polygon.InsertBefore = i + 1;

						if(Event.current.control){
							if (selected.Contains(i))
								selected.Remove(i);
							else
								selected.Add(i);
						}
						else{
							if (!selected.Contains(i)){
								selected.Clear();
								selected.Add(i);						
							}
						}
						m_mouseStartPosition = WorldMousePosition;
						CurrentState = State.Dragging;
					}
				}

				if(DeleteKeyDown){
					// This function is kinda bad...its assuming that the selected set will iterate
					// smallest to biggest, and then subtracts a value for the index shift...theres probably
					// a better way but I was in a rush
					if (selected.Count > 0)
					{
						Undo.RecordObjects(polygon.GetComponents<Component>(), "Deleting points");
						int subtract = 0;
						foreach (int item in selected){
							polygon.Points.RemoveAt(item - subtract);
							subtract++;
						}

						polygon.InsertBefore = polygon.Points.Count;
						// Delete!
						polygon.UpdateComponents();
						Event.current.Use();
					}
				}
				break;

			case State.SelectionBox:
				Handles.BeginGUI();
				GUI.Box(selectionBoxArea, "");
				Handles.EndGUI();

				if (LeftMouseUp){
					CurrentState = State.Modify;
					Selected = selected;
					SceneView.RepaintAll();
				}
				break;

			case State.Dragging:
				Vector2 mouseDelta = WorldToLocal(WorldMousePosition) - WorldToLocal(m_mouseStartPosition);
				m_mouseStartPosition = WorldMousePosition;

				if(mouseDelta.magnitude > 0 && !m_hasDragged){
					Undo.RecordObjects(polygon.GetComponents<Component>(), "Move points");
					m_hasDragged = true;
				}

				foreach(int i in selected){
					polygon.Points[i] += mouseDelta;
				}

				if (LeftMouseUp){
					m_hasDragged = false;
					CurrentState = State.Modify;
					polygon.UpdateComponents();
				}

				break;
		}

		if (LeftMouseDown || LeftMouseUp)
			Event.current.Use();

	}

	// Draws the textures for the points
	private void DrawPoints(ref HashSet<int> selected)
	{	
		Handles.BeginGUI();
		for (int i = 0; i < polygon.Points.Count; i++)
		{
			Vector3 position = polygon.Points[i];
			Vector3 world = LocalToWorld(position);
			Vector3 screenPosition = WorldToScreen(world);
			Rect rect = new Rect(-8, -8, 16, 16);
			rect.x += screenPosition.x;
			rect.y += screenPosition.y;

			DrawTexture(selected.Contains(i) ? m_selectedNodeTexture : m_nodeTexture, screenPosition, new Vector2(6, 6));
		}
		Handles.EndGUI();	
	}

	// Gets the highlighted points. 
	private HashSet<int> GetHighlightedPoints(Rect selectionBoxArea)
	{
		HashSet<int> selected = Selected;

		if (CurrentState == State.SelectionBox)
		{
			selected = new HashSet<int>(Selected);

			if (!Event.current.control)
				selected.Clear();

			HashSet<int> areaSelected = new HashSet<int>();
			FindPointsInRect(selectionBoxArea, ref areaSelected);

			if (Event.current.control)
				if (Event.current.shift)
					selected.ExceptWith(areaSelected);
				else
					selected.UnionWith(areaSelected);
			else
				selected.UnionWith(areaSelected);
		}

		return selected;
	}

	// Gets the selection box area
	private Rect GetSelectionBoxArea()
	{
		Rect selectionBoxArea = new Rect(0, 0, 0, 0);
		if (CurrentState == State.SelectionBox){
			selectionBoxArea = AbsoluteRectangle(WorldToScreen(m_mouseStartPosition), Event.current.mousePosition);
		}
		return selectionBoxArea;
	}

	// Helper function
	public Vector2 WrappedIndex(List<Vector2> array, int index)
	{
		return array[(index + array.Count) % array.Count];
	}

	// Draws the outline of the polygon
	private void DrawPolygonOutline()
	{	
		Handles.color = Color.white;
		Handles.DrawPolyLine(LocalsToWorlds(polygon.Points));

		if (polygon.Points.Count >= 2)
		{
			Vector2[] points = { polygon.Points[0], polygon.Points[polygon.Points.Count - 1] };
			Handles.DrawPolyLine(LocalsToWorlds(points));
		}

		if (CurrentState == State.Add && m_leftMouseDownAdd)
		{
			// Display elastic band
			if (polygon.Points.Count > 0)
			{
				Vector2[] points = { WrappedIndex(polygon.Points, polygon.InsertBefore-1), WorldToLocal(WorldMousePosition), WrappedIndex(polygon.Points, polygon.InsertBefore) };
				Handles.color = Color.grey;
				Handles.DrawPolyLine(LocalsToWorlds(points));
			}
		}

		if (polygon.Points.Count >= 3)
		{
			// Show the user which segment will get split when they add
			Vector2[] points = { WrappedIndex(polygon.Points, polygon.InsertBefore - 1), WrappedIndex(polygon.Points, polygon.InsertBefore) };
			Handles.color = Color.cyan;
			Handles.DrawPolyLine(LocalsToWorlds(points));
		}

	}
	
	// Find an absolute rectangle from 2 points. The rectangle will not have a negative width or height
	public static Rect AbsoluteRectangle(Vector2 start, Vector2 end)
	{
		return new Rect(0,0,0,0)
		            	{
		            		xMin = Mathf.Min(start.x, end.x),
		            		yMin = Mathf.Min(start.y, end.y),
		            		xMax = Mathf.Max(start.x, end.x),
		            		yMax = Mathf.Max(start.y, end.y)
		            	};
	}

	// Draws a texture at its pixel size
	public void DrawTexture(Texture2D texture, Vector2 position, Vector2 origin)
	{
		Rect area = new Rect(-origin.x, -origin.y, texture.width, texture.height);
		area.x += position.x;
		area.y += position.y;

		GUI.DrawTexture(area, texture);
	}

	// Determine if the left mouse button is down
	public bool LeftMouseDown
	{
		get { return Event.current.type == EventType.MouseDown && Event.current.button == 0; }
	}

	// Determine if the left mouse button is up
	public bool LeftMouseUp
	{
		get { return Event.current.type == EventType.MouseUp && Event.current.button == 0; }	
	}

	// Determine if we are pressing the delete key
	public bool DeleteKeyDown
	{
		get { return KeyDown(KeyCode.Delete) ; }
	}

	// Determine if we are pressing a key
	public bool KeyDown(KeyCode keyCode)
	{
		return Event.current.type == EventType.KeyDown && Event.current.keyCode == keyCode;
	}

	// Find the closet point in the polygon - this could be made more generic
	int ClosetPoint(Vector2 position)
	{
		float minPixelDistance = 10;

		List<Vector2> points = polygon.Points;
		int ret = -1;

		for( int i = 0; i < points.Count; i++){
			Vector3 screenPosition = WorldToScreen(LocalToWorld(points[i]));
			float dist = Vector2.Distance(position, screenPosition);
			if(dist < minPixelDistance){
				ret = i;
				minPixelDistance = dist;
			}
		}
		return ret;
	}

	// Finds all the points that are contained within the rectangle - this could be made more generic
	void FindPointsInRect(Rect rect, ref HashSet<int> contained)
	{
		contained.Clear();

		List<Vector2> points = polygon.Points;
		for (int i = 0; i < points.Count; i++)
		{
			Vector3 screenPosition = WorldToScreen(LocalToWorld(points[i]));

			if (rect.Contains(screenPosition))
				contained.Add(i);
		}
	}

	// Converts a local point to a world point
	Vector3 LocalToWorld(Vector2 local)
	{
		return polygon.transform.TransformPoint(local);	
	}

	// Converts a local point to a world point
	Vector3[] LocalsToWorlds(IEnumerable<Vector2> locals)
	{
		return locals.Select(local => LocalToWorld(local)).ToArray();
	}

	// Converts a world point to a local point
	Vector2 WorldToLocal(Vector3 world)
	{
		Transform transform = polygon.gameObject.transform;
		return transform.InverseTransformPoint(world);
	}

	// Converts a screen point to a world point
	Vector3 ScreenToWorld(Vector2 screen)
	{
		Ray ray = HandleUtility.GUIPointToWorldRay(screen);
		Plane plane = new Plane(polygon.transform.forward, polygon.transform.position);
		float enter;
		plane.Raycast(ray, out enter);

		return ray.origin + ray.direction * enter;
	}

	// Converts a world point to a screen point
	Vector2 WorldToScreen(Vector3 world)
	{
		return HandleUtility.WorldToGUIPoint(world);
	}

	// Converts a screen point to a local point
	Vector2 ScreenToLocal(Vector2 screen)
	{
		return LocalToWorld(ScreenToWorld(screen));
	}

	// Converts a local point to a screen point
	Vector2 LocalToScreen(Vector2 local)
	{
		return WorldToScreen(LocalToWorld(local));
	}

	// Gets the world mouse position
	Vector3 WorldMousePosition{
		get{
			return ScreenToWorld(Event.current.mousePosition);
		}
	}

	// Draws a button because Unity doesn't have a EditorGUILayout function for it :/
	bool Button(string text, bool isChecked)
	{
		Rect area = EditorGUILayout.BeginHorizontal();

		GUILayout.Label("");
	
		EditorGUILayout.EndHorizontal();

		Color old = GUI.color;
		
		if (isChecked)
			GUI.color = Color.green;

		bool ret = GUI.Button(area, text);
		GUI.color = old;

		return ret != isChecked;
	}

	// Draw GUI stuff
	public override void OnInspectorGUI()
	{
		bool oldAddPoints = CurrentState == State.Add;
		bool oldModify = CurrentState == State.Modify || CurrentState == State.Dragging || CurrentState == State.SelectionBox;

		EditorGUILayout.BeginHorizontal();
		bool addPoints = Button("Add points", oldAddPoints);
		bool modify = Button("Modify points", oldModify);
		EditorGUILayout.EndHorizontal();

		if (addPoints != oldAddPoints)
		{
			CurrentState = addPoints ? State.Add : State.Normal;
			SceneView.RepaintAll();
		}

		if (modify != oldModify)
		{
			CurrentState = modify ? State.Modify : State.Normal;
			SceneView.RepaintAll();
		}
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Center points"))
        {
            polygon.Recenter();
        }
        if (GUILayout.Button("Save mesh"))
        {
            Debug.Log("Saving mesh not yet implemented");
        }
        EditorGUILayout.EndHorizontal();

		ShapeInspectorGUI(polygon.PolygonMesh, "Polygon mesh");
#if !USE_POLYGON_COLLIDER_2D
		ShapeInspectorGUI(polygon.PolygonCollider, "Polygon collider");
#endif
		float smoothAngle = EditorGUILayout.FloatField("Smooth angle", polygon.SmoothAngle);
		if(smoothAngle != polygon.SmoothAngle){
			polygon.SmoothAngle = smoothAngle;
			polygon.UpdateComponents();
		}

		polygon.FrontUVScale = EditorGUILayout.Vector2Field("Front uv scale", polygon.FrontUVScale);
		polygon.BackUVScale = EditorGUILayout.Vector2Field("Back uv scale", polygon.BackUVScale);
		polygon.SideUVScale = EditorGUILayout.Vector2Field("Side uv scale", polygon.SideUVScale);

		if(GUI.changed)
			polygon.UpdateComponents();
	}

	public void ShapeInspectorGUI(Polygon.ShapeData shapeData, string name)
	{
		shapeData.Enabled = EditorGUILayout.BeginToggleGroup(name + " enabled", shapeData.Enabled);
		shapeData.GenerateFront = EditorGUILayout.Toggle("Generate front", shapeData.GenerateFront);
		shapeData.GenerateBack = EditorGUILayout.Toggle("Generate back", shapeData.GenerateBack);
		shapeData.GenerateSides = EditorGUILayout.Toggle("Generate sides", shapeData.GenerateSides);
		shapeData.Extrude = EditorGUILayout.FloatField("Extrude", shapeData.Extrude);
		shapeData.Elevation = EditorGUILayout.FloatField("Elevation", shapeData.Elevation);
		EditorGUILayout.EndToggleGroup();
	}
}
