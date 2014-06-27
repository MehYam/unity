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

    public GameController(Loader loader)
    {
        Debug.Log("GameState constructor " + GetHashCode());

        this.loader = loader;
        this.effects = new Effects(loader);

        GlobalGameEvent.Instance.MapReady += OnMapReady;
        GlobalGameEvent.Instance.HerolingAttached += OnHerolingAttached;
        GlobalGameEvent.Instance.HerolingDetached += OnHerolingDetached;
        GlobalGameEvent.Instance.PossessionContact += OnHeroTouchedPossessed;
        GlobalGameEvent.Instance.ActorDeath += OnActorDeath;
    }

    //KAI: some nice way to mark this as dev only?
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

    public XRect WorldBounds { get; private set; }
    void OnMapReady(TileMap map, XRect bounds)
    {
        GlobalGameEvent.Instance.MapReady -= OnMapReady;
        WorldBounds = bounds;

        Start();
    }

    void Start()
    {
        SpawnPlayer(Vector3.zero);
        StartNextLevel();
    }
    void SpawnPlayer(Vector3 location)
    {
        var main = Main.Instance;
        Debug.Log("Spawning player " + main.defaultVehicle);

        var tank = loader.GetTank(main.defaultVehicle);
        if (tank == null)
        {
            var playerVehicle = loader.GetVehicle(main.defaultVehicle);
            var player = playerVehicle.Spawn();
            InitPlayer(player, playerVehicle);
            SetPlayerPlaneBehaviors(player, playerVehicle);
        }
        else
        {
            var tankHelper = new TankSpawnHelper(this, tank.hullName, tank.turretName);
            InitPlayer(tankHelper.hullGO, tankHelper.hull);
            SetPlayerTankBehaviors(tankHelper);
        }

        this.player.gameObject.transform.position = location;
    }
    void StartNextLevel()
    {
        StartNextWave();
    }

    int _liveEnemies = 0;
    void StartNextWave()
    {
        if (Main.Instance.runWaves)
        {
            var wave = loader.levels[Main.Instance.startWave].NextWave();
            foreach (var squad in wave.squads)
            {
                for (int i = 0; i < squad.count; ++i)
                {
                    SpawnMob(squad.enemyID);
                    ++_liveEnemies;
                }
            }
        }
    }

    void SpawnMob(string vehicleKey)
    {
        var tank = loader.GetTank(vehicleKey);
        if (tank != null)
        {
            var bf = ActorBehaviorFactory.Instance;
            var tankHelper = new TankSpawnHelper(this, tank.hullName, tank.turretName);

            var hullBehavior = new CompositeBehavior(
                bf.CreateTankTreadAnimator(tankHelper.treadLeft, tankHelper.treadRight),
                bf.CreateRoam(Consts.MAX_MOB_HULL_ROTATION_DEG_PER_SEC, true),
                bf.CreateAutofire(new RateLimiter(0, 4), Consts.Layer.MOB_AMMO)
            );

            var turretFireBehavior = new SequencedBehavior();
            turretFireBehavior.Add(bf.CreateAutofire(new RateLimiter(1), Consts.Layer.MOB_AMMO), new RateLimiter(3, 3));
            turretFireBehavior.Add(null, new RateLimiter(3, 3));
            
            var turretBehavior = new CompositeBehavior();
            turretBehavior.Add(bf.CreateRotateToPlayer(Consts.MAX_MOB_TURRET_ROTATION_DEG_PER_SEC));
            turretBehavior.Add(turretFireBehavior);

            SpawnMobHelper(tankHelper.hullGO);

            tankHelper.hullGO.GetComponent<Actor>().behavior = hullBehavior;
            tankHelper.turretGO.GetComponent<Actor>().behavior = turretBehavior;
        }
        else
        {
            var vehicle = loader.GetVehicle(vehicleKey);
            var go = vehicle.Spawn();
            go.GetComponent<Actor>().behavior = ActorBehaviorScripts.Instance.Get(vehicleKey);

            SpawnMobHelper(go);
        }
    }
    void SpawnMobHelper(GameObject go)
    {
        go.name += " mob";

        // put the actor at the edge
        Vector3 spawnLocation;
        var bounds = new XRect(WorldBounds);
        bounds.Inflate(-1);

        if (Util.CoinFlip())
        {
            spawnLocation = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Util.CoinFlip() ? bounds.min.y : bounds.max.y);
        }
        else
        {
            spawnLocation = new Vector3(Util.CoinFlip() ? bounds.min.x : bounds.max.x, Random.Range(bounds.min.y, bounds.max.y));
        }
        go.transform.localPosition = spawnLocation;
        go.layer = (int)Consts.Layer.MOB;
    }

    public GameObject SpawnAmmo(Actor launcher, VehicleType type, WorldObjectType.Weapon weapon, Consts.Layer layer)
    {
        var go = type.Spawn();
        go.transform.parent = Main.Instance.AmmoParent.transform;
        go.layer = (int)layer;

        var body = go.GetComponent<Rigidbody2D>();
        body.drag = 0;

        var sprite = go.GetComponent<SpriteRenderer>();
#if DEBUG_AMMO
        sprite.sortingOrder = 10;
#else
        sprite.sortingOrder = Consts.SORT_ORDER_AMMO;
#endif
        var actor = go.GetComponent<Actor>();
        actor.SetExpiry(2);
        actor.collisionDamage = weapon.damage;
        Util.Sneeze(launcher.transform, actor.transform, weapon.offset, weapon.angle);
        if (type.acceleration == 0)
        {
            // give the ammo instant acceleration
            body.mass = 0;
            body.velocity = Util.GetLookAtVector(actor.transform.rotation.eulerAngles.z, type.maxSpeed);
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
                sprite.sortingOrder = Consts.SORT_ORDER_HEROLING;

                // give it a push
                body.velocity =
                    launcher.rigidbody2D.velocity + 
                    Util.GetLookAtVector(actor.transform.rotation.eulerAngles.z, type.maxSpeed);
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

        AudioSource.PlayClipAtPoint(Main.Instance.sounds.Bullet, launcher.transform.position);
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

                AudioSource.PlayClipAtPoint(Main.Instance.sounds.HerolingCapture, host.transform.position);
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
    void OnHeroTouchedPossessed(Actor host)
    {
        host.StartCoroutine(RunHostPossessionAnimation(host));
    }

    IEnumerator RunHostPossessionAnimation(Actor host)
    {
        var clipLength = Main.Instance.sounds.fanfare1.length * 1.25f;
        AudioSource.PlayClipAtPoint(Main.Instance.sounds.fanfare1, player.transform.position, 0.25f);

        // 1. Stop all activity and pause
        var timeScale = Time.timeScale;
        Time.timeScale = 0;

        // 2. Remove physics from the hero, pause for a minute
        Util.DisablePhysics(player);

        // 3. Tween it to the host
        var start = player.transform.position;
        var endTime = Time.realtimeSinceStartup + clipLength;
        var sprite = player.GetComponent<SpriteRenderer>();
        while (Time.realtimeSinceStartup < endTime)
        {
            var progress = 1 - (endTime - Time.realtimeSinceStartup) / clipLength;
            var lerped = Vector3.Lerp(start, host.transform.position, progress);
            player.transform.position = lerped;

            var color = sprite.color;
            color.a = 1 - progress;
            sprite.color = color;

            yield return new WaitForEndOfFrame();
        }

        // 4. Host becomes the new player
        var oldHero = player;
        player = host.gameObject;
        var vehicle = player.GetComponent<Actor>().worldObject as VehicleType;

        InitPlayer(player, vehicle);
        if (vehicle is TankHullType)
        {
            var reconstructedHelper = new TankSpawnHelper(player);
            SetPlayerTankBehaviors(reconstructedHelper);
        }
        else
        {
            SetPlayerPlaneBehaviors(player, vehicle);
        }

        // 5. Destroy the old hero and return the herolings
        GameObject.Destroy(oldHero);
        DecrementEnemies();
        HerolingActor.RemoveAll();

        // 6. Resume all activity
        Time.timeScale = timeScale;

        yield return null;
    }
    IEnumerator RunHostDepossessionAnimation()
    {
        yield return new WaitForSeconds(0.3f);

        // 1. Stop all activity and pause
        var timeScale = Time.timeScale;
        Time.timeScale = 0;

        // 2. Spin hero fast to a stop
        var start = Time.realtimeSinceStartup;
        var lastSpin = start;
        float elapsed = 0;
        do
        {
            var now = Time.realtimeSinceStartup;
            elapsed = now - start;

            var pctDone = elapsed / Consts.DEPOSSESSION_DURATION;
            var rotationsPerSec = Consts.DEPOSSESSION_ROTATIONS_PER_SEC * (1 - pctDone);

            Debug.Log(rotationsPerSec);

            player.transform.Rotate(0, 0, (now - lastSpin) * 360 * rotationsPerSec);
            lastSpin = now;

            yield return new WaitForEndOfFrame();
        }
        while (elapsed < Consts.DEPOSSESSION_DURATION);

        // 3. Resume all activity
        Time.timeScale = timeScale;
    }

    void SpawnMuzzleFlash(Actor launcher)
    {
        var flash = effects.GetRandomMuzzleFlash().ToRawGameObject();
        flash.transform.position = launcher.transform.position;
        flash.transform.rotation = launcher.transform.rotation;
    }

    sealed class TankSpawnHelper
    {
        const string HULL_NAME = "hull";
        const string TURRET_NAME = "turret";
        const string LEFT_TREAD_NAME = "treadLeft";
        const string RIGHT_TREAD_NAME = "treadRight";

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

            hullGO.name = HULL_NAME;
            turretGO.name = TURRET_NAME;

            treadLeft = tread.ToRawGameObject();
            treadRight = tread.ToRawGameObject();
            treadLeft.name = LEFT_TREAD_NAME;
            treadRight.name = RIGHT_TREAD_NAME;

            turretGO.transform.parent = hullGO.transform;
            treadLeft.transform.parent = hullGO.transform;
            treadRight.transform.parent = hullGO.transform;

            var hullSprite = hullGO.GetComponent<SpriteRenderer>();
            hullSprite.sortingOrder = Consts.SORT_ORDER_TANK_HULL;

            var hullBounds = hullSprite.sprite.bounds;
            var pivotY = hullBounds.min.y + hull.turretPivotY / Consts.PixelsToUnits;

            turretGO.GetComponent<SpriteRenderer>().sortingOrder = -1;
            turretGO.gameObject.transform.localPosition = new Vector3(0, pivotY);

            treadLeft.gameObject.transform.Rotate(0, 0, 180);
            treadRight.gameObject.transform.Rotate(0, 0, 180);
            treadLeft.gameObject.transform.localPosition = new Vector3(hullBounds.min.x, 0);
            treadRight.gameObject.transform.localPosition = new Vector3(hullBounds.max.x, 0);
        }

        //KAI: cheesy - if we had a proper TankActor type, we wouldn't need this...  all this type-aversion is making for scrambled code
        /// <summary>
        /// Re-constructs the spawn helper for an already-spawned tank, useful for the possession case
        /// </summary>
        /// <param name="gameObject">The already spawned tank</param>
        public TankSpawnHelper(GameObject gameObject)
        {
            this.hullGO = gameObject;
            this.turretGO = gameObject.transform.FindChild(TURRET_NAME).gameObject;
            this.treadLeft = gameObject.transform.FindChild(LEFT_TREAD_NAME).gameObject;
            this.treadRight = gameObject.transform.FindChild(RIGHT_TREAD_NAME).gameObject;

            this.hull = (TankHullType)hullGO.GetComponent<Actor>().worldObject;
            this.turret = (TankPartType)turretGO.GetComponent<Actor>().worldObject;
        }
    }

    void SetPlayerPlaneBehaviors(GameObject go, VehicleType vehicle)
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
            var autoFire = bf.CreateAutofire(new RateLimiter(0.5f), layer);
            behaviors.Add(bf.OnFire(
                new CompositeBehavior(
                    bf.faceMouse,
                    autoFire
                ),
                autoFire
            ));
            behaviors.Add(bf.heroRegen);
        }

        var actor = go.GetComponent<Actor>();
        actor.behavior = behaviors;
    }
    void SetPlayerTankBehaviors(TankSpawnHelper tankHelper)
    {
        var bf =ActorBehaviorFactory.Instance;

        // hull
        var behaviors = new CompositeBehavior();
        behaviors.Add(new PlayerInput());
        behaviors.Add(bf.faceForward);

        var hullFire = bf.CreateAutofire(new RateLimiter(0.5f), Consts.Layer.FRIENDLY_AMMO);
        behaviors.Add(bf.OnFire(hullFire, hullFire));
        behaviors.Add(bf.CreateTankTreadAnimator(tankHelper.treadLeft, tankHelper.treadRight));
        tankHelper.hullGO.GetComponent<Actor>().behavior = behaviors;

        // turret
        var turretFire = bf.CreateAutofire(new RateLimiter(0.5f), Consts.Layer.FRIENDLY_AMMO);
        tankHelper.turretGO.GetComponent<Actor>().behavior = new CompositeBehavior(
            bf.faceMouse,
            bf.OnFire(turretFire, turretFire)
        );

    }
    void InitPlayer(GameObject go, VehicleType vehicle)
    {
        go.name += " player";
        go.layer = (int)Consts.Layer.FRIENDLY;
        go.AddComponent<AudioListener>();

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
            AudioSource.PlayClipAtPoint(Main.Instance.sounds.Explosion1, asplode.transform.position);
            if (enemy)
            {
                DecrementEnemies();
            }
        }
        if (enemy)
        {
            AudioSource.PlayClipAtPoint(Main.Instance.sounds.Explosion1, actor.gameObject.transform.position);
        }

        var wasPlayer = actor.gameObject == player;
        var wasHero = actor.worldObject.name == "HERO";
        var deathPos = actor.gameObject.transform.position;

        GameObject.Destroy(actor.gameObject);
        if (wasPlayer)
        {
            if (wasHero)
            {
                GlobalGameEvent.Instance.FireCenterPrint("Game Over");
                GlobalGameEvent.Instance.FireGameOver();
            }
            else
            {
                SpawnPlayer(deathPos);

                var playerActor = player.GetComponent<Actor>();
                playerActor.StartCoroutine(RunHostDepossessionAnimation());
                playerActor.GrantInvuln(Consts.POST_DEPOSSESSION_INVULN);
            }
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
        return nextWave < waves.Count ? waves[nextWave++] : null;
    }
    public int numWaves { get { return waves.Count; } }
}
