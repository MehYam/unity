using UnityEngine;
using System.Collections;

public sealed class LevelOffPitch : MonoBehaviour
{
    float velocityX = 0;
	void Update()
    {
        var angles = gameObject.transform.eulerAngles;

        angles.x = Mathf.SmoothDampAngle(angles.x, 0, ref velocityX, 0.3f);
        gameObject.transform.eulerAngles = angles;
    }
}
