using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour
{
    public int MaxVelocity = 40;  //KAI: the downside is that we can't modify these at runtime from the IDE... unless you implement for it.
    public int Acceleration = 5;
    public GameObject Player;

	// Use this for initialization
	void Start()
    {
        Physics2D.gravity = Vector2.zero;

        var behaviors = new CompositeBehavior();
        behaviors.AddBehavior(new PlayerInput(MaxVelocity, Acceleration));
        behaviors.AddBehavior(new FaceForward());
        behaviors.AddBehavior(new FaceMouseOnFire());

        Player.GetComponent<ActorBehaviorHost>().behavior = behaviors;
	}
}
