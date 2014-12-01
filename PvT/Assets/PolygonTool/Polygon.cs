#define USE_POLYGON_COLLIDER_2D

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if !USE_POLYGON_COLLIDER_2D
[RequireComponent(typeof(MeshCollider))]
#endif
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("Mesh/Polygon")]
public class Polygon : MonoBehaviour {

	public List<Vector2> Points = new List<Vector2>();
	public HashSet<int> Selected = new HashSet<int>();
	
	public float SmoothAngle = 35;

	[System.Serializable] public class ShapeData
	{
		public bool GenerateFront = true;
		public bool GenerateBack = true;
		public bool GenerateSides = true;
		public float Extrude = 1.0f;
		public float Elevation = 0.5f;
		public bool Enabled = true;
	}

	public Vector2 FrontUVScale = new Vector2(0.25f, 0.25f);
	public Vector2 BackUVScale = new Vector2(0.25f, 0.25f);
	public Vector2 SideUVScale = new Vector2(0.25f, 0.25f);

	public ShapeData PolygonMesh = new ShapeData();
#if !USE_POLYGON_COLLIDER_2D
	public ShapeData PolygonCollider = new ShapeData();
#endif
	public int InsertBefore = 0;

	void Start()
	{
		UpdateComponents();
	}

    public void Recenter()
    {
        // average all the points to find the middle
        Vector2 avg = Vector2.zero;
        foreach (var point in Points)
        {
            avg += point;
        }

        avg /= Points.Count;

        // subtract the average from each point to recenter
        for (var i = 0; i < Points.Count; ++i)
        {
            var point = Points[i];
            Points[i] = point - avg;
        }
        UpdateComponents();
    }

	public void UpdateComponents()
	{
		// Update mesh collider
#if !USE_POLYGON_COLLIDER_2D

		if(Application.isPlaying)
			GetComponent<MeshCollider>().sharedMesh = GenerateMesh(PolygonCollider.GenerateFront, PolygonCollider.GenerateBack, PolygonCollider.GenerateSides, PolygonCollider.Extrude, PolygonCollider.Elevation, true, true, Vector2.one, Vector2.one, Vector2.one);
#endif
		Mesh mesh = GenerateMesh(PolygonMesh.GenerateFront, PolygonMesh.GenerateBack, PolygonMesh.GenerateSides, PolygonMesh.Extrude, PolygonMesh.Elevation, true, true, FrontUVScale, BackUVScale, SideUVScale);	
		GetComponent<MeshFilter>().mesh = mesh;

        var collider2D = GetComponent<PolygonCollider2D>();
        if (collider2D != null)
        {
            UpdatePolygonCollider2D(collider2D);
        }
	}

    void UpdatePolygonCollider2D(PolygonCollider2D collider2D)
    {
        if (Points.Count > 2)
        {
            collider2D.SetPath(0, Points.ToArray());
            //collider2D.CreatePrimitive(Points.Count);
        }
    }

