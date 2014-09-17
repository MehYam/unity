//#define DEBUG_AMMO

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using PvT.Util;

public sealed class GameController : IGame
{
    public GameObject player { get; private set; }
    public bool enemyInPossession    { get { return !player.GetComponent<Actor>().isHero; } }
    public Loader loader { get; private set; }
    public Effects effects { get; private set; }

    public GameController(Loader loader)
    {
        Debug.Log("GameState constructor " + GetHashCode());

        this.loader = loader;
        this.effects = new Effects(loader);

        var gge = GlobalGameEvent.Instance;
        gge.HerolingAttached += OnHerolingAttached;
        gge.HerolingDetached += OnHerolingDetached;
        gge.CollisionWithOverwhelmed += OnCollisionWithOverwhelmed;
        gge.ActorDeath += OnActorDeath;
        gge.MapReady += OnMapReady;
    }

    void OnMapReady(XRect bounds)
    {
        WorldBounds = bounds;

        var border = GameObject.Find("/border");

        border.transform.FindChild("bottom").localPosition = new Vector2(0, bounds.bottom);
        border.transform.FindChild("top").localPosition = new Vector2(0, bounds.top);
        border.transform.FindChild("left").localPosition = new Vector2(bounds.left, 0);
        border.transform.FindChild("right").localPosition = new Vector2(bounds.right, 0);

        GlobalGameEvent.Instance.FireGameReady(this);
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

    GameObject _map;
    public void SetMap(GameObject mapPrefab)
    {
        if (_map != null)
        {
            GameObject.Destroy(_map);
        }
        _map = (GameObject)GameObject.Instantiate(mapPrefab);
        _map.AddComponent<Map>();
    }

    public XRect WorldBounds 
    { 
        get; 
        private set; 
    }

    public GameObject SpawnPlayer(Vector3 location)
    {
        var main = Main.Instance;
        Debug.Log("Spawning player --- " + main.defaultVehicle);

        var tank = loader.GetTank(main.defaultVehicle);
        if (tank == null)
        {
            var playerVehicle = loader.GetVehicle(main.defaultVehicle);
            var go = playerVehicle.Spawn(Consts.SortingLayer.FRIENDLY);
            InitPlayerVehicle(go, playerVehicle);
            SetPlayerPlaneBehaviors(go, playerVehicle);

            this.player = go;
        }
        else
        {
            var tankHelper = new TankSpawnHelper(this, tank.hullName, tank.turretName);
            InitPlayerVehicle(tankHelper.hullGO, tankHelper.hull);
            SetPlayerTankBehaviors(tankHelper);

            this.player = tankHelper.hullGO;
        }
        SetSecondaryHerolingBehavior((CompositeBehavior)this.player.GetComponent<Actor>().behavior);

        this.player.gameObject.transform.position = location;

        GlobalGameEvent.Instance.FirePlayerSpawned(this.player);
        return this.player.gameObject;
    }

    public GameObject SpawnMob(string vehicleKey)
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

            tankHelper.hullGO.GetComponent<Actor>().behavior = hullBehavior;
            tankHelper.turretGO.GetComponent<Actor>().behavior = turretBehavior;

            SpawnMobHelper(tankHelper.hullGO);
            return tankHelper.hullGO;
        }
        var vehicle = loader.GetVehicle(vehicleKey);
        var go = vehicle.Spawn(Consts.SortingLayer.MOB);
        go.GetComponent<Actor>().behavior = ActorBehaviorScripts.Instance.Get(vehicleKey);

