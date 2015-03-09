using UnityEngine;
using System.Collections;

public class testMonoBehavior : MonoBehaviour
{
    static int iLog;
    static void Log(string str)
    {
        Debug.Log(string.Format("++++++++++++ {0}. {1}", iLog++, str));
    }

    void Awake()
    {
        Log("Awake");
    }

    // Use this for initialization
	void Start()
    {
        Log("Start");
	}

    bool _firstUpdate = false;
    void Update()
    {
        if (!_firstUpdate)
        {
            Log("Update");
            _firstUpdate = true;
        }
    }
    bool _firstFixedUpdate = false;
    void FixedUpdate()
    {
        if (!_firstFixedUpdate)
        {
            Log("FixedUpdate");
            _firstFixedUpdate = true;
        }
    }
    void OnEnable()
    {
        Log("OnEnable");
    }
    void OnDisable()
    {
        Log("OnDisable");
    }
	void OnDestroy()
    {
        Log("OnDestroy");
    }

    void OnMouseDown()
    {
        Log("OnMouseDown");
    }

    void OnMouseUp()
    {
        Log("OnMouseUp");
    }
}
