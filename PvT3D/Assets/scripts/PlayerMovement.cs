using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class PlayerMovement : MonoBehaviour
{
    Actor actor;
    void Start()
    {
        actor = GetComponent<Actor>();
    }
    void FixedUpdate()
    {
        var movement = InputUtil.MovementVector;
        if (movement != Vector3.zero)
        {
            movement.Normalize();
            gameObject.GetComponent<Rigidbody>().AddForce(movement * actor.acceleration);
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Break();
        }
        var body = GetComponent<Rigidbody>();
        GlobalEvent.Instance.FireDebugString(string.Format("Velocity: {0:0.0}", body.velocity.magnitude));
    }
#endif
}
