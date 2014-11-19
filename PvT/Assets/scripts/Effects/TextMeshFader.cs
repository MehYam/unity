using UnityEngine;
using System.Collections;

using PvT.Util;
public class TextMeshFader : MonoBehaviour 
{
    float    time = 1;
    float    _startTime;
    TextMesh _mesh;

	// Use this for initialization
	void Start()
    {
        _startTime = Time.time;
        _mesh = GetComponent<TextMesh>();	    
	}
	
	// Update is called once per frame
	void Update()
    {
        Util.SetAlpha(_mesh, 1 - (Time.time - _startTime) / time);
	}
}
