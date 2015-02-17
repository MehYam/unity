using UnityEngine;
using System.Collections;

public class test : MonoBehaviour
{
    public GameObject target;

	// Use this for initialization
    Vector2 startPos;
	void Start()
    {
	    startPos = target.transform.position;
	}

	public void ReplaceTarget()
    {
        target.transform.position = startPos;
    }
}
