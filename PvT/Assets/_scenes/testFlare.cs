using UnityEngine;
using System.Collections;

public sealed class testFlare : MonoBehaviour
{
    public GameObject ReplaceWith;

	// Use this for initialization
	void Start()
    {
        StartCoroutine(DoReplace());	
	}
	
    IEnumerator DoReplace()
    {
        yield return new WaitForSeconds(Consts.FLARE_ANIMATION_PEAK_SECONDS);

        // allocate the new ship
        var newShip = (GameObject)Instantiate(ReplaceWith);
        newShip.transform.position = transform.position;

        // KAI: here you'd transfer the physics properties over and set IGame.player

        // move the animation over to the new thing
        var animation = transform.GetChild(0);
        animation.transform.parent = newShip.transform;

        Destroy(gameObject);
    }
}
