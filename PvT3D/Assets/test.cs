using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {

    float vel = 0;
	// Update is called once per frame
	void Update()
    {
        var angles = gameObject.transform.eulerAngles;

        angles.x = Mathf.SmoothDampAngle(angles.x, 60, ref vel, 1);

        Debug.Log("vel " + vel);
        vel = Mathf.Clamp(vel, 0, 10);
        gameObject.transform.eulerAngles = angles;
	}
}
