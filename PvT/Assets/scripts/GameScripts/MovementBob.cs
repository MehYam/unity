using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class MovementBob : MonoBehaviour
{
    public float range = 0.5f;

    Vector3 offset = Vector3.zero;
    Vector3 offsetTarget = Vector3.zero;
    Vector2 velocities = Vector2.zero;
    float bobTime = 0;
    void PickBobTarget()
    {
        // Get ready to pick a random location from -range to range in both directions.
        var rangeRect = new XRect(0, 0, range, range);
        rangeRect.Move(-range / 2, -range / 2);

        // Shift the range back by how much we've already bobbed
        rangeRect.Move(-offsetTarget);

        // Set the target and let Update() do the work
        offsetTarget = new Vector3(Random.Range(rangeRect.left, rangeRect.right), Random.Range(rangeRect.bottom, rangeRect.top));

        velocities = Vector2.zero;
        bobTime = Random.Range(0.1f, 0.3f);
    }

    const float EPSILON = 0.001f;
    void Update()
    {
        if (Vector3.Distance(offset, offsetTarget) < EPSILON)
        {
            PickBobTarget();
        }

        Vector2 prevOffset = offset;
        offset.x = Mathf.SmoothDamp(offset.x, offsetTarget.x, ref velocities.x, 1);
        offset.y = Mathf.SmoothDamp(offset.y, offsetTarget.y, ref velocities.y, 1);

        var diff = Util.Sub(prevOffset, offset);
        transform.Translate(diff.x, diff.y, 0);
    }
}
