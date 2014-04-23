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
        var pixels = camera.pixelRect;
        var screenMin = camera.ScreenToWorldPoint(Vector3.zero);
        var screenMax = camera.ScreenToWorldPoint(new Vector3(pixels.xMax, pixels.yMax));
        var worldCoords = new Rect(screenMin.x, screenMax.y, screenMax.x - screenMin.x, screenMax.y - screenMin.y);

        Debug.Log(string.Format("screen dims: {0}, world coords: {1}", pixels, worldCoords));

        var borderMin = new Vector3(Border.transform.FindChild("left").localPosition.x, Border.transform.FindChild("bottom").localPosition.y);
        var borderMax = new Vector3(Border.transform.FindChild("right").localPosition.x, Border.transform.FindChild("top").localPosition.y);

        var halfScreenWidth = (screenMax.x - screenMin.x) / 2;
        var halfScreenHeight = (screenMax.y - screenMin.y) / 2;

        limit.xMin = borderMin.x + halfScreenWidth;
        limit.xMax = borderMax.x - halfScreenWidth;
        limit.yMin = borderMin.y + halfScreenHeight;
        limit.yMax = borderMax.y - halfScreenHeight;

        Debug.Log(string.Format("Border: {0},{1}, camera limit: {2}", borderMin, borderMax, limit));
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
