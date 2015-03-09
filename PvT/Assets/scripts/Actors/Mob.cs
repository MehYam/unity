using UnityEngine;
using System.Collections;

public sealed class Mob : MonoBehaviour
{
	// Use this for initialization
	void Start()
    {
        gameObject.layer = (int)Consts.CollisionLayer.MOB;
        if (Main.Instance.MobParent != null)
        {
            gameObject.transform.parent = Main.Instance.MobParent.transform;
        }
	}
	
}
