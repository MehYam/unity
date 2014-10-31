using UnityEngine;
using System.Collections;

public class RandomRotation : MonoBehaviour
{
	void Start()
    {
	    transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 360));
	}
}
