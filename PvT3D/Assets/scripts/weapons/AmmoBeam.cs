using System.Collections;
using UnityEngine;

public class AmmoBeam : MonoBehaviour
{
    public float duration = 1;
    public float maxDistance = 100;
    public float width = 0.5f;

    bool piercing = false;

    // Use this for initialization
    void Start()
    {
        var layerMask = Layers.Masks.Environment | Layers.Masks.Inanimate |
            ((gameObject.layer == Layers.FriendlyAmmo) ? Layers.Masks.Enemy : Layers.Masks.Friendly);

        // Raycast to see where this laser goes
        float distance = RaycastTargetsAndReturnDistance(layerMask);
        
        // We're scaling the object up as an explosion beam effect, in order to grow the front and not the back,
        // we must position the child cube so that it's back edge is lined up to our origin.
        var visual = transform.GetChild(0);
        visual.transform.localPosition = new Vector3(0, 0, visual.transform.localScale.z / 2);

        var scale = transform.localScale;

        LeanTween.scaleZ(gameObject, distance / visual.localScale.z, duration / 20).setEaseOutExpo();
        LeanTween.scaleX(gameObject, width / visual.localScale.x, duration / 4);

        LeanTween.alpha(gameObject, 0, duration).setDestroyOnComplete(true);
    }

    float RaycastTargetsAndReturnDistance(int layerMask)
    {
        float distance = 0;
        if (piercing)
        {
            //var hits = Physics.RaycastAll(transform.position, transform.forward, distance);

            // loop the hits and send collision to each one
        }
        else
        {
            var hit = new RaycastHit();
            if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, layerMask, QueryTriggerInteraction.Collide))
            {
                Debug.LogFormat("hit detected at {0} on {1}", hit.point, hit.collider);
                distance = Vector3.Distance(hit.point, transform.position);
            }

            var actor = hit.collider.GetComponent<Actor>();
            actor.HandleActorCollision(GetComponent<Actor>());
        }
        return distance;
    }
}
