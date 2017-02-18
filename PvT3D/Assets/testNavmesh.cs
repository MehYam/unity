using UnityEngine;
using System.Collections;

public class testNavmesh : MonoBehaviour {

    public GameObject target;

	// Use this for initialization
	void Start () {

        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        //agent.destination = target.transform.position;

        var path = new UnityEngine.AI.NavMeshPath();
        agent.CalculatePath(target.transform.position, path);

        
	}
}
