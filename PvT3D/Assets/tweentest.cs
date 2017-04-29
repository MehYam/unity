using System.Collections;
using UnityEngine;

public class tweentest : MonoBehaviour
{
    [SerializeField] AnimationCurve curve;

	// Use this for initialization
	void Start()
    {
        var scale = transform.localScale;

        LeanTween.scaleZ(gameObject, 50, 0.5f).setEaseOutExpo().setLoopPingPong(1).setDelay(1f);
        LeanTween.scaleX(gameObject, scale.x * 5, 0.5f).setEaseOutExpo().setLoopPingPong(1).setDelay(1f);
        LeanTween.scaleY(gameObject, scale.y * 5, 0.5f).setEaseOutExpo().setLoopPingPong(1).setDelay(1f);
    }

    IEnumerator StartAnimation()
    {
        yield return new WaitForSeconds(1);

        Debug.LogFormat("time {0}", curve.keys[curve.length - 1].time);

        //LeanTween.scale(gameObject, new Vector3(3, 3, 3), curve.keys[curve.length - 1].time).setEase(curve);

        LeanTween.scaleZ(gameObject, 100, 1).setEaseInOutCirc().setLoopPingPong(1);
    }
}
