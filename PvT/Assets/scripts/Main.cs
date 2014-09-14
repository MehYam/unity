//#define AUTO_SELECT_EDITOR_GAMESTATE

using UnityEngine;
using System.Collections;

using PvT.Util;

public class Main : MonoBehaviour
{
    public PhysicsMaterial2D Bounce;

    public TextAsset Vehicles;
    public TextAsset Tanks;
    public TextAsset TankHulls;
    public TextAsset TankTurrets;
    public TextAsset Ammo;
    public TextAsset Levels;
    public TextAsset AI;
    public TextAsset Misc;

    public GameObject ProgressBar;
    public GameObject TrackingArrow;
    public GameObject OverwhelmedIndicator;

    public GameObject EffectParent;
    public GameObject AmmoParent;

    public MainSounds sounds;
    public MainMusic music;
    public HUD hud;

    /// <summary>
    /// ///////////////////////// debug/dev items
    /// </summary>
    public string defaultVehicle = "hero";
    public bool playMusic = true;
    public bool runWaves = true;
    public int startWave = 0;

    static Main _instance;
    static public Main Instance
    {
        get { return _instance; }
    }

    public IGame game { get; private set; }

    public void PlayMusic(AudioClip music)
    {
        if (playMusic)
        {
            audio.clip = music;
            audio.loop = true;
            audio.Play();
        }
    }

    // Use this for initialization
	void Start()
    {
        DebugUtil.Log(this, "Start");

        _instance = this;

        game = new GameController(CreateLoader());
        GlobalGameEvent.Instance.MapReady += OnMapReady;
        GlobalGameEvent.Instance.IntroOver += OnIntroOver;

        Physics2D.gravity = Vector2.zero;

        GlobalGameEvent.Instance.FireMainReady();
	}
    Loader CreateLoader()
    {
        return new Loader(Vehicles.text, Tanks.text, TankHulls.text, TankTurrets.text, Ammo.text, Levels.text, AI.text, Misc.text);
    }
    public void Debug_Respawn()
    {
        game.Debug_Respawn(CreateLoader());
    }
    void OnDestroy()
    {
        Debug.Log("Main.OnDestroy");

        _instance = null;
        GlobalGameEvent.ReleaseAll();
    }

    void OnMapReady(XRect bounds)
    {
#if AUTO_SELECT_GAMESTATE_EDITOR
        Selection.objects = new Object[] { GameObject.Find("_gameState") };
#endif
    }
    void OnIntroOver(MonoBehaviour script)
    {
        Debug.Log("Main.OnIntroOver");
        script.enabled = false;

        GetComponent<TutorialScript>().enabled = true;
    }
}
