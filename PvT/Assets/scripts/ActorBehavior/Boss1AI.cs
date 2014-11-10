using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PvT.DOM;
using PvT.Util;

public sealed class Boss1AI : MonoBehaviour
{
    void Start()
    {
        gameObject.AddComponent<Fader>();
        gameObject.layer = (int)Consts.CollisionLayer.BOSS_NO_MOB_COLLISION;

        GetComponent<SpriteRenderer>().sortingOrder = 1; // put us on top of the mobs we'll be carrying

        StartCoroutine(Script());

        GlobalGameEvent.Instance.ActorDeath += OnActorDeath;
    }
    void OnDestroy()
    {
        GlobalGameEvent.Instance.ActorDeath -= OnActorDeath;
    }

    const int FIRE_NODES = 6;
    struct Behaviors
    {
        public readonly IActorBehavior LASER_PHASE;
        public readonly IActorBehavior CHARGE_FUSION_PHASE;
        public Behaviors(Actor boss)
        {
            var abf = ActorBehaviorFactory.Instance;
            var lasers = new CompositeBehavior();
            for (var iWeapon = 0; iWeapon < Boss1AI.FIRE_NODES; ++iWeapon)
            {
                lasers.Add(new WeaponDischargeBehavior(Consts.CollisionLayer.MOB_AMMO, boss.actorType.weapons[iWeapon]));
            }
            LASER_PHASE = new CompositeBehavior(new PeriodicBehavior(lasers, new RateLimiter(0.3f)), abf.facePlayer);

            CHARGE_FUSION_PHASE = new CompositeBehavior(
                new PeriodicBehavior(
                    new WeaponDischargeBehavior(
                        Consts.CollisionLayer.MOB_AMMO, boss.actorType.weapons[FIRE_NODES]), new RateLimiter(0.5f)), 
                abf.facePlayer);
        }
    }

    int _spawnedMobs;
    Behaviors _behaviors;
    IEnumerator Script()
    {
        Debug.Log("Running script for " + name);
        var main = Main.Instance;
        var fader = GetComponent<Fader>();
        var actor = GetComponent<Actor>();
        actor.maxRotationalVelocity = 5;

        var abf = ActorBehaviorFactory.Instance;
        _behaviors = new Behaviors(actor);
        fader.Fade(0, 0, false);

        //KAI: break from the while loop when done
        while (true)
        {
            // wait until any child mobs are all dead
            yield return StartCoroutine(Util.YieldUntil(() => _spawnedMobs == 0));

            // teleport close to player in + earthquake
            var closeToPlayer = Util.GetLookAtVector(UnityEngine.Random.Range(0, 360), 7);

            transform.position = Util.Add(main.game.player.transform.position, closeToPlayer);
            Util.LookAt2D(actor.transform, main.game.player.transform);

            // spawn a bunch of new child mobs
            AttachMobs("GREENK");

            fader.Fade(1, 5, false);

            main.game.PlaySound(main.sounds.roar, Camera.main.transform.position);
            main.game.ShakeGround();

            // start shooting
            yield return new WaitForSeconds(2);
            actor.behavior = Util.CoinFlip() ? _behaviors.LASER_PHASE : _behaviors.CHARGE_FUSION_PHASE;

            yield return new WaitForSeconds(5);

            DetachMobs();

            // fade
            actor.behavior = abf.facePlayer;
            fader.Fade(0, 3, false);
            yield return new WaitForSeconds(3);
        
            transform.position = new Vector2(-20, -20);
        }
    }

    void AttachMobs(string type)
    {
        var game = Main.Instance.game;
        var bossWeapons = GetComponent<Actor>().actorType.weapons;
        for (int i = 0; i < FIRE_NODES; ++i)
        {
            var mob = game.SpawnMob(type);
            mob.tag = Consts.SPAWNED_MOB_TAG;
            mob.AddComponent<AlphaInherit>();
            mob.rigidbody2D.isKinematic = true;

            Util.PrepareLaunch(transform, mob.transform, bossWeapons[i].offset);
            mob.transform.parent = transform;

            var mobActor = mob.GetComponent<Actor>();
            mobActor.pauseBehavior = true;
            mobActor.isCapturable = true;

            MobAI.Instance.AttachAI(mobActor);
            ++_spawnedMobs;
        }
    }
    void DetachMobs()
    {
        var toDetach = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.tag == Consts.SPAWNED_MOB_TAG)
            {
                toDetach.Add(child);
            }
        }
        var newParent = Main.Instance.MobParent.transform;
        foreach (var spawn in toDetach)
        {
            spawn.parent = newParent;
            spawn.rigidbody2D.isKinematic = false;
            spawn.GetComponent<Actor>().pauseBehavior = false;
            spawn.GetComponent<AlphaInherit>().enabled = false;
        }
    }
    void OnActorDeath(Actor actor)
    {
        if (actor.tag == Consts.SPAWNED_MOB_TAG)
        {
            --_spawnedMobs;
            
            DebugUtil.Assert(_spawnedMobs >= 0, "Not keeping spawned mob count correctly");
        }
    }
}
