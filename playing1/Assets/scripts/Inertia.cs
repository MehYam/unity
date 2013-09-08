using UnityEngine;
using System.Collections;

public class Inertia : MonoBehaviour
{
    const float SPEED_FACTOR = 0.03f;

    public Vector3 speed;
    public Vector3 force;
    public float decay;

    public float rotationalSpeed = 0;
    public float rotationalForce = 0;
    public float rotationalDecay = 0;
    void Start()
    {
        // use speed.y as the initial velocity
        var initialSpeed = speed.y;

        var radians = transform.localEulerAngles.z * Mathf.Deg2Rad;
        speed = new Vector3(-Mathf.Sin(radians), Mathf.Cos(radians)) * initialSpeed;
    }
    
    void Update()
    {
        transform.localPosition = transform.localPosition + (speed * SPEED_FACTOR);
	}
}
