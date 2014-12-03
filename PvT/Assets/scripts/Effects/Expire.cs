using UnityEngine;
using System.Collections;

public class Expire : MonoBehaviour
{
    float _expireTime = 0;
    public void SetExpiry(float secondsFromNow)
    {
        _expireTime = Time.fixedTime + secondsFromNow;
    }
	void FixedUpdate()
    {
	    if (Time.fixedTime >= _expireTime)
        {
            GameObject.Destroy(gameObject);
        }
	}
}
