using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour
{
    static public bool UNDER_FORCE = false;  //KAI: hack, use a global event instead
    public float acceleration = 1;
	void FixedUpdate()
    {
        var current = CurrentMovementVector;
        UNDER_FORCE = current != Vector3.zero;

        if (UNDER_FORCE)
        {
            gameObject.GetComponent<Rigidbody>().AddForce(current * acceleration);
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Break();
        }
    }
#endif

    Vector3 CurrentMovementVector
    {
        get { return new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")); }
    }
}
