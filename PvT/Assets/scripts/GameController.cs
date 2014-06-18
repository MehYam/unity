//#define DEBUG_AMMO

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using PvT.Util;

public sealed class GameController
{
    public GameObject player { get; private set; }
    public Loader loader { get; private set; }
    public Effects effects { get; private set; }
    public GameObject currentlyPossessed { get; private set; }

    readonly GameObject ammoParent;
    public GameController(Loader loader)
    {
        Debug.Log("GameState constructor " + GetHashCode());

        this.loader = loader;
        this.effects = new Effects(loader);
        this.ammoParent = GameObject.Find("_ammoParent");

        GlobalGameEvent.Instance.MapReady += OnMapReady;
        GlobalGameEvent.Instance.HerolingAttached += OnHerolingAttached;
        GlobalGameEvent.Instance.HerolingDetached += OnHerolingDetached;
        GlobalGameEvent.Instance.PossessionContact += OnPossessionContact;
        GlobalGameEvent.Instance.ActorDeath += OnActorDeath;
    }

    //KAI: some nice way to mark this as dev only?
    public void Debug_Respawn(Loader loader)
    {
        if (player != null)
        {
            GameObject.Destroy(player);
        }
        this.loader = loader;
        SpawnPlayer();
    }

    public XRect WorldBounds { get; private set; }
    void OnMapReady(TileMap map, XRect bounds)
    {
        GlobalGameEvent.Instance.MapReady -= OnMapReady;
        WorldBounds = bounds;

        Start();
    }

    void Start()
    {
        SpawnPlayer();
        StartNextLevel();
    }
    void SpawnPlayer()
    {
        var main = Main.Instance;
        if (main.defaultIsPlane)
        {
            Debug.Log("spawning " + main.defaultPlane);
            var playerVehicle = loader.GetVehicle(main.defaultPlane);
            var player = playerVehicle.Spawn();
            InitPlayer(player, playerVehicle);
            AddPlayerPlaneBehaviors(player, playerVehicle);
        }
        else
        {
            var tankHelper = new TankSpawnHelper(this, main.defaultTank, main.defaultTurret);
            InitPlayer(tankHelper.hullGO, tankHelper.hull);
            AddPlayerTankBehaviors(tankHelper);
        }

        this.player.gameObject.transform.localPosition = Vector3.zero;
    }
    void StartNextLevel()
    {
        StartNextWave();
    }

    int _liveEnemies = 0;
    void StartNextWave()
    {
        if (Main.Instance.wavesActive)
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
    }

