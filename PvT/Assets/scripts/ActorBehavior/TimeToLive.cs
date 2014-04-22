using UnityEngine;
using System.Collections;

public sealed class TimeToLive : MonoBehaviour
{
    public float Seconds = 0;

    float _start;
    void Start()
    {
        _start = Time.time;
    }
	void Update()
    {
        if (Seconds > 0 && (Time.time - _start) > Seconds)
        {
            Debug.Log("Destroying.........");
            Destroy(gameObject);
        }
	}
}
