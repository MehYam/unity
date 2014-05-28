using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour
{
    public TextAsset Enemies;
    public TextAsset Levels;
    public GameObject Explosion;

    public int MaxVelocity = 40;  //KAI: the downside is that we can't modify these at runtime from the IDE... unless you implement for it.
    public int Acceleration = 5;
    public GameObject Player;

    static Main _instance;
    static public Main Instance
    {
        get { return _instance; }
    }

    public GameState gameState { get; private set; }

    // Use this for initialization
	void Start()
    {
        _instance = this;

        // prime the game state
        gameState = new GameState(Enemies.text, Levels.text);

        //Application.targetFrameRate = 3;
        //QualitySettings.vSyncCount = 2;

        Physics2D.gravity = Vector2.zero;

        var behaviors = new CompositeBehavior();
        behaviors.AddBehavior(new PlayerInput(MaxVelocity, Acceleration));
        behaviors.AddBehavior(new FaceForward());
        behaviors.AddBehavior(new FaceMouseOnFire());

        Player.GetComponent<ActorBehaviorHost>().behavior = behaviors;

        // begin the game
        gameState.Start();
	}
    void OnDestroy()
    {
        _instance = null;
    }
}