    void SpawnMob(VehicleType vehicle)
    {
        var go = vehicle.Spawn();
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

    public GameObject SpawnAmmo(Actor launcher, VehicleType type, WorldObjectType.Weapon weapon, Consts.Layer layer)
    {
        var go = type.Spawn();
        go.transform.parent = ammoParent.transform;
        go.layer = (int)layer;

        var body = go.GetComponent<Rigidbody2D>();
        body.drag = 0;

        var sprite = go.GetComponent<SpriteRenderer>();
#if DEBUG_AMMO
        sprite.sortingOrder = 10;
#else
        sprite.sortingOrder = Consts.AMMO_SORT_ORDER;
#endif
        var actor = go.GetComponent<Actor>();
        actor.SetExpiry(2);
        actor.collisionDamage = weapon.damage;
        Consts.Sneeze(launcher.transform, actor.transform, weapon.offset, weapon.angle);
        if (type.acceleration == 0)
        {
            // give the ammo instant acceleration
            body.mass = 0;
            body.velocity = Consts.GetLookAtVector(actor.transform.rotation.eulerAngles.z, type.maxSpeed);
        }
        else
        {
            // treat the ammo like a vehicle (i.e. rocket)
            body.mass = type.mass;
            //KAI: MAJOR CHEESE
            if (weapon.type == "HEROLING")
            {
                actor.SetExpiry(Actor.EXPIRY_INFINITE);

                // make it appear on top of mobs and friendlies
                sprite.sortingOrder = 1;

                // give it a push
                body.velocity =
                    launcher.rigidbody2D.velocity + 
                    Consts.GetLookAtVector(actor.transform.rotation.eulerAngles.z, type.maxSpeed);
            }
            else
            {
                actor.behavior = ActorBehaviorFactory.Instance.thrust;
            }
        }

        if (launcher.worldObject is TankPartType)
        {
            // it's a turret
            SpawnMuzzleFlash(actor);
        }
        return go;
    }

    void OnHerolingAttached(Actor host)
    {
        var herolings = host.GetComponentsInChildren<HerolingActor>();
        if (herolings.Length >= Consts.POSSESSION_THRESHHOLD)
        {
            if (!(host.behavior is BypassedBehavior)) //KAI: wrong, there may be bypassed behaviors not having to do with possession
            {
                // act possessed
                DebugUtil.Assert(!(host.behavior is BypassedBehavior));
                new BypassedBehavior(host, ActorBehaviorFactory.Instance.CreatePossessedBehavior());

                currentlyPossessed = host.gameObject;
            }
        }
    }
    void OnHerolingDetached(Actor host)
    {
        var herolings = host.GetComponentsInChildren<HerolingActor>();
        if (herolings.Length == 0)
        {
            var bypass = host.behavior as BypassedBehavior;
            if (bypass != null)
            {
                bypass.Restore();

                currentlyPossessed = null;
            }
        }
    }
    void OnPossessionContact(Actor host)
    {
        //KAI: might be cleaner to just destroy the enemy, and respawn it as that type, in the same spot
        var oldHero = player;

        player = host.gameObject;
        var vehicle = player.GetComponent<Actor>().worldObject as VehicleType;
        
        InitPlayer(player, vehicle);
        AddPlayerPlaneBehaviors(player, vehicle);

        GameObject.Destroy(oldHero);
    
        DecrementEnemies();

        HerolingActor.ReturnAll();
    }

    void SpawnMuzzleFlash(Actor launcher)
    {
        var flash = effects.GetRandomMuzzleFlash().ToRawGameObject();
        flash.transform.position = launcher.transform.position;
        flash.transform.rotation = launcher.transform.rotation;
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

            hullGO = hull.Spawn();
            turretGO = turret.Spawn();

            treadLeft = tread.ToRawGameObject();
            treadRight = tread.ToRawGameObject();
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
        var bf = ActorBehaviorFactory.Instance;
        var behaviors = new CompositeBehavior();
        behaviors.Add(new PlayerInput(bf.faceForward));

        if (vehicle.weapons[0].type == "SHIELD") //KAI: cheeze
        {
            behaviors.Add(bf.CreateShield());
        }
        else
        {
            //KAI: cheese
            var hero = vehicle.name == "HERO";
            var layer = hero ? Consts.Layer.HEROLINGS : Consts.Layer.FRIENDLY_AMMO;

            if (hero)
            {
                behaviors.Add(bf.CreateHeroAnimator(go));
            }
            behaviors.Add(bf.OnFire(
                new CompositeBehavior(
                    bf.faceMouse,
                    bf.CreateAutofire(new RateLimiter(0.5f), layer)
                )
            ));
        }

        go.GetComponent<Actor>().behavior = behaviors;
    }
    void AddPlayerTankBehaviors(TankSpawnHelper tankHelper)
    {
        var bf =ActorBehaviorFactory.Instance;

        // hull
        var behaviors = new CompositeBehavior();
        behaviors.Add(new PlayerInput());
        behaviors.Add(bf.faceForward);
        behaviors.Add(bf.OnFire(bf.CreateAutofire(new RateLimiter(0.5f), Consts.Layer.FRIENDLY_AMMO)));
        behaviors.Add(bf.CreateTankTreadAnimator(tankHelper.treadLeft, tankHelper.treadRight));
        tankHelper.hullGO.GetComponent<Actor>().behavior = behaviors;

        // turret
        tankHelper.turretGO.GetComponent<Actor>().behavior = new CompositeBehavior(
            bf.faceMouse,
            bf.OnFire(bf.CreateAutofire(new RateLimiter(0.5f), Consts.Layer.FRIENDLY_AMMO))
        );

    }
    void InitPlayer(GameObject go, VehicleType vehicle)
    {
        go.name += " player";
        go.layer = (int)Consts.Layer.FRIENDLY;

        player = go;

        Camera.main.GetComponent<CameraFollow>().Target = go;

        GlobalGameEvent.Instance.FirePlayerSpawned(go);
    }

    void OnActorDeath(Actor actor)
    {
        var enemy = actor.gameObject.layer == (int)Consts.Layer.MOB;
        if (actor.explodesOnDeath && (enemy || actor.gameObject.layer == (int)Consts.Layer.FRIENDLY))
        {
            var asplode = effects.GetVehicleExplosion().ToRawGameObject();
            asplode.transform.position = actor.transform.position;

            if (enemy)
            {
                DecrementEnemies();
            }
        }
        var wasPlayer = actor.gameObject == player;
        GameObject.Destroy(actor.gameObject);

        if (wasPlayer)
        {
            SpawnPlayer();
        }
    }
    void DecrementEnemies()
    {
        --_liveEnemies;
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
