using UnityEditor;
using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour
{
    public PhysicsMaterial2D Bounce;

    public TextAsset Vehicles;
    public TextAsset TankHulls;
    public TextAsset TankTurrets;
    public TextAsset Ammo;
    public TextAsset Levels;
    public TextAsset AI;
    public TextAsset Misc;

    public GameObject ProgressBar;

    /// <summary>
    /// ///////////////////////// debug/dev items
    /// </summary>
    public bool wavesActive = true;
    public string defaultPlane = "BEE";
    public string defaultTank = "tankhull0";
    public string defaultTurret = "tankturret0";
    public bool defaultIsPlane = true;
    /// 


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

        var loader = new Loader(Vehicles.text, Ammo.text, TankHulls.text, TankTurrets.text, Levels.text, AI.text, Misc.text);
        game = new GameController(loader);
        GlobalGameEvent.Instance.MapReady += OnMapReady;

        //Application.targetFrameRate = 3;
        //QualitySettings.vSyncCount = 2;
        Physics2D.gravity = Vector2.zero;
	}
    public void Debug_Respawn()
    {
        game.Debug_Respawn(new Loader(Vehicles.text, Ammo.text, TankHulls.text, TankTurrets.text, Levels.text, AI.text, Misc.text));
    }
    void OnDestroy()
    {
        Debug.Log("Main.OnDestroy");

        _instance = null;
        GlobalGameEvent.Instance.MapReady -= OnMapReady;
    }

    void OnMapReady(TileMap map, XRect bounds)
    {
        Selection.objects = new Object[] { GameObject.Find("_gameState") };
    }
}
