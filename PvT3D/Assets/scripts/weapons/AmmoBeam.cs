using System.Collections;
using UnityEngine;

public class AmmoBeam : MonoBehaviour
{
    public float duration = 1;
    public float maxDistance = 100;
    public float width = 0.5f;

    Actor us;
    bool piercing = true;
    Layers.Layer hostileLayer;

    // Use this for initialization
    void Start()
    {
        us = GetComponent<Actor>();

        hostileLayer = (gameObject.layer == Layers.FriendlyAmmo.idNum) ? Layers.Enemy : Layers.Friendly;
        var layerMask = Layers.Environment.mask | Layers.Inanimate.mask | hostileLayer.mask;

        // Raycast to see where this laser goes
        float distance = RaycastTargetsAndReturnDistance(layerMask);

        // Add a meter to pierce the object some
        distance += 1;
        
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
        float distance = maxDistance;
        if (piercing)
        {
            var hits = Physics.RaycastAll(transform.position, transform.forward, distance, layerMask);

            // loop the hits and send collision to each one
            float nearestEnvironment = Mathf.Infinity;
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject.layer == Layers.Environment.idNum)
                {
                    nearestEnvironment = Mathf.Min(nearestEnvironment, Vector3.Distance(hit.point, transform.position));
                }
                else
                {
                    SendCollision(hit);
                }
            }
            distance = Mathf.Min(maxDistance, nearestEnvironment);
        }
        else
        {
            var hit = new RaycastHit();
            if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, layerMask, QueryTriggerInteraction.Collide))
            {
                distance = Vector3.Distance(hit.point, transform.position);
            }
            SendCollision(hit);
        }
        return distance;
    }
    void SendCollision(RaycastHit hit)
    {
        var otherActor = hit.collider.GetComponent<Actor>();
        if (otherActor != null && us)
        {
            otherActor.HandleActorCollision(us);
        }
    }
}