        SpawnMobHelper(go);
        return go;
    }
    void SpawnMobHelper(GameObject go)
    {
        go.name += " mob";
        go.layer = (int)Consts.Layer.MOB;
    }

    public GameObject SpawnAmmo(Actor launcher, VehicleType type, WorldObjectType.Weapon weapon, Consts.Layer layer)
    {
#if DEBUG_AMMO
        var go = type.Spawn(Consts.SortingLayer.UI);
#else
        var go = type.Spawn(Consts.SortingLayer.MOB_AMMO);
#endif
        go.transform.parent = Main.Instance.AmmoParent.transform;
        go.layer = (int)layer;

        var body = go.GetComponent<Rigidbody2D>();
        body.drag = 0;

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
            actor.behavior = ActorBehaviorFactory.Instance.thrust;
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
        ++host.attachedHerolings;
    }
    void OnHerolingDetached(Actor host)
    {
        --host.attachedHerolings;
    }

    void OnCollisionWithOverwhelmed(Actor host)
    {
        var playerActor = player.GetComponent<Actor>();
        if (playerActor.isHero)
        {
            host.attachedHerolings = 0;
            host.StartCoroutine(RunPossessionAnimation(host));

            GlobalGameEvent.Instance.FireEnemyDeath();
        }
        else
        {
            // kill the old possessee first
            playerActor.SetExpiry(Actor.EXPIRY_IMMEDIATE);
        }
    }

    IEnumerator RunPossessionAnimation(Actor host)
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

        InitPlayerVehicle(player, vehicle);
        if (vehicle is TankHullType)
        {
            var reconstructedHelper = new TankSpawnHelper(player);
            SetPlayerTankBehaviors(reconstructedHelper);
        }
        else
        {
            SetPlayerPlaneBehaviors(player, vehicle);
        }
        SetSecondaryHerolingBehavior((CompositeBehavior)player.GetComponent<Actor>().behavior);

        // 5. Destroy the old hero and return the herolings
        GameObject.Destroy(oldHero);
        HerolingActor.RemoveAll();

        // 6. Resume all activity
        Time.timeScale = timeScale;

        GlobalGameEvent.Instance.FirePlayerSpawned(this.player);
        yield return null;
    }
    IEnumerator RunDepossessionAnimation()
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

            var pctDone = elapsed / Consts.HEROLING_OVERWHELM_DURATION;
            var rotationsPerSec = Consts.HEROLING_OVERWHELM_ROTATIONS_PER_SEC * (1 - pctDone);

            player.transform.Rotate(0, 0, (now - lastSpin) * 360 * rotationsPerSec);
            lastSpin = now;

            yield return new WaitForEndOfFrame();
        }
        while (elapsed < Consts.HEROLING_OVERWHELM_DURATION);

        // 3. Resume all activity
        Time.timeScale = timeScale;
    }

    void SpawnMuzzleFlash(Actor launcher)
    {
        var flash = effects.GetRandomMuzzleFlash().ToRawGameObject(Consts.SortingLayer.EXPLOSIONS);
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
            var tread = game.loader.GetMisc("tanktreadParent");

            hullGO = hull.Spawn(Consts.SortingLayer.TANKBODY);
            turretGO = turret.Spawn(Consts.SortingLayer.TANKTURRET);

            hullGO.name = HULL_NAME;
            turretGO.name = TURRET_NAME;

            treadLeft = tread.ToRawGameObject(Consts.SortingLayer.TANKTREAD);
            treadRight = tread.ToRawGameObject(Consts.SortingLayer.TANKTREAD);
            treadLeft.name = LEFT_TREAD_NAME;
            treadRight.name = RIGHT_TREAD_NAME;

            turretGO.transform.parent = hullGO.transform;
            treadLeft.transform.parent = hullGO.transform;
            treadRight.transform.parent = hullGO.transform;

            var hullSprite = hullGO.GetComponent<SpriteRenderer>();
            var hullBounds = hullSprite.sprite.bounds;
            var pivotY = hullBounds.min.y + hull.turretPivotY / Consts.PixelsToUnits;
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

        var actor = go.GetComponent<Actor>();
        var heroType = Main.Instance.game.loader.GetVehicle("HERO");
        var isHero = vehicle == heroType;

        if (vehicle.weapons.Length > 0 && vehicle.weapons[0].type == "SHIELD") //KAI: cheese
        {
            behaviors.Add(bf.CreateShield());
        }
        else
        {
            var layer = isHero ? Consts.Layer.HEROLINGS : Consts.Layer.FRIENDLY_AMMO;

            var primaryFire = bf.CreateAutofire(new RateLimiter(0.5f), layer);
            behaviors.Add(bf.OnPlayerInput("Jump", primaryFire));

            var fire1 = isHero ? primaryFire : new CompositeBehavior(bf.faceMouse, primaryFire);
            behaviors.Add(bf.OnPlayerInput("Fire1", fire1));
            if (isHero)
            {
                behaviors.Add(bf.CreateHeroAnimator(go));
            }
            behaviors.Add(bf.heroRegen);
        }

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
    void SetSecondaryHerolingBehavior(CompositeBehavior behaviors)
    {
        var bf = ActorBehaviorFactory.Instance;

        var herolingFire = Main.Instance.game.loader.GetVehicle("HERO").weapons;

        // captured ship, add herolings to secondary fire
        var secondaryFire = bf.CreateAutofire(new RateLimiter(0.5f), Consts.Layer.HEROLINGS, herolingFire);
        behaviors.Add(bf.OnPlayerInput("Fire2", secondaryFire));
    }
    static void InitPlayerVehicle(GameObject player, VehicleType vehicle)
    {
        player.name += " player";
        player.layer = (int)Consts.Layer.FRIENDLY;

        Camera.main.GetComponent<CameraFollow>().Target = player;
    }

    void OnActorDeath(Actor actor)
    {
        var enemy = actor.gameObject.layer == (int)Consts.Layer.MOB;
        if (actor.explodesOnDeath && (enemy || actor.gameObject.layer == (int)Consts.Layer.FRIENDLY))
        {
            var asplode = effects.GetVehicleExplosion().ToRawGameObject(Consts.SortingLayer.EXPLOSIONS);
            asplode.transform.position = actor.transform.position;

            AudioSource.PlayClipAtPoint(Main.Instance.sounds.Explosion1, asplode.transform.position);
        }
        if (enemy)
        {
            AudioSource.PlayClipAtPoint(Main.Instance.sounds.Explosion1, actor.gameObject.transform.position);
            GlobalGameEvent.Instance.FireEnemyDeath();
        }

        var wasPlayer = actor.gameObject == player;
        var wasHero = actor.isHero;
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
                playerActor.StartCoroutine(RunDepossessionAnimation());
                playerActor.GrantInvuln(Consts.POST_DEPOSSESSION_INVULN);
            }
        }
    }
}
