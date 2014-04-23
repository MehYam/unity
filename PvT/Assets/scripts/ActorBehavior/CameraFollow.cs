using UnityEngine;
using System.Collections;

public sealed class CameraFollow : MonoBehaviour
{
    public GameObject Target;
    public GameObject Border;

    float _preserveCameraZ;
    void Start()
    {
        _preserveCameraZ = camera.transform.localPosition.z;
    }

    Rect limit = new Rect();
    void CalcBounds()
    {
        var screenInWorld = Consts.GetScreenRectInWorldCoords(camera);

        var borderMin = new Vector3(Border.transform.FindChild("left").localPosition.x, Border.transform.FindChild("bottom").localPosition.y);
        var borderMax = new Vector3(Border.transform.FindChild("right").localPosition.x, Border.transform.FindChild("top").localPosition.y);

        limit.width = (borderMax.x - borderMin.x) - screenInWorld.width;
        limit.height = (borderMax.y - borderMin.y) - screenInWorld.height;
        limit.center = Vector2.zero;

        Debug.Log(string.Format("Border: {0},{1}, camera limit: {2} {3},{4}", borderMin, borderMax, limit, limit.xMin, limit.xMax));
    }

	// Update is called once per frame
	void Update()
    {
        if (limit.width == 0)
        {
            CalcBounds();
        }
        var newX = Mathf.Min(limit.xMax, Mathf.Max(Target.transform.localPosition.x, limit.xMin));
        var newY = Mathf.Min(limit.yMax, Mathf.Max(Target.transform.localPosition.y, limit.yMin));

        camera.transform.localPosition = new Vector3(newX, newY, _preserveCameraZ);

        //camera.transform.localPosition = new Vector3(newX, Target.transform.localPosition.y, _preserveCameraZ);
	}
}
