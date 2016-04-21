//#define USE_OLD_CAMERA_CLAMPING
using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class CameraFollow : MonoBehaviour
{
    public GameObject Target;
    public GameObject Border;

    float _preserveCameraZ;
    void Start()
    {
        _preserveCameraZ = GetComponent<Camera>().transform.localPosition.z;
    }

    XRect limit = new XRect();
    void CalcBounds()
    {
        var screenInWorld = Util.GetScreenRectInWorldCoords(GetComponent<Camera>());

#if USE_OLD_CAMERA_CLAMPING
        var borderMin = new Vector2(Border.transform.FindChild("left").localPosition.x, Border.transform.FindChild("bottom").localPosition.y);
        var borderMax = new Vector2(Border.transform.FindChild("right").localPosition.x, Border.transform.FindChild("top").localPosition.y);
        var borderSize = borderMax - borderMin;

        limit = new XRect(0, 0,
            borderSize.x - screenInWorld.width,
            borderSize.y - screenInWorld.height);

        limit.Move(-limit.width / 2, -limit.height / 2);

        Util.Log("Border: {0},{1}, camera limit: {2}", borderMin, borderMax, limit);
#endif
    }

    void LateUpdate()
    {
        if (Target != null)
        {
#if USE_OLD_CAMERA_CLAMPING
            if (limit.width == 0)
            {
                CalcBounds();
            }
            var clampedPos = limit.Clamp(Target.transform.localPosition);
            GetComponent<Camera>().transform.localPosition = new Vector3(clampedPos.x, clampedPos.y, _preserveCameraZ);
#else
            var pos = Target.transform.localPosition;
            pos.z = _preserveCameraZ;
            GetComponent<Camera>().transform.localPosition = pos;
#endif
        }
    }
}
