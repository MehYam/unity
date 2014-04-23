using UnityEngine;
using System.Collections;

public class FacePlayer : MonoBehaviour
{
    public Transform Player;

	// Update is called once per frame
	void Update()
    {
        transform.localRotation = Consts.GetLookAtAngle(transform, Player.localPosition - transform.localPosition);
	}
}
