using UnityEngine;
using System.Collections;

public sealed class PlayerControllable : MonoBehaviour
{
    public enum FacingBehavior { FACE_FORWARD, FACE_MOUSE, FACE_MOUSE_ON_FIRE };
    public FacingBehavior Facing = FacingBehavior.FACE_MOUSE_ON_FIRE;
}
