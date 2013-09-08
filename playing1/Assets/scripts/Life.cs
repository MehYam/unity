using UnityEngine;
using System.Collections;

public class Life : MonoBehaviour
{
    public float seconds = 1;
    public float health = float.NaN;

    void Update()
    {
        if (!float.IsNaN(seconds))
        {
            // might expire from time
            Debug.Log("seconds: " + seconds);
            seconds -= Time.deltaTime;
            if (seconds <= 0)
            {
                DestroyObject(gameObject);
            }
        }
        if (!float.IsNaN(health) && health <= 0)
        {
            // might expire from loss of health
            DestroyObject(gameObject);
        }
    }
}
