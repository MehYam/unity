using UnityEngine;
using System.Collections;

public class testMonoBehavior : MonoBehaviour
{
    public bool LetScriptLive = true;
    public bool LetObjectLive = true;

    int iLog;
    void Log(string str)
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
        if (!LetScriptLive)
        {
            Log("Destroying the monobehavior");
            Destroy(this);
        }
        if (!LetObjectLive)
        {
            Log("Destroying the object");
            Destroy(gameObject);
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
