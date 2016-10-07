using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour
{
    static public bool UNDER_FORCE
    {
        get { return CurrentMovementVector != Vector3.zero; } 
    }
    Actor actor;
    void Start()
    {
        actor = GetComponent<Actor>();
    }
	void FixedUpdate()
    {
        if (UNDER_FORCE)
        {
            gameObject.GetComponent<Rigidbody>().AddForce(NormalizedMovementVector * actor.acceleration);
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

    static Vector3 CurrentMovementVector
    {
        get { return new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")); }
    }
    static Vector3 NormalizedMovementVector
    {
        get
        {
            var retval = CurrentMovementVector;
            retval.Normalize();
            return retval;
        }
    }
}
