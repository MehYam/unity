using UnityEngine;
using System.Collections;

public sealed class Terrain : MonoBehaviour
{
    public GameObject LineSegment;

    sealed class Hill
    {
        public const float FLAT_ANGLE = 90;
        public const float MIN_ANGLE = 40;
        public const float MAX_ANGLE = 140;
        public const float SEGMENTS = 20;

        public readonly float startAngle;
        public readonly float endAngle;

        public Hill(float startAngle)
        {
            this.startAngle = startAngle;
            endAngle = Random.Range(MIN_ANGLE, MAX_ANGLE);
        }
    }
    const float HILLS = 10;
	void Start()
    {
        // single mesh and poly collider better?  Probably.
        GameObject lastSegment = null;
        var hill = new Hill(Hill.FLAT_ANGLE);
        for (int h = 0; h < HILLS; ++h)
        {
            float range = hill.endAngle - hill.startAngle;
            for (int s = 0; s < Hill.SEGMENTS; ++s)
            {
                GameObject segment = (GameObject)Instantiate(LineSegment);

                var currentAngle = hill.startAngle + range * (s/Hill.SEGMENTS);
                var rotation = currentAngle - Hill.FLAT_ANGLE;
                segment.transform.Rotate(0, 0, rotation);

                if (lastSegment != null)
                {
                    // cheesy hack
                    var lastBounds = lastSegment.renderer.bounds;
                    segment.transform.position = lastBounds.max;            
                }

                segment.transform.parent = transform;

                lastSegment = segment;
            }
            hill = new Hill(hill.endAngle);
        }
	}
}
