using UnityEngine;
using System.Collections;

public sealed class LevelOffPitch : MonoBehaviour
{
    float velocityX = 0;
    float velocityZ = 0;
	void Update()
    {
        var angles = gameObject.transform.eulerAngles;

        angles.x = Mathf.SmoothDampAngle(angles.x, 0, ref velocityX, 1);
        angles.z = Mathf.SmoothDampAngle(angles.z, 0, ref velocityZ, 1);

        gameObject.transform.eulerAngles = angles;
    }
}
