using UnityEngine;
using System.Collections;

public class Expire : MonoBehaviour
{
    public float expireTime { get; private set; }
    public void SetExpiry(float secondsFromNow)
    {
        expireTime = Time.fixedTime + secondsFromNow;
    }
	void FixedUpdate()
    {
	    if (Time.fixedTime >= expireTime)
        {
            GameObject.Destroy(gameObject);
        }
	}
}
