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

    Bounds bounds = new Bounds();
    void CalcBounds()
    {
        var borderBottomLeft = new Vector3(Border.transform.FindChild("left").localPosition.x, Border.transform.FindChild("bottom").localPosition.y);
        var borderTopRight = new Vector3(Border.transform.FindChild("right").localPosition.x, Border.transform.FindChild("top").localPosition.y);

        var screenBottomLeft = camera.ScreenToWorldPoint(Vector3.zero);
        var screenTopRight = camera.ScreenToWorldPoint(new Vector3(Screen.currentResolution.width, Screen.currentResolution.height));
        var screenWidth = screenTopRight.x - screenBottomLeft.x;
        var screenHeight = screenTopRight.y - screenBottomLeft.y;

        // KAI: this is broken...
        //var minX = borderBottomLeft.x + screenWidth / 2;
        //var maxX = borderTopRight.x - screenWidth / 2;
        //var minY = borderBottomLeft.y + screenHeight / 2;
        //var maxY = borderTopRight.y - screenHeight / 2;
        var foo = 3;
        var minX = borderBottomLeft.x + foo;
        var maxX = borderTopRight.x - foo;
        var minY = borderBottomLeft.y + foo;
        var maxY = borderTopRight.y - foo;

        bounds = new Bounds(Vector3.zero, new Vector3(maxX - minX, maxY - minY));
        Debug.Log(bounds);
    }

	// Update is called once per frame
	void Update()
    {
        if (bounds.size.x == 0)
        {
            CalcBounds();
        }
        var newX = Mathf.Min(bounds.max.x, Mathf.Max(Target.transform.localPosition.x, bounds.min.x));
        var newY = Mathf.Min(bounds.max.y, Mathf.Max(Target.transform.localPosition.y, bounds.min.y));

        camera.transform.localPosition = new Vector3(newX, newY, _preserveCameraZ);

        //camera.transform.localPosition = new Vector3(newX, Target.transform.localPosition.y, _preserveCameraZ);
	}
}
