using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class Terrain : MonoBehaviour
{
    public bool allowSharp = true;
    public GameObject lineSegment;

    sealed class Hill
    {
        public const float MIN_ANGLE = -45;
        public const float MAX_ANGLE = 45;
        public const float SEGMENTS = 10;

        public readonly float startAngle;
        public readonly float endAngle;

        public Hill()
        {
            startAngle = Random.Range(MIN_ANGLE, MAX_ANGLE);
            endAngle = Random.Range(MIN_ANGLE, MAX_ANGLE);
        }
        public Hill(float startAngle)
        {
            this.startAngle = startAngle;
            endAngle = Random.Range(MIN_ANGLE, MAX_ANGLE);
        }
    }
    const float HILLS = 40;
	void Start()
    {
        // single mesh and poly collider better?  Probably.
        var bounds = lineSegment.renderer.bounds;
        var rightEdge = new Vector2(bounds.max.x, 0);
        var leftEdge = -rightEdge;

        GameObject lastSegment = null;
        var hill = new Hill(0);
        for (int h = 0; h < HILLS; ++h)
        {
            float range = hill.endAngle - hill.startAngle;
            for (int s = 0; s < Hill.SEGMENTS; ++s)
            {
                GameObject segment = (GameObject)Instantiate(lineSegment);

                var currentAngle = hill.startAngle + range * (s/Hill.SEGMENTS);
                var rotation = currentAngle;
                segment.transform.Rotate(0, 0, rotation);

                if (lastSegment != null)
                {
                    var lastRightEdge = Util.RotatePoint(rightEdge, lastSegment.transform.rotation.eulerAngles.z);
                    var thisLeftEdge = Util.RotatePoint(leftEdge, currentAngle);

                    Debug.Log(thisLeftEdge);
                    segment.transform.position = 
                        (Vector2)lastSegment.transform.position + lastRightEdge -
                        thisLeftEdge;
                }

                segment.transform.parent = transform;

                lastSegment = segment;

                if (s == 0)
                {
                    var sprite = segment.GetComponent<SpriteRenderer>();
                    sprite.color = Color.red;
                }
            }
            hill = allowSharp ? new Hill() : new Hill(hill.endAngle);
        }
	}
}
