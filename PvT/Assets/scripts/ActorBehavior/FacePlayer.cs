using UnityEngine;
using System.Collections;

[System.Obsolete("use FacePlayerBehavior")]
public class FacePlayer : MonoBehaviour
{
    public Transform Player;
    public float RotationalInertia = 0;  // from 0 to 1, 1 being completely inert

	// Update is called once per frame
	void Update()
    {
        var previous = transform.localRotation;
        var newRot = Consts.GetLookAtAngle(transform, Player.localPosition - transform.localPosition);

        // implement a crude rotational drag by "softening" the delta.  KAI: look into relying more on the physics engine to handle this
        var angleDelta = Consts.diffAngle(previous.eulerAngles.z, newRot.eulerAngles.z);
        angleDelta *= (1 - RotationalInertia);
        transform.Rotate(0, 0, angleDelta);
	}
}
