using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<TweenPosition>().To(Vector3.zero, 1, (gameObject) => { Debug.Log("done " + gameObject); });
	}
}
