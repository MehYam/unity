using UnityEngine;
using System.Collections;

// KAI: this and GameState both do the same thing - i.e. Player should probably 
// come from GameState instead?
public class Main : MonoBehaviour
{
    public TextAsset Vehicles;
    public TextAsset TankHulls;
    public TextAsset TankTurrets;
    public TextAsset Ammo;
    public TextAsset Levels;
    public TextAsset AI;
    public GameObject Explosion;

    static Main _instance;
    static public Main Instance
    {
        get { return _instance; }
    }

    public GameController game { get; private set; }

    // Use this for initialization
	void Start()
    {
        _instance = this;

        // prime the game state
        var loader = new Loader(Vehicles.text, Ammo.text, TankHulls.text, TankTurrets.text, Levels.text, AI.text);
        game = new GameController(loader);

        //Application.targetFrameRate = 3;
        //QualitySettings.vSyncCount = 2;

        Physics2D.gravity = Vector2.zero;
	}
    void OnDestroy()
    {
        _instance = null;
    }
}
