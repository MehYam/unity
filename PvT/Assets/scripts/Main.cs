using UnityEngine;
using System.Collections;

// KAI: this and GameState both do the same thing - i.e. Player should probably 
// come from GameState instead?
public class Main : MonoBehaviour
{
    public TextAsset Vehicles;
    public TextAsset Ammo;
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
        gameState = new GameState(Vehicles.text, Ammo.text, Levels.text);

        //Application.targetFrameRate = 3;
        //QualitySettings.vSyncCount = 2;

        Physics2D.gravity = Vector2.zero;

        var behaviors = new CompositeBehavior();
        behaviors.Add(new PlayerInput(MaxVelocity, Acceleration));
        behaviors.Add(new FaceForward());
        behaviors.Add(new FaceMouseOnFire());

        Player.GetComponent<Actor>().behavior = behaviors;
	}
    void OnDestroy()
    {
        _instance = null;
    }
}
