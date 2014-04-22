using UnityEngine;
using System.Collections;

public class ActorBehaviorHost : MonoBehaviour
{
    public IActorBehavior behavior;
	
	// Update is called once per frame
	void Update()
    {
        if (behavior != null)
        {
            behavior.Update(gameObject);
        }
	}
}
