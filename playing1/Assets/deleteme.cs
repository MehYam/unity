using UnityEngine;
using System.Collections;

public class deleteme : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        var filter = GetComponent<MeshFilter>();

        Debug.Log(string.Format("Filter: {1}, {0} ", filter.mesh.bounds, name));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
