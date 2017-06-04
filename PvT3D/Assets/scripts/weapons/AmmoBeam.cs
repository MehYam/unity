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
        // Raycast to see where this laser goes
        var hit = new RaycastHit();
        int layerMask = ~LayerMask.NameToLayer("environment");
        if (Physics.Raycast(transform.position, transform.forward, out hit, 100, layerMask, QueryTriggerInteraction.Collide))
        {
            Debug.LogFormat("hit detected at {0} on {1}", hit.point, hit.collider);

            distance = Vector3.Distance(hit.point, transform.position);
        }
        //Debug.DrawRay(transform.position, transform.forward * 100, Color.white, 3, false);

        // We're scaling the object up as an explosion beam effect, in order to grow the front and not the back,
        // we must position the child cube so that it's back edge is lined up to our origin.
        var visual = transform.GetChild(0);
        visual.transform.localPosition = new Vector3(0, 0, visual.transform.localScale.z / 2);

        var scale = transform.localScale;

        //LeanTween.scaleZ(gameObject, distance / visual.localScale.z, duration / 2).setEaseOutExpo();
        LeanTween.scaleZ(gameObject, distance / visual.localScale.z, duration / 200).setEaseOutExpo();
        LeanTween.scaleX(gameObject, width / visual.localScale.x, duration / 4);

        LeanTween.alpha(gameObject, 0, duration).setDestroyOnComplete(true);
    }
}
