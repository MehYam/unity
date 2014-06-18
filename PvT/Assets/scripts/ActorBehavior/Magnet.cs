using UnityEngine;
using System.Collections;

using PvT.Util;

public class Magnet : MonoBehaviour
{
    public Transform Target;
    public float Magnitude = 1;
	void FixedUpdate()
    {
        if (Target != null)
        {
            //KAI: should be using world position instead of local position for most of these
            var angle = Util.GetLookAtAngle(transform, Target.transform.localPosition - transform.localPosition);
            var directionVector = Util.RotatePoint(new Vector2(Magnitude, 0), angle.eulerAngles.z);

            rigidbody2D.AddForce(directionVector);
        }
	}
}
