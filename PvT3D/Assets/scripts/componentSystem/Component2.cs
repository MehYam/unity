using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using kaiGameUtil;

namespace PvT3D.ShipComponent2
{
    abstract class Component
    {
        public readonly string name;
        public Component(string name) { this.name = name; }
        public override string ToString()
        {
            return name;
        }
    }
    interface IConnectable
    {
        void Connect(Component source)
    }
    interface ISwitchable
    {
        Action Activated { get; }
        Action Deactivated { get; }

        void Activate();
        void Deactivate();
    }
    interface IFrameHandler
    {
        Action FrameUpdated { get; }
        void FrameUpdate();
    }
    class Power : Component, ISwitchable, IFrameHandler
    {
        public readonly float power;
        public Power(float power) : base("Power")
        {
            this.power = power;
            Activated = delegate { };
            Deactivated = delegate { };
            FrameUpdated = delegate { };
        }
        public Action Activated { get; private set; }
        public Action Deactivated { get; private set; }
        public Action FrameUpdated { get; private set; }
        public void Activate()
        {
            Activated();
        }
        public void Deactivate()
        {
            Deactivated();
        }
        public void FrameUpdate()
        {
            FrameUpdated();
        }
    }
    //KAI: left off here........................................
    //
    // I'm still not sure this provides value.  Switching back to master branch for now
    class Charger : Component, ISwitchable, IFrameHandler
    {
        public Action ChargeChanged = delegate { };
        public Action ChargeFull = delegate { };
        public Action ChargeEmitted = delegate { };

        public Charger() : base("Charger") { }
        public override void Connect(Component source)
        {
            if (source is Switch)
            {
                var controls = source as Switch;
                controls.Activated += Charge;  //KAI: this might be wrong
                controls.Deactivated += Discharge;  //KAI: this might be wrong
                controls.FixedUpdate += OnFixedUpdate;
            }
        }
        public float charge { get; set; }
        public void Charge()
        {
        }
        public void Discharge()
        {
        }
        void OnFixedUpdate()
        {
        }
    }
    abstract class AmmoEmitter : Component
    {
        public Action<Ammo> AmmoEmitted = delegate { };
        public AmmoEmitter(string name) : base(name) { }
    }
    class PlasmaConvertor : AmmoEmitter
    {
        public PlasmaConvertor() : base("PlasmaConvertor") { }
        public override void Connect(Component source)
        {
            if (source is Charger)
            {
                var charger = source as Charger;
                charger.ChargeEmitted += OnChargeEmitted;
            }
        }
        void OnChargeEmitted()
        {
            var ammo = new PlasmaAmmo();
            AmmoEmitted(ammo);
        }
    }
    class LaserConvertor : AmmoEmitter
    {
        public LaserConvertor() : base("PlasmaConvertor") { }
        public override void Connect(Component source)
        {
            if (source is Charger)
            {
                var charger = source as Charger;
                charger.ChargeEmitted += OnChargeEmitted;
            }
        }
        void OnChargeEmitted()
        {
            var ammo = new LaserAmmo();
            AmmoEmitted(ammo);
        }
    }
    class ShieldConvertor : AmmoEmitter
    {
        public ShieldConvertor() : base("ShieldConvertor") { }

        Charger charger;
        public override void Connect(Component source)
        {
            charger = source as Charger;
            if (charger != null)
            {
                var charger = source as Charger;
                charger.ChargeChanged += OnChargeChanged;
                charger.ChargeEmitted += OnChargeEmitted;
            }
        }
        public void TakeDamage(float damage)
        {
            charger.charge -= damage;
            if (charger.charge <= 0)
            {
                charger.Discharge();
            }
        }
        void OnChargeChanged()
        {
        }
        void OnChargeEmitted()
        {
        }
    }
    class Autofire : Component
    {
        public Autofire() : base("Autofire") { }

        Charger charger;
        public override void Connect(Component source)
        {
            charger = source as Charger;
            if (charger != null)
            {
                var charger = source as Charger;
                charger.ChargeFull += OnChargeFull;
            }
        }
        void OnChargeFull()
        {
            charger.Discharge();
            charger.Charge();
        }
    }
    class LifetimeModifier : AmmoEmitter
    {
        public LifetimeModifier() : base("LifetimeModifier") { }
        public override void Connect(Component source)
        {
            if (source is AmmoEmitter)
            {
                var emitter = source as AmmoEmitter;
                emitter.AmmoEmitted += OnAmmoEmitted;
            }
        }
        void OnAmmoEmitted(Ammo ammo)
        {
            ammo.duration = 2112;
            AmmoEmitted(ammo);
        }
    }
    class SpeedModifier : AmmoEmitter
    {
        public SpeedModifier() : base("SpeedModifier") { }
        public override void Connect(Component source)
        {
            if (source is AmmoEmitter)
            {
                var emitter = source as AmmoEmitter;
                emitter.AmmoEmitted += OnAmmoEmitted;
            }
        }
        void OnAmmoEmitted(Ammo ammo)
        {
            var projectile = ammo as ProjectileAmmo;
            if (projectile != null)
            {
                projectile.speed = 2112;
            }
        }
    }

    class Schematic
    {
        public readonly Layer<Component> grid;
        public Schematic(int width, int height)
        {
            grid = new Layer<Component>(width, height);
        }
        public override string ToString()
        {
            return grid.ToString();
        }
    }
}
