using System.Collections;
using UnityEngine;

public class AmmoBeam : MonoBehaviour
{
    public float duration = 1;
    public float distance = 50;
    public float width = 0.5f;

    // Use this for initialization
    void Start()
    {
        // We're scaling the object up as an explosion beam effect, in order to grow the front and not the back,
        // we must position the child cube so that it's back edge is lined up to our origin.
        var visual = transform.GetChild(0);
        visual.transform.localPosition = new Vector3(0, 0, visual.transform.localScale.z / 2);

        var scale = transform.localScale;

        LeanTween.scaleZ(gameObject, distance / visual.localScale.z, duration / 2).setEaseOutExpo();
        LeanTween.scaleX(gameObject, width / visual.localScale.x, duration / 4);

        LeanTween.alpha(gameObject, 0, duration).setDestroyOnComplete(true);
    }
}
