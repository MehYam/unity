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
    public bool enemyInPossession { get; private set; }
    public Loader loader { get; private set; }
    public Effects effects { get; private set; }

    public GameController(Loader loader)
    {
        Debug.Log("GameState constructor " + GetHashCode());

        this.loader = loader;
        this.effects = new Effects(loader);

        var gge = GlobalGameEvent.Instance;
        gge.CollisionWithOverwhelmed += OnCollisionWithOverwhelmed;
        gge.ActorDeath += OnActorDeath;
        gge.MapReady += OnMapReady;
        gge.AmmoSpawned += OnAmmoSpawned;
        gge.PlayerDataUpdated += OnPlayerDataUpdated;
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
        _map.GetOrAddComponent<Map>();
    }

    public XRect WorldBounds 
    { 
        get; 
        private set; 
    }

    public void SpawnPlayer(Vector3 location, string actorTypeName = null)
    {
        var main = Main.Instance;

        if (string.IsNullOrEmpty(actorTypeName))
        {
            actorTypeName = String.IsNullOrEmpty(main.defaultVehicle) ? "hero" : main.defaultVehicle;
        }
        Debug.Log("Spawning player --- " + actorTypeName);

        var tank = loader.GetTank(actorTypeName);
        if (tank == null)
        {
            var playerVehicle = loader.GetActorType(actorTypeName);
            var go = playerVehicle.Spawn(Consts.SortingLayer.FRIENDLY, true);
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
        var playerActor = player.GetComponent<Actor>();
        SetSecondaryHerolingBehavior((CompositeBehavior)playerActor.behavior);

        this.player.gameObject.transform.position = location;

        enemyInPossession = !playerActor.isHero;
        if (!enemyInPossession)
        {
            // Hero gets no collisionDamage
            playerActor.collisionDamage = 0;
        }
        GlobalGameEvent.Instance.FirePlayerSpawned(playerActor);
    }
    public void SwapPlayer(string key)
    {
        var oldPlayer = player;

        SpawnPlayer(oldPlayer.transform.position);

        player.transform.rotation = oldPlayer.transform.rotation;
        player.rigidbody2D.velocity = oldPlayer.rigidbody2D.velocity;

        GameObject.Destroy(oldPlayer);
    }

    public GameObject SpawnMob(string vehicleKey)
    {
        var tank = loader.GetTank(vehicleKey);
        if (tank != null)
        {
            var tankHelper = new TankSpawnHelper(this, tank.hullName, tank.turretName);
            SpawnMobHelper(tankHelper.hullGO);
            return tankHelper.hullGO;
        }
        var vehicle = loader.GetActorType(vehicleKey);
        var go = vehicle.Spawn(Consts.SortingLayer.MOB, true);

        SpawnMobHelper(go);
        return go;
    }
    void SpawnMobHelper(GameObject go)
    {
        go.name += " mob";
        go.layer = (int)Consts.CollisionLayer.MOB;
        go.transform.parent = Main.Instance.MobParent.transform;
    }

    public GameObject SpawnAmmo(Actor launcher, ActorType.Weapon weapon, Consts.CollisionLayer layer)
    {
        var type = loader.GetActorType(weapon.actorName);
        var goAmmo = type.Spawn(Consts.SortingLayer.AMMO, true);

        goAmmo.transform.parent = Main.Instance.AmmoParent.transform;
        goAmmo.layer = (int)layer;

        goAmmo.rigidbody2D.drag = 0;

        var actorAmmo = goAmmo.GetComponent<Actor>();
        actorAmmo.SetExpiry(weapon.ttl);
        actorAmmo.collisionDamage = weapon.damage;
        actorAmmo.showsHealthBar = false;
        actorAmmo.isAmmo = true;

        if (weapon.lit)
        {
            var renderer = actorAmmo.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = weapon.color;
            }
        }
        Util.PrepareLaunch(launcher.transform, actorAmmo.transform, weapon.offset, weapon.angle);
        if (type.acceleration == 0)
        {
            // give the ammo instant acceleration
            goAmmo.rigidbody2D.mass = 0;
            goAmmo.rigidbody2D.velocity = Util.GetLookAtVector(actorAmmo.transform.rotation.eulerAngles.z, type.maxSpeed);
        }
        else
        {
            // treat the ammo like a vehicle (i.e. rocket)
            goAmmo.rigidbody2D.mass = type.mass;
            actorAmmo.behavior = ActorBehaviorFactory.Instance.thrust;
        }

        if (launcher.actorType is TankTurretType)
        {
            // it's a turret
            SpawnMuzzleFlash(launcher.gameObject, goAmmo);
        }

        GlobalGameEvent.Instance.FireAmmoSpawned(actorAmmo, weapon);
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
        actor.collisionDamage = weapon.damage * damageMultiplier;
        actor.showsHealthBar = false;

        Util.PrepareLaunch(launcher.transform, actor.transform, weapon.offset, weapon.angle);

        PlaySound(loader.GetWeaponSound(weapon.actorName), launcher.transform.position, Mathf.Max(0.2f, damageMultiplier));

        GlobalGameEvent.Instance.FireAmmoSpawned(actor, weapon);
        return go;
    }

    public void PlaySound(AudioClip clip, Vector2 position, float volume = 1)
    {
        //// play the sound at the Z of the camera
        //var cameraPos = Camera.main.transform.position;
        //var soundPos = new Vector3(position.x, position.y, cameraPos.z);  

        AudioSource.PlayClipAtPoint(clip, position, volume);
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

        PlaySound(Main.Instance.sounds.roar, cameraTransform.position);
        yield return new WaitForSeconds(Main.Instance.sounds.roar.length /2 );

        cameraTransform.parent = null;
        GameObject.Destroy(host.gameObject);
    }

    void OnAmmoSpawned(Actor ammo, ActorType.Weapon weapon)
    {
        PlaySound(loader.GetWeaponSound(weapon.actorName), ammo.transform.position);
    }
    void OnPlayerDataUpdated(PlayerData playerData)
    {
        var actorType = player.GetComponent<Actor>().actorType;
        var upgradeType = PlayerData.Instance.GetTierUpgrade(actorType);

        if (upgradeType != actorType)
        {
            var levelUp = player.GetOrAddComponent<LevelUp>();
            levelUp.levelTo = upgradeType;
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

        var clipLength = Main.Instance.sounds.fanfare1.length * 1.25f;
        PlaySound(Main.Instance.sounds.fanfare1, player.transform.position, 0.25f);

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
        var vehicle = player.GetComponent<Actor>().actorType as ActorType;

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
        return actorType.HasWeapons && actorType.weapons[0].chargeSeconds > 0;
    }
    static bool HasShieldWeapon(ActorType actorType)
    {
        return actorType.HasWeapons && actorType.weapons[0].actorName == "SHIELD"; // KAI: cheese
    }
    void SetPlayerPlaneBehaviors(GameObject go, ActorType vehicle)
    {
        var isHopper = go.GetComponent<HopBehavior>() != null;
        var bf = ActorBehaviorFactory.Instance;
        var behaviors = new CompositeBehavior();
        if (isHopper)
        {
            behaviors.Add(new PlayerHopInput());
        }
        else
        {
            behaviors.Add(new PlayerInput(bf.faceForward));
        }

        var actor = go.GetComponent<Actor>();
        var heroType = Main.Instance.game.loader.GetActorType("HERO");
        var isHero = vehicle == heroType;

        if (isHopper)
        {
            actor.speedModifier = new ActorModifier(1000, 1000); // unlock the speed limit
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
        behaviors.Add(new PlayerInput());
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
    static void InitPlayerVehicle(GameObject player, ActorType vehicle)
    {
        player.name += " player";
        player.layer = (int)Consts.CollisionLayer.FRIENDLY;

        Camera.main.GetComponent<CameraFollow>().Target = player;
    }

    void OnActorDeath(Actor actor)
    {
        var enemy = actor.gameObject.layer == (int)Consts.CollisionLayer.MOB;
        if (actor.explodesOnDeath && (enemy || actor.gameObject.layer == (int)Consts.CollisionLayer.FRIENDLY))
        {
            var asplode = effects.GetVehicleExplosion().ToRawGameObject(Consts.SortingLayer.EXPLOSIONS);
            asplode.transform.position = actor.transform.position;

            PlaySound(Main.Instance.sounds.Explosion1, asplode.transform.position);
        }
        if (enemy)
        {
            PlaySound(Main.Instance.sounds.Explosion1, actor.gameObject.transform.position);
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
