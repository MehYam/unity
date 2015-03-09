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
        gge.CollisionWithOverwhelmed += OnCollisionWithOverwhelmed;
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
    public void SpawnPlayer(Vector2 location, string actorTypeName = null)
    {
        var main = Main.Instance;

        if (string.IsNullOrEmpty(actorTypeName))
        {
            actorTypeName = String.IsNullOrEmpty(main.defaultVehicle) ? "hero" : main.defaultVehicle;
        }

        var tank = loader.GetTank(actorTypeName);
        if (tank == null)
        {
            var playerVehicle = loader.GetActorType(actorTypeName);
            var go = playerVehicle.Spawn(Consts.SortingLayer.FRIENDLY, true);
            SetPlayerPlaneBehaviors(go, playerVehicle);
        }
        else
        {
            var tankHelper = new TankSpawnHelper(this, tank.hullName, tank.turretName);
            SetPlayerTankBehaviors(tankHelper);
        }
        var playerActor = player.GetComponent<Actor>();
        SetSecondaryHerolingBehavior((CompositeBehavior)playerActor.behavior);

        this.player.gameObject.transform.position = location;

        // KAI: commenting this out in branch "designRevamp"
        //enemyInPossession = !playerActor.isHero;
        //if (!enemyInPossession)
        //{
        //    // Hero gets no collisionDamage
        //    playerActor.collisionDamage = 0;
        //}
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
        var tank = loader.GetTank(actorKey);
        if (tank != null)
        {
            var tankHelper = new TankSpawnHelper(this, tank.hullName, tank.turretName);
            retval = tankHelper.hullGO;
        }
        else
        {
            var actor = loader.GetActorType(actorKey);
            retval = actor.Spawn(Consts.SortingLayer.MOB, true);
        }
        retval.AddComponent<Mob>();
        return retval;
    }

    public GameObject SpawnProjectile(Actor launcher, ActorType.Weapon weapon, Consts.CollisionLayer layer)
    {
        //KAI: this is not completely sussed out yet
        var type = loader.GetActorType(weapon.actorName);
        var goAmmo = type.Spawn(Consts.SortingLayer.AMMO, true);
        goAmmo.layer = (int)layer;

        var ammo = goAmmo.GetComponent<Ammo>();
        if (ammo != null)
        {
            ammo.weapon= weapon;
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
        var go = type.Spawn(Consts.SortingLayer.AMMO_TOP, false);

        go.transform.parent = Main.Instance.AmmoParent.transform;
        go.layer = (int)layer;

        var actor = go.GetComponent<Actor>();
        actor.collisionDamage = weapon.attrs.damage * damageMultiplier;
        actor.showsHealthBar = false;

        Util.PrepareLaunch(launcher.transform, actor.transform, weapon.offset, weapon.angle);

        GlobalGameEvent.Instance.FireAmmoSpawned(actor, weapon);
        return go;
    }
    public GameObject SpawnObject(Vector2 point)
    {
        return null;
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

    void OnCollisionWithOverwhelmed(Actor host)
    {
        Debug.Log("Collision with overwhelmed " + host.name);

        var playerActor = player.GetComponent<Actor>();
        if (playerActor.isHero)
        {
            host.StartCoroutine(PossessionSequence(host));
        }
        else
        {
            // eject from the previous possessee
            var prevHost = playerActor;

            SpawnPlayer(playerActor.transform.position);

            prevHost.behavior = null;
            prevHost.gameObject.layer = (int)Consts.CollisionLayer.MOB;

            DebugUtil.Assert(player.GetComponent<Actor>().isHero, "Player must be hero after ejecting from previous possessee");
            OnCollisionWithOverwhelmed(host);
        }
    }

    IEnumerator PossessionSequence(Actor host)
    {
        GlobalGameEvent.Instance.FirePossessionInitiated(host);

        enemyInPossession = true;

        PlaySound(Sounds.GlobalEvent.POSSESSION, player.transform.position);

        // 1. Stop all activity and pause
        var timeScale = Time.timeScale;
        Time.timeScale = 0;

        // 2. Remove physics from the hero, pause for a minute
        Util.DisablePhysics(player);

        // 3. Tween it to the host
        const float clipLength = 2f;
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
        var vehicle = player.GetComponent<Actor>().actorType as ActorType;

        //KAI: broken
        //InitPlayerVehicle(player, vehicle);
        if (vehicle is TankHullType)
        {
            var reconstructedHelper = new TankSpawnHelper(player);
            SetPlayerTankBehaviors(reconstructedHelper);
        }
        else
        {
            SetPlayerPlaneBehaviors(player, vehicle);
        }
        var playerActor = player.GetComponent<Actor>();
        SetSecondaryHerolingBehavior((CompositeBehavior)playerActor.behavior);

        // 5. Destroy the old hero and return the herolings
        GameObject.Destroy(oldHero);
        HerolingActor.ReturnAll();

        // 6. Resume all activity
        Time.timeScale = timeScale;

        host.GrantInvuln(Consts.POST_POSSESSION_INVULN);
        
        GlobalGameEvent.Instance.FirePlayerSpawned(playerActor);
        GlobalGameEvent.Instance.FirePossessionComplete(playerActor);
        GlobalGameEvent.Instance.FireMobDeath(host);
        yield return null;
    }
    IEnumerator DepossessionSequence()
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

        player.GetComponent<Actor>().GrantInvuln(Consts.POST_DEPOSSESSION_INVULN);

        GlobalGameEvent.Instance.FireDepossessionComplete();
    }

    void SpawnMuzzleFlash(GameObject launcher, GameObject firePoint)
    {
        var flash = effects.GetRandomMuzzleFlash().ToRawGameObject(Consts.SortingLayer.EXPLOSIONS);
        flash.transform.position = firePoint.transform.position;
        flash.transform.rotation = firePoint.transform.rotation;
        flash.transform.parent = launcher.transform;
    }

    static bool HasChargeWeapon(ActorType actorType)
    {
        return actorType.HasWeapons && actorType.weapons[0].attrs.chargeSeconds > 0;
    }
    static bool HasShieldWeapon(ActorType actorType)
    {
        return actorType.HasWeapons && actorType.weapons[0].actorName == "SHIELD"; // KAI: cheese
    }
    void SetPlayerPlaneBehaviors(GameObject go, ActorType vehicle)
    {
        var isHopper = go.GetComponent<HopBehavior>() != null;  //KAI: cheese, this means it only works if possesssing the hopping mob
        var bf = ActorBehaviorFactory.Instance;
        var behaviors = new CompositeBehavior();
        if (isHopper)
        {
            behaviors.Add(new PlayerHopInput());
        }

        var actor = go.GetComponent<Actor>();
        var heroType = Main.Instance.game.loader.GetActorType("HERO");
        var isHero = vehicle == heroType;

        if (isHopper)
        {
            actor.AddActorModifier(new ActorAttrs(1000, 1000, 0)); // unlock the speed limit
        }
        else if (HasShieldWeapon(vehicle))
        {
            var controller = new ShieldWeaponController(Consts.CollisionLayer.FRIENDLY, vehicle.weapons[0]);

            var shieldBehavior = new PlayerButton(
                MasterInput.impl.Primary,
                controller.Start,
                new CompositeBehavior(bf.faceMouse, (Action<Actor>)controller.OnFrame).FixedUpdate,
                new CompositeBehavior(bf.faceMouse, (Action<Actor>)controller.Discharge).FixedUpdate
            );
            behaviors.Add(shieldBehavior);
            shieldBehavior = new PlayerButton(
                MasterInput.impl.PrimaryAlt,
                controller.Start,
                controller.OnFrame,
                controller.Discharge
            );
            behaviors.Add(shieldBehavior);
        }
        else if (HasChargeWeapon(vehicle))
        {
            var controller = new ChargeWeaponController(Consts.CollisionLayer.FRIENDLY_AMMO, vehicle.weapons[0]);

            var chargeBehavior = new PlayerButton(
                MasterInput.impl.Primary,
                controller.StartCharge,
                new CompositeBehavior(bf.faceMouse, new GoHomeYouAreDrunkBehavior(), (Action<Actor>)controller.Charge).FixedUpdate,
                new CompositeBehavior(bf.faceMouse, (Action<Actor>)controller.Discharge).FixedUpdate
            );
            behaviors.Add(chargeBehavior);
            chargeBehavior = new PlayerButton(
                MasterInput.impl.PrimaryAlt,
                controller.StartCharge,
                new CompositeBehavior(new GoHomeYouAreDrunkBehavior(), (Action<Actor>)controller.Charge).FixedUpdate,
                controller.Discharge
            );
            behaviors.Add(chargeBehavior);
        }
        else
        {
            // set up the primary and secondary fire buttons
            var layer = isHero ? Consts.CollisionLayer.HEROLINGS : Consts.CollisionLayer.FRIENDLY_AMMO;

            var fireAhead = bf.CreateAutofire(layer, vehicle.weapons);
            behaviors.Add(bf.CreatePlayerButton(MasterInput.impl.PrimaryAlt, fireAhead));

            // hero doesn't point to the mouse when firing
            var fireToMouse = isHero ? fireAhead : new CompositeBehavior(bf.faceMouse, fireAhead);
            behaviors.Add(bf.CreatePlayerButton(MasterInput.impl.Primary, fireToMouse));
        }

        if (isHero)
        {
            behaviors.Add(bf.CreateHeroAnimator(go));
            behaviors.Add(bf.heroRegen);
        }

        actor.behavior = behaviors;
    }
    void SetPlayerTankBehaviors(TankSpawnHelper tankHelper)
    {
        var bf =ActorBehaviorFactory.Instance;

        if (HasChargeWeapon(tankHelper.hull) || HasChargeWeapon(tankHelper.turret))
        {
            Debug.LogWarning("Charge weapons not currently supported on tanks");
        }

        // hull
        var behaviors = new CompositeBehavior();
        behaviors.Add(bf.faceForward);

        if (tankHelper.hull.HasWeapons)
        {
            var hullFire = bf.CreateAutofire(Consts.CollisionLayer.FRIENDLY_AMMO, tankHelper.hull.weapons);
            behaviors.Add(bf.CreatePlayerButton(MasterInput.impl.Primary, hullFire));
            behaviors.Add(bf.CreatePlayerButton(MasterInput.impl.PrimaryAlt, hullFire));
        }
        behaviors.Add(bf.CreateTankTreadAnimator(tankHelper.treadLeft, tankHelper.treadRight));
        tankHelper.hullGO.GetComponent<Actor>().behavior = behaviors;

        // turret
        var turretFire = bf.CreateAutofire(Consts.CollisionLayer.FRIENDLY_AMMO, tankHelper.turret.weapons);
        tankHelper.turretGO.GetComponent<Actor>().behavior = new CompositeBehavior(
            bf.faceMouse,
            bf.CreatePlayerButton(MasterInput.impl.Primary, turretFire),
            bf.CreatePlayerButton(MasterInput.impl.PrimaryAlt, turretFire)
        );
    }
    void SetSecondaryHerolingBehavior(CompositeBehavior behaviors)
    {
        var bf = ActorBehaviorFactory.Instance;

        var herolingFire = Main.Instance.game.loader.GetActorType("HERO").weapons;

        // captured ship, add herolings to secondary fire
        var secondaryFire = bf.CreateAutofire(Consts.CollisionLayer.HEROLINGS, herolingFire);
        behaviors.Add(bf.CreatePlayerButton(MasterInput.impl.Secondary, secondaryFire));
    }

    void OnActorDeath(Actor actor)
    {
        var enemy = actor.gameObject.layer == (int)Consts.CollisionLayer.MOB;
        if (actor.explodesOnDeath && (enemy || actor.gameObject.layer == (int)Consts.CollisionLayer.FRIENDLY))
        {
            var asplode = effects.GetVehicleExplosion().ToRawGameObject(Consts.SortingLayer.EXPLOSIONS);
            asplode.transform.position = actor.transform.position;

            PlaySound(Sounds.GlobalEvent.EXPLOSION, asplode.transform.position);
        }
        if (enemy)
        {
            GlobalGameEvent.Instance.FireMobDeath(actor);
        }

        var wasPlayer = actor.gameObject == player;
        var wasHero = actor.isHero;
        var deathPos = actor.gameObject.transform.position;

        if (wasPlayer && ! wasHero)
        {
            SpawnPlayer(deathPos);

            var playerActor = player.GetComponent<Actor>();
            playerActor.StartCoroutine(DepossessionSequence());
        }
    }
}
