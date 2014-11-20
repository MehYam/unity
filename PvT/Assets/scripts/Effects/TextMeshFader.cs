using UnityEngine;
using System.Collections;

using PvT.Util;
public class TextMeshFader : MonoBehaviour 
{
    float    time = 1;
    float    _startTime;
    TextMesh _meshFG;
    TextMesh _meshBG;

	// Use this for initialization
	void Start()
    {
        _startTime = Time.time;

        // this is such a hack to get the drop shadow on text hack to work hack hack hack
        _meshFG = GetComponent<TextMesh>();	    
        _meshBG = transform.GetChild(0).GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update()
    {
        var alpha = 1.5f - (Time.time - _startTime) / time;
        Util.SetAlpha(_meshFG, alpha);
        if (_meshBG != null)
        {
            Util.SetAlpha(_meshBG, alpha);
        }
	}
}
