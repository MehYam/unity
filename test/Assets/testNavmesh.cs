using UnityEngine;
using System.Collections;

public class testNavmesh : MonoBehaviour {

    public GameObject target;

	// Use this for initialization
	void Start () {

        var agent = GetComponent<NavMeshAgent>();

        //agent.destination = target.transform.position;
        
	}
}
