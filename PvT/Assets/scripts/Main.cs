//#define AUTO_SELECT_EDITOR_GAMESTATE

using UnityEngine;
using System;
using System.Collections;

using PvT.Util;

public class Main : MonoBehaviour
{
    [Serializable]
    public sealed class GameConfig
    {
        public TextAsset Actors;
        public TextAsset Tanks;
        public TextAsset TankHulls;
        public TextAsset TankTurrets;
        public TextAsset Weapons;
        public TextAsset Levels;
        public TextAsset AI;
        public TextAsset Misc;
    }
    public GameConfig config;
    [Serializable]
    public sealed class Assets
    {
        public PhysicsMaterial2D Bounce;

        public GameObject collisionParticles;
        public GameObject damageSmokeParticles;

        public GameObject HealthProgressBar;
        public GameObject OverwhelmProgressBar;
        public GameObject TrackingArrow;
        public GameObject OverwhelmedIndicator;
    }
    public Assets assets;
    [Serializable]
    public sealed class Sounds
    {
        public AudioClip HerolingBump;
        public AudioClip HerolingCapture;

        public AudioClip Laser;
        public AudioClip Bullet;
        public AudioClip SmallCollision;
        public AudioClip BigCollision;
        public AudioClip Explosion1;

        public AudioClip fanfare1;
        public AudioClip roar;
    }
    public Sounds sounds;
    [Serializable]
    public sealed class Music
    {
        public AudioClip intro;
        public AudioClip duskToDawn;
    }
    public Music music;
    
    public HUD hud;
    public GameObject EffectParent;
    public GameObject AmmoParent;
    public GameObject MobParent;

    /// <summary>
    /// ///////////////////////// debug/dev items
    /// </summary>
    public string defaultVehicle = "";
    public bool playMusic = true;

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
            //audio.loop = true;
            audio.Play();
        }
    }

    // Use this for initialization
	void Start()
    {
        DebugUtil.Log(this, "Start");

        Util.SPRITE_FORWARD_ANGLE = -90; // our sprites are pointed facing up, but in the default 2D coordinate system right == 0;

        _instance = this;

        game = new GameController(CreateLoader());
        GlobalGameEvent.Instance.MapReady += OnMapReady;
        GlobalGameEvent.Instance.IntroOver += OnIntroOver;
        GlobalGameEvent.Instance.TutorialOver += OnTutorialOver;

        Physics2D.gravity = Vector2.zero;

        GlobalGameEvent.Instance.FireMainReady();
	}
    Loader CreateLoader()
    {
        return new Loader(config);
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

    void OnMapReady(GameObject unused, XRect bounds)
    {
#if AUTO_SELECT_GAMESTATE_EDITOR
        Selection.objects = new Object[] { GameObject.Find("_main") };
#endif
    }
    void OnIntroOver(MonoBehaviour script)
    {
        Debug.Log("Main.OnIntroOver");
        script.enabled = false;

        GetComponent<TutorialScript>().enabled = true;
    }
    void OnTutorialOver(MonoBehaviour script)
    {
        Debug.Log("Main.OnTutorialOver");
        script.enabled = false;

        GetComponent<LevelScript>().enabled = true;
    }
}
