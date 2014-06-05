#define PLAYER_AS_PLANE
#define DEBUG_AMMO

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public sealed class GameController
{
    public GameObject player { get; private set; }

    public Loader loader { get; private set; }
    public GameController(Loader loader)
    {
        Debug.Log("GameState constructor " + GetHashCode());

        this.loader = loader;
        GlobalGameEvent.Instance.MapReady += OnMapReady;
    }

    public XRect WorldBounds { get; private set; }
    void OnMapReady(TileMap map, XRect bounds)
    {
        GlobalGameEvent.Instance.MapReady -= OnMapReady;
        WorldBounds = bounds;

#if PLAYER_AS_PLANE
        var playerVehicle = loader.GetVehicle("OSPREY3");
        var player = SpawnWorldObject(playerVehicle);
        InitPlayer(player, playerVehicle);
        AddPlayerPlaneBehaviors(player, playerVehicle);
#else
        var tankHelper = new TankSpawnHelper(this, "tankhull4", "tankturret3");
        InitPlayer(tankHelper.hullGO, tankHelper.hull);
        AddPlayerTankBehaviors(tankHelper);

#endif
        Start();
    }

    void Start()
    {
        //StartNextLevel();
    }
    void StartNextLevel()
    {
        StartNextWave();
    }

    int _liveEnemies = 0;
    void StartNextWave()
    {
        var wave = loader.levels[0].NextWave();
        foreach (var squad in wave.squads)
        {
            VehicleType v = loader.GetVehicle(squad.enemyID);
            if (v != null)
            {
                for (int i = 0; i < squad.count; ++i)
                {
                    SpawnMob(v);
                    ++_liveEnemies;
                }
            }
            else
            {
                Debug.LogError("VehicleType not found for enemy " + squad.enemyID);
            }
        }
    }

    GameObject SpawnWorldObject(WorldObjectType worldObject, bool physics = true)
    {
        var go = worldObject.ToGameObject();
        var actor = go.AddComponent<Actor>();
        actor.worldObject = worldObject;

        go.AddComponent<DropShadow>();
        go.name = worldObject.name;

        if (physics)
        {
            var body = go.AddComponent<Rigidbody2D>();
            body.mass = float.IsNaN(worldObject.mass) ? 0 : worldObject.mass;
            body.drag = 0.1f;
        }
        return go;
    }

    void SpawnMob(VehicleType vehicle)
    {
        var go = SpawnWorldObject(vehicle);
        go.name += " enemy";

        var ai = loader.GetAI(vehicle.name);
        if (ai != null)
        {
            go.GetComponent<Actor>().behavior = ActorBehaviorScripts.Instance.Get(ai.behavior);
        }

        // put the actor at the edge
        Vector3 spawnLocation;
        var bounds = new XRect(WorldBounds);
        bounds.Inflate(-1);

        if (Consts.CoinFlip())
        {
            spawnLocation = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Consts.CoinFlip() ? bounds.min.y : bounds.max.y);
        }
        else
        {
            spawnLocation = new Vector3(Consts.CoinFlip() ? bounds.min.x : bounds.max.x, Random.Range(bounds.min.y, bounds.max.y));
        }
        go.transform.localPosition = spawnLocation;
        go.layer = (int)Consts.Layer.MOB;
    }

    //kai: this ain't perfect
    public void SpawnAmmo(Actor launcher, VehicleType type, WorldObjectType.Weapon weapon, bool player)
    {
        var go = type.ToGameObject();
        var body = go.AddComponent<Rigidbody2D>();

        body.mass = 1;
        body.drag = 0;

        var sprite = go.GetComponent<SpriteRenderer>();
#if DEBUG_AMMO
        sprite.sortingOrder = 10;
#else
        sprite.sortingOrder = Consts.AMMO_SORT_ORDER;
#endif
        var ammo = go.AddComponent<Actor>();
        ammo.worldObject = type;
        ammo.timeToLive = 2;

        var scale = launcher.transform.localScale;
        var scaledOffset = new Vector2(weapon.offset.x, weapon.offset.y);
        scaledOffset.Scale(scale);
        scaledOffset.y = weapon.offset.y;  //KAI: not sure why....

        Debug.Log(string.Format("{0} -> {1}, scale {2}", weapon.offset, scaledOffset, scale));

        ammo.transform.localPosition = Consts.Add(launcher.transform.position, scaledOffset);
        ammo.transform.RotateAround(launcher.transform.position, Vector3.forward, launcher.transform.rotation.eulerAngles.z);

        ammo.transform.Rotate(0, 0, -weapon.angle);
        //ammo.behavior = ActorBehaviorFactory.Instance.thrust;

        go.layer = (int)(player ? Consts.Layer.FRIENDLY_AMMO : Consts.Layer.MOB_AMMO);

        //KAI: not everything should get thrust, some should just get a velocity and hold it
    }

    sealed class TankSpawnHelper
    {
        public readonly TankHullType hull;
        public readonly TankPartType turret;
        public readonly GameObject hullGO;
        public readonly GameObject turretGO;
        public readonly GameObject treadLeft;
        public readonly GameObject treadRight;
        public TankSpawnHelper(GameController game, string tankHull, string tankTurret)
        {
            hull = game.loader.GetTankHull(tankHull);
            turret = game.loader.GetTankPart(tankTurret);
            var tread = game.loader.GetTankPart("tanktreadParent");

            hullGO = game.SpawnWorldObject(hull);
            turretGO = game.SpawnWorldObject(turret, false);

            hullGO.rigidbody2D.drag = 1;

            treadLeft = tread.ToGameObject();
            treadRight = tread.ToGameObject();
            treadLeft.name = "treadLeft";
            treadRight.name = "treadRight";

            turretGO.transform.parent = hullGO.transform;
            treadLeft.transform.parent = hullGO.transform;
            treadRight.transform.parent = hullGO.transform;

            var hullSprite = hullGO.GetComponent<SpriteRenderer>();
            hullSprite.sortingOrder = -2;

            var hullBounds = hullSprite.sprite.bounds;
            var pivotY = hullBounds.min.y + hull.turretPivotY / Consts.PixelsToUnits;

            turretGO.GetComponent<SpriteRenderer>().sortingOrder = -1;
            turretGO.gameObject.transform.localPosition = new Vector3(0, pivotY);

            treadLeft.gameObject.transform.Rotate(0, 0, 180);
            treadRight.gameObject.transform.Rotate(0, 0, 180);
            treadLeft.gameObject.transform.localPosition = new Vector3(hullBounds.min.x, 0);
            treadRight.gameObject.transform.localPosition = new Vector3(hullBounds.max.x, 0);
        }
    }

    void AddPlayerPlaneBehaviors(GameObject go, VehicleType vehicle)
    {
        var behaviors = new CompositeBehavior();
        behaviors.Add(new PlayerInput(vehicle.maxSpeed * 10000, vehicle.acceleration));
        behaviors.Add(ActorBehaviorFactory.Instance.faceForward);
        behaviors.Add(ActorBehaviorFactory.Instance.faceMouseOnFire);
        behaviors.Add(ActorBehaviorFactory.Instance.CreatePlayerfire(
                ActorBehaviorFactory.Instance.CreateAutofire(new RateLimiter(0.5f)))
        );

        go.GetComponent<Actor>().behavior = behaviors;
    }
    void AddPlayerTankBehaviors(TankSpawnHelper tankHelper)
    {
        var bf =ActorBehaviorFactory.Instance;

        // hull
        var behaviors = new CompositeBehavior();
        behaviors.Add(new PlayerInput(tankHelper.hull.maxSpeed * 10000, tankHelper.hull.acceleration));
        behaviors.Add(bf.faceForward);
        behaviors.Add(bf.CreatePlayerfire(bf.CreateAutofire(new RateLimiter(0.5f))));
        behaviors.Add(bf.CreateTankTreadAnimator(tankHelper.treadLeft, tankHelper.treadRight));
        tankHelper.hullGO.GetComponent<Actor>().behavior = behaviors;

        // turret
        tankHelper.turretGO.GetComponent<Actor>().behavior = new CompositeBehavior(
            bf.faceMouse,
            bf.CreatePlayerfire(bf.CreateAutofire(new RateLimiter(0.5f)))
        );

    }
    void InitPlayer(GameObject go, VehicleType vehicle)
    {
        go.name += " player";
        go.transform.localPosition = Vector3.zero;

        player = go;

        Camera.main.GetComponent<CameraFollow>().Target = go;

        GlobalGameEvent.Instance.FirePlayerSpawned(go);
    }

    public void HandleCollision(ContactPoint2D contact)
    {
        var boom = (GameObject)GameObject.Instantiate(Main.Instance.Explosion);
        boom.transform.localPosition = contact.point;

        if (contact.collider.gameObject == Main.Instance.game.player)
        {
            var anim = boom.GetComponent<Animation>();
            anim.Play();
        }

        var go = contact.collider.gameObject;
        if (go.layer == (int)Consts.Layer.MOB)
        {
            --_liveEnemies;
            GameObject.Destroy(go);
        }

        if (_liveEnemies == 0)
        {
            StartNextWave();
        }
    }

}

public sealed class Level
{
    public sealed class Squad
    {
        public readonly string enemyID;
        public readonly int count;

        public Squad(string enemy, int count) { this.enemyID = enemy; this.count = count; }

    }
    public sealed class Wave
    {
        public readonly IList<Squad> squads;

        public Wave(IList<Squad> squads) { this.squads = squads; }
    }

    readonly IList<Wave> waves;
    int nextWave = 0;
    public Level(IList<Wave> waves) { this.waves = waves; }

    public Wave NextWave()
    {
        return nextWave <= waves.Count ? waves[nextWave++] : null;
    }
    public int numWaves { get { return waves.Count; } }
}
