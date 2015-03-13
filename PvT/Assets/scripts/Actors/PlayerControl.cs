using UnityEngine;
using System;
using System.Collections;

using PvT.DOM;

public sealed class PlayerControl : MonoBehaviour
{
    public enum FacingBehavior { FACE_FORWARD, FACE_MOUSE, FACE_MOUSE_ON_FIRE };
    public FacingBehavior Facing = FacingBehavior.FACE_MOUSE_ON_FIRE;
    public bool Moveable = true;

    Actor _actor;
    void OnEnable()
    {
        _actor = GetComponent<Actor>();

        // 1. movement control
        var bf = ActorBehaviorFactory.Instance;
        var behaviors = (CompositeBehavior)_actor.behavior;
        var playerControllable = _actor.GetComponent<PlayerControl>();
        if (playerControllable.Facing == PlayerControl.FacingBehavior.FACE_MOUSE_ON_FIRE)
        {
            behaviors.Add(bf.faceMouseOnFire);
        }
        else if (playerControllable.Facing == PlayerControl.FacingBehavior.FACE_MOUSE)
        {
            behaviors.Add(bf.faceMouse);
        }

        // 2. weapons
        var layer = Consts.CollisionLayer.FRIENDLY_AMMO;

        if (_actor.actorType.HasShieldWeapon)
        {
            var controller = new ShieldWeaponController(Consts.CollisionLayer.FRIENDLY, _actor.actorType.weapons[0]);

            var shieldBehavior = new PlayerButton(
                MasterInput.impl.Primary,
                controller.OnStart,
                new CompositeBehavior(bf.faceMouse, (Action<Actor>)controller.OnFrame).FixedUpdate,
                new CompositeBehavior(bf.faceMouse, (Action<Actor>)controller.OnEnd).FixedUpdate
            );
            behaviors.Add(shieldBehavior);
        }
        else if (_actor.actorType.HasChargeWeapon)
        {
            var controller = new ChargeWeaponController(Consts.CollisionLayer.FRIENDLY_AMMO, _actor.actorType.weapons[0]);

            var chargeBehavior = new PlayerButton(
                MasterInput.impl.Primary,
                controller.OnStart,
                new CompositeBehavior(bf.faceMouse, new GoHomeYouAreDrunkBehavior(), (Action<Actor>)controller.OnFrame).FixedUpdate,
                new CompositeBehavior(bf.faceMouse, (Action<Actor>)controller.OnEnd).FixedUpdate
            );
            behaviors.Add(chargeBehavior);
        }
        else
        {
            behaviors.Add(bf.CreatePlayerButton(MasterInput.impl.Primary, bf.CreateAutofire(layer, _actor.actorType.weapons)));
        }

        // 3. secondary weapons (launch herolings)
        var herolingFire = Main.Instance.game.loader.GetActorType("HERO").weapons;
        var secondaryFire = bf.CreateAutofire(Consts.CollisionLayer.HEROLINGS, herolingFire);
        behaviors.Add(bf.CreatePlayerButton(MasterInput.impl.Secondary, secondaryFire));
    }

    public void FixedUpdate()
    {
        if (_actor != null && Moveable)
        {
            var current = MasterInput.impl.CurrentMovementVector;
            if (current != Vector2.zero)
            {
                if (_actor.thrustEnabled)
                {
                    _actor.AddThrust(current * _actor.attrs.acceleration);
                }
            }
        }
    }
}
