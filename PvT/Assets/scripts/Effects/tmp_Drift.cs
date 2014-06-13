using UnityEngine;
using System.Collections;

public class tmp_Drift : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<Actor>().behavior = ActorBehaviorFactory.Instance.drift;
        GetComponent<Actor>().health = 1;
	}
}