	Mesh GenerateMesh(bool front, bool back, bool sides, float extrude, float elevate, bool useNormals, bool useUVS, Vector2 frontUVScale, Vector2 backUVScale, Vector2 sideUVScale)
	{
		Mesh mesh = new Mesh();

		if(Points.Count == 0)
			return mesh;

		int backStart = 0;
		int sidesStart = 0;

		Vector3 elevation = Vector3.back * elevate;

		// Vertices
		{
			List<Vector3> points = new List<Vector3>();
			{
				// front
				if (front){
					foreach (Vector3 point in Points)
						points.Add(point + elevation);
				}

				backStart = points.Count;

				// back
				if (back){
					foreach (Vector3 point in Points)
						points.Add(point + Vector3.forward * extrude + elevation);
				}

				sidesStart = points.Count;

				if (sides){
					//front side
					foreach (Vector3 point in Points){
						points.Add(point + elevation);
						points.Add(point + elevation);
					}
					// back side);
					foreach (Vector3 point in Points){
						points.Add(point + Vector3.forward * extrude + elevation);
						points.Add(point + Vector3.forward * extrude + elevation);
					}
				}
			}
			mesh.vertices = points.ToArray();
		}

		// uvs
		if(useUVS){
			List<Vector2> uvs = new List<Vector2>();

			float totalLength = 0;
			// calculate total length
			{
				Vector2 previousPoint = Points[Points.Count-1];
				foreach (Vector2 point in Points){
					totalLength += (point - previousPoint).magnitude;
					previousPoint = point;
				}
			}

			if (front){
				for (int i = 0; i < Points.Count; i++){
					Vector2 point = Points[i];
					uvs.Add(new Vector2(point.x * frontUVScale.x, point.y * frontUVScale.y));
				}
			}
			if (back){
				for (int i = 0; i < Points.Count; i++){
					Vector2 point = Points[i];
					uvs.Add(new Vector2(point.x * backUVScale.x, point.y * backUVScale.y));
				}
			}

			if(sides){
				for(int z = 0; z < 2; z ++){
					Vector2 previousPoint = Points[0];
					float length = 0;

					for(int i = 0; i < Points.Count; i++){
						Vector2 point = Points[i];
						length += (point - previousPoint).magnitude;

						uvs.Add(new Vector2(z * extrude * sideUVScale.x, (i == 0 ? totalLength : length) * sideUVScale.y));
						uvs.Add(new Vector2(z * extrude * sideUVScale.x, length * sideUVScale.y));				
						

						previousPoint = point;
					}
					length += (Points[0] - previousPoint).magnitude;
				}
			}
			mesh.uv = uvs.ToArray();
		}

		bool counterClockwise;
		// Triangles (the hard part)
		{
			List<Vector2> points2D = new List<Vector2>();

			foreach (Vector3 point in Points)
				points2D.Add(point);

			List<int> indices = new List<int>();

			PolygonTool.Triangulate.Process(ref points2D, ref indices, out counterClockwise);
		
			List<int> triangles = new List<int>();
			
			// front
			if(front)
				triangles.AddRange(indices);

			// back
			if (back){
				for (int i = 0; i < indices.Count; i += 3){
					triangles.Add(indices[i + 2] + backStart);
					triangles.Add(indices[i + 1] + backStart);
					triangles.Add(indices[i + 0] + backStart);
				}
			}

			if (sides){
				// side
				int v1 = sidesStart + Points.Count * 0 + Points.Count * 2 - 1;
				int v2 = sidesStart + Points.Count * 2 + Points.Count * 2 - 1;

				for (int i = 0; i < Points.Count; i++){
					int v3 = sidesStart + Points.Count * 0 + i * 2;
					int v4 = sidesStart + Points.Count * 2 + i * 2;

					if (counterClockwise){
						triangles.Add(v1);
						triangles.Add(v3);
						triangles.Add(v2);
						triangles.Add(v3);
						triangles.Add(v4);
						triangles.Add(v2);		
					}
					else{
						triangles.Add(v1);
						triangles.Add(v2);
						triangles.Add(v3);
						triangles.Add(v3);
						triangles.Add(v2);
						triangles.Add(v4);
					}

					v1 = v3 + 1;
					v2 = v4 + 1;
				}
			}

			mesh.triangles = triangles.ToArray();
			
		}

		// normals
		if(useNormals)
		{
			List<Vector3> normals = new List<Vector3>();

			if (front){
				// front
				for (int i = 0; i < Points.Count; i++)
					normals.Add(Vector3.back);
			}

			if (back){
				for (int i = 0; i < Points.Count; i++)
					normals.Add(Vector3.forward);
			}

			// the sides
			if (sides){
				for (int repeat = 0; repeat < 2; repeat++){
					for (int i = 0; i < Points.Count; i++){
						Vector2 p1 = Points[(i + Points.Count - 1) % Points.Count];
						Vector2 p2 = Points[i];
						Vector2 p3 = Points[(i + 1) % Points.Count];

						Vector2 diff1 = (p1 - p2).normalized;
						Vector2 diff2 = (p2 - p3).normalized;

						Vector2 normal1 = new Vector3(diff1.y, -diff1.x, 0).normalized;
						Vector2 normal2 = new Vector3(diff2.y, -diff2.x, 0).normalized;
						Vector2 smoothNormal = (normal1 + normal2).normalized;

						if (counterClockwise){
							smoothNormal *= -1;
							normal1 *= -1;
							normal2 *= -1;
						}

						if (Vector2.Dot(normal1, normal2) > Mathf.Cos(Mathf.Deg2Rad * SmoothAngle)){
							normals.Add(smoothNormal);
							normals.Add(smoothNormal);
						}
						else{
							normals.Add(normal1);
							normals.Add(normal2);
						}
					}
				}
			}
			mesh.normals = normals.ToArray();
		}

		mesh.RecalculateBounds();
		return mesh;
	}
}
