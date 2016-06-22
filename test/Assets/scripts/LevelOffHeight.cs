using UnityEngine;
using System.Collections;

public sealed class LevelOffHeight : MonoBehaviour
{
    static private float EPSILON = 0.0001f;

    public float height = 0;
    public float speed = 1f;
	void FixedUpdate()
    {
        var pos = gameObject.transform.position;
        var delta = pos.y - height;

        if (Mathf.Abs(delta) > EPSILON)
        {
            pos.y -= delta * speed * Time.fixedDeltaTime;
        }
        else
        {
            pos.y = height;
        }
        gameObject.transform.position = pos;
    }
}
