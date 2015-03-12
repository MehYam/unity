using UnityEngine;
using System.Collections;

public sealed class Tank : MonoBehaviour
{
    Animator left;
    Animator right;
    Rigidbody2D body;
	void Start()
    {
        left = transform.FindChild("tanktreadParentLeft").GetComponentInChildren<Animator>();
        right = transform.FindChild("tanktreadParentRight").GetComponentInChildren<Animator>();

        left.applyRootMotion = false;
        right.applyRootMotion = false;

        body = GetComponent<Rigidbody2D>();
        body.drag = 1;
        body.angularDrag = 5;
    }

    void FixedUpdate()
    {
        left.speed = right.speed = body.velocity.sqrMagnitude * 2;
    }
}
