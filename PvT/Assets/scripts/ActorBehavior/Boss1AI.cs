using UnityEngine;
using System.Collections;

using PvT.DOM;
using PvT.Util;

public sealed class Boss1AI : MonoBehaviour
{
    void Start()
    {
        gameObject.AddComponent<Fader>();

        StartCoroutine(Script());

        //actor.behavior = ActorBehaviorFactory.Instance.CreateAutofire(
        //    Consts.CollisionLayer.MOB_AMMO,
        //    actor.actorType.weapons
        //);dwww
        //actor.behavior = ActorBehaviorFactory.Instance.facePlayer;
    }

    struct Behaviors
    {
        public readonly IActorBehavior LASER_PHASE;
        public readonly IActorBehavior CHARGE_FUSION_PHASE;
        public Behaviors(Actor boss)
        {
            var abf = ActorBehaviorFactory.Instance;
            var lasers = new CompositeBehavior();
            for (var iWeapon = 0; iWeapon < 6; ++iWeapon)
            {
                lasers.Add(new WeaponDischargeBehavior(Consts.CollisionLayer.MOB_AMMO, boss.actorType.weapons[iWeapon]));
            }
            LASER_PHASE = new CompositeBehavior(new PeriodicBehavior(lasers, new RateLimiter(0.3f)), abf.facePlayer);

            CHARGE_FUSION_PHASE = new CompositeBehavior(
                new PeriodicBehavior(
                    new WeaponDischargeBehavior(
                        Consts.CollisionLayer.MOB_AMMO, boss.actorType.weapons[6]), new RateLimiter(0.5f)), 
                abf.facePlayer);
        }
    }
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
            // scatter
            var scatter = Util.GetLookAtVector(UnityEngine.Random.Range(0, 360), 7);

            transform.position = Util.Add(main.game.player.transform.position, scatter);

            // teleport in + earthquake
            Util.LookAt2D(actor.transform, main.game.player.transform);

            fader.Fade(1, 5, false);
            main.game.PlaySound(main.sounds.roar, Camera.main.transform.position);

            main.game.ShakeGround();

            //// shoot
            //yield return new WaitForSeconds(2);
            //actor.behavior = Util.CoinFlip() ? _behaviors.LASER_PHASE : _behaviors.CHARGE_FUSION_PHASE;

            //yield return new WaitForSeconds(5);

            // spawn and drop off a bunch of mobs

            // fade
            actor.behavior = abf.facePlayer;
            fader.Fade(0, 3, false);

            yield return new WaitForSeconds(3);
        }
    }
}
