using UnityEngine;
using System.Collections;

public class testphysics : MonoBehaviour
{
    public float acceleration;

    void Start()
    {
        var trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.sortingLayerID = (int)Consts.SortingLayer.MOB;

            Debug.Log("Setting trail sort layer");
        }
    }
	// Update is called once per frame
	void Update()
    {
        if (transform.position.x > 0)
        {
            rigidbody2D.AddForce(new Vector2(-acceleration, 0));
        }
	}
}
