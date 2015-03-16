using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using PvT.DOM;
using PvT.Util;

public sealed class GameController : IGame
{
    GameObject _player;
    public GameObject player
    {
        get
        {
            if (_player == null)
            {
                _player = GameObject.Find("dummyPlayer");
                if (_player == null)
                {
                    _player = new GameObject("dummyPlayer");
                }
            }
            return _player;
        }
        private set { _player = value; }
    }
    public Vector2 playerSpawn { get; set; }
    public bool enemyInPossession { get; private set; }
    public Loader loader { get; private set; }
    public Effects effects { get; private set; }

    public GameController(Loader loader)
    {
        Debug.Log("GameState constructor " + GetHashCode());

        this.loader = loader;
        this.effects = new Effects(loader);

        var gge = GlobalGameEvent.Instance;
        gge.ActorDeath += OnActorDeath;
        gge.GainingXP += OnGainingXP;
        gge.MapReady += OnMapReady;
        gge.PlayerSpawned += OnPlayerSpawned;
    }

    void OnMapReady(GameObject unused, XRect bounds)
    {
        WorldBounds = bounds;

        var border = GameObject.Find("/border");

        border.transform.FindChild("bottom").localPosition = new Vector2(0, bounds.bottom);
        border.transform.FindChild("top").localPosition = new Vector2(0, bounds.top);
        border.transform.FindChild("left").localPosition = new Vector2(bounds.left, 0);
        border.transform.FindChild("right").localPosition = new Vector2(bounds.right, 0);

        GlobalGameEvent.Instance.FireGameReady(this);
    }

    void OnPlayerSpawned(Actor playerActor)
    {
        player = playerActor.gameObject;

        Camera.main.GetComponent<CameraFollow>().Target = player;
    }

    //KAI: some nice way to tag this as dev only?
    public void Debug_Respawn(Loader loader)
    {
        Vector3 pos = Vector3.zero;
        if (player != null)
        {
            pos = player.transform.position;
            GameObject.Destroy(player);
        }
        this.loader = loader;
        SpawnPlayer(pos);
    }

    Map _map;
    public void SetMap(GameObject mapPrefab)
    {
        if (_map != null)
        {
            GameObject.Destroy(_map.gameObject);
        }
        var mapGO = (GameObject)GameObject.Instantiate(mapPrefab);
        _map = mapGO.GetOrAddComponent<Map>();
    }
    public IList<ITarget> mapWaypoints { get { return _map.waypoints; } }
    public IList<GameObject> mapDoors { get { return _map.doors; } }

    public XRect WorldBounds 
    { 
        get; 
        private set; 
    }

    //KAI: this needs to trim down to almost nothing
    public GameObject SpawnPlayer(Vector2 location, string actorTypeName = null)
    {
        var main = Main.Instance;

        if (string.IsNullOrEmpty(actorTypeName))
        {
            actorTypeName = String.IsNullOrEmpty(main.defaultPlayer) ? "hero" : main.defaultPlayer;
        }

        var playerType = loader.GetActorType(actorTypeName);
        var playerObj = playerType.Spawn();
        
        playerObj.AddComponent<Player>();
        playerObj.transform.position = location;

        return playerObj;
    }
    public void SwapPlayer(string key)
    {
        var oldPlayer = player;

        SpawnPlayer(oldPlayer.transform.position);

        player.transform.rotation = oldPlayer.transform.rotation;
        player.GetComponent<Rigidbody2D>().velocity = oldPlayer.GetComponent<Rigidbody2D>().velocity;

        GameObject.Destroy(oldPlayer);
    }

    public GameObject SpawnMob(string actorKey)
    {
        GameObject retval;
        var actor = loader.GetActorType(actorKey);
        retval = actor.Spawn();

        retval.AddComponent<Mob>();
        return retval;
    }

