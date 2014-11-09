using UnityEngine;
using System;
using System.Collections;

using PvT.Util;

public class test : MonoBehaviour
{
    static void Log(string str)
    {
        Debug.Log(string.Format("### test frame {0} => {1}", Time.frameCount, str));
    }

	// Use this for initialization
	void Start () {
        
        // Test the tween with the lambda completion function
        GetComponent<TweenPosition>().To(Vector3.zero, 1, (go) => { Log("done animation for -> " + go); });

        // Test chaining of coroutines
        // This demonstrates how to wait for the completion of some event using coroutines.
        Log("calling CoroutineChain");
        StartCoroutine(CoroutineChain_waits_for_three_clicks());
        Log("done calling CoroutineChain");

        Log(" delta 1 " + Mathf.DeltaAngle(30, 45));
        Log(" delta 2 " + Mathf.DeltaAngle(45, 30));

        // testing what happens with multiple AddComponent calls
        gameObject.AddComponent<HopBehavior>();
        gameObject.AddComponent<HopBehavior>();
        gameObject.AddComponent<HopBehavior>();
        gameObject.AddComponent<Rigidbody2D>();
        gameObject.AddComponent<Rigidbody2D>();
        gameObject.AddComponent<Rigidbody2D>();
    }

    IEnumerator CoroutineChain_waits_for_three_clicks()
    {
        yield return null;

        Log("CoroutineChain");

        yield return null;

        Log("CoroutineChain");

        var clickTarget = _clicks + 3;
        yield return StartCoroutine(Util.YieldUntil(() =>
            {
                return clickTarget <= _clicks;
            }
        ));

        Log("CoroutineChain got its clicks, returning...");
    }

    static IEnumerator YieldUntil(Func<bool> condition)
    {
        while (!condition())
        {
            yield return new WaitForEndOfFrame();
        }
    }

    int _clicks = 0;
    void Update()
    {
        // can't do this in WaitForClicks because Input needs to be called in Update.  Very lame.
        if (Input.GetButtonDown("Fire1"))
        {
            Log("click!");

            ++_clicks;
        }
    }
}