    public GameObject SpawnProjectile(Actor launcher, ActorType.Weapon weapon, Consts.CollisionLayer layer)
    {
        //KAI: this is not completely sussed out yet
        var type = loader.GetActorType(weapon.actorName);

        var goAmmo = type.Spawn();
        goAmmo.layer = (int)layer;

        var ammo = goAmmo.GetComponent<Ammo>();
        if (ammo != null)
        {
            ammo.weapon = weapon;
        }

        Util.PrepareLaunch(launcher.transform, goAmmo.transform, weapon.offset, weapon.angle);
        if (launcher.actorType is TankTurretType)
        {
            // it's a turret
            SpawnMuzzleFlash(launcher.gameObject, goAmmo);
        }

        return goAmmo;
    }
    public GameObject SpawnHotspot(Actor launcher, ActorType.Weapon weapon, float damageMultiplier, Consts.CollisionLayer layer)
    {
        ///THIS IS COPY PASTA FROM SpawnAmmo
        var type = loader.GetActorType(weapon.actorName);
        var go = type.Spawn();
        var ammo = go.GetComponent<Ammo>();
        if (ammo != null)
        {
            ammo.weapon = weapon;
        }

        Main.Instance.ParentAmmo(go.transform);
        go.layer = (int)layer;

        var actor = go.GetComponent<Actor>();
        actor.collisionDamage = weapon.attrs.damage * damageMultiplier;
        actor.showsHealthBar = false;

        Util.PrepareLaunch(launcher.transform, actor.transform, weapon.offset, weapon.angle);

        GlobalGameEvent.Instance.FireAmmoSpawned(actor, weapon);
        return go;
    }
    public void PlaySound(Actor actor, Sounds.ActorEvent evt, float volume = 1)
    {
        var sound = loader.sounds.Get(actor.actorType, evt);
        if (sound != null)
        {
            AudioSource.PlayClipAtPoint(sound, actor.transform.position, volume);
        }
    }
    public void PlaySound(Sounds.GlobalEvent evt, Vector2 pos, float volume = 1)
    {
        var sound = loader.sounds.Get(evt);
        if (sound != null)
        {
            AudioSource.PlayClipAtPoint(sound, pos, volume);
        }
    }
    public void ShakeGround()
    {
        var go = new GameObject("earthquake");
        var shaker = go.GetOrAddComponent<Vibrate>();
        shaker.StartCoroutine(ShakeGroundScript(shaker));
    }
    IEnumerator ShakeGroundScript(MonoBehaviour host)
    {
        var cameraTransform = Camera.main.gameObject.transform;
        cameraTransform.parent = host.transform;

        PlaySound(Sounds.GlobalEvent.ROAR, cameraTransform.position);
        yield return new WaitForSeconds(5);

        cameraTransform.parent = null;
        GameObject.Destroy(host.gameObject);
    }

    void OnGainingXP(int xpGain, Vector2 where)
    {
        var actorType = player.GetComponent<Actor>().actorType;
        var upgradeType = PlayerData.Instance.GetTierUpgrade(actorType);

        // Check to see if this is a tier up first - if not, check for a regular level up
        if (upgradeType != actorType)
        {
            Debug.Log("Upgrading tier to " + upgradeType.ToString());

            var levelUp = player.GetOrAddComponent<TierUp>();
            levelUp.levelTo = upgradeType;

            //KAI: should be detected and fired from PlayerData
            GlobalGameEvent.Instance.FireTierUp(upgradeType);
        }
        else
        {
            var xp = PlayerData.Instance.GetXP(actorType);
            var newLevel = PlayerData.GetLevelAtXP(xp);
            if (newLevel > PlayerData.GetLevelAtXP(xp - xpGain))
            {
                Debug.Log("Level up!");
                player.GetOrAddComponent<LevelUp>();

                //KAI: should be detected and fired from PlayerData
                GlobalGameEvent.Instance.FireLevelUp(newLevel);
            }
        }
    }

    void SpawnMuzzleFlash(GameObject launcher, GameObject firePoint)
    {
        var flash = effects.GetRandomMuzzleFlash().CreateInstance();
        flash.transform.position = firePoint.transform.position;
        flash.transform.rotation = firePoint.transform.rotation;
        flash.transform.parent = launcher.transform;
    }

    void OnActorDeath(Actor actor)
    {
        //KAI: this has to go away
        var enemy = actor.gameObject.layer == (int)Consts.CollisionLayer.MOB;
        if (actor.explodesOnDeath && (enemy || actor.gameObject.layer == (int)Consts.CollisionLayer.FRIENDLY))
        {
            var asplode = effects.GetVehicleExplosion().CreateInstance();
            asplode.transform.position = actor.transform.position;

            PlaySound(Sounds.GlobalEvent.EXPLOSION, asplode.transform.position);
        }
    }
}
