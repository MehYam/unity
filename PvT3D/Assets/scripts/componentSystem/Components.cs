using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using kaiGameUtil;
using System;

namespace PvT3D.WeaponComponents
{
    /// <summary>
    /// AmmoProduct can be thought of as the object on the assembly line.  It passes from Component to Component, which alter the product
    /// to produce something useful
    /// </summary>
    public class AmmoProduct
    {
        public enum Type { Plasma, Laser, Shield }
        public Type type = Type.Plasma;  //KAI: consider hierarchy instead, so that a method override actually does the launching based on the type

        public float power = 0;
        public float duration = 0;
        public float speed = 0;
        public float width = 0;
        public float distance = 0;
    }
    interface IAmmoProductConsumer
    {
        void ConsumeAmmoProduct(AmmoProduct product);
    }
    interface IAmmoProductProducer
    {
        IAmmoProductConsumer output { set; }
    }
    interface IFrameHandler
    {
        void OnFixedUpdate();
    }
    interface ICharger
    {
        Power powerSource { set; }
        float currentCharge { get; }

        void Charge();
        void Discharge();
    }
    abstract class Component
    {
        public readonly string name;
        public Component(string name) { this.name = name; }

        public override string ToString()
        {
            return name;
        }
    }
    /// <summary>
    /// Power defines damage, and perhaps more things in the future
    /// </summary>
    class Power : Component
    {
        public readonly float power;
        public Power(string name, float power) : base(name) { this.power = power; }
    }
    class Charger : Component, ICharger, IAmmoProductProducer
    {
        readonly public float rate;
        public Charger(string name, float rate) : base(name)
        {
            this.rate = rate;
        }
        public Power powerSource { set; private get; }
        public IAmmoProductConsumer output { set; private get; }

        float _startOfCharge = 0;
        public void Charge()
        {
            _startOfCharge = Time.fixedTime;
        }
        public void Discharge()
        {
            if (output != null && _startOfCharge > 0)
            {
                var product = new AmmoProduct();
                product.power = currentCharge;
                output.ConsumeAmmoProduct(product);
            }
            _startOfCharge = 0;
        }
        public float currentCharge
        {
            get
            {
                return _startOfCharge > 0 ? Mathf.Min((Time.fixedTime - _startOfCharge) * rate, powerSource.power) : 0;
            }
        }
    }
    class Autofire : Component, IFrameHandler, ICharger
    {
        public Autofire(string name) : base(name) { }

        public ICharger charger { set; private get; }
        public float currentCharge { get {  return charger.currentCharge; } }
        public Power powerSource { set; private get; }

        public void Charge()
        {
            if (charger.currentCharge == powerSource.power)
            {
                charger.Discharge();
            }
            charger.Charge();
        }
        public void Discharge()
        {
            // leave the actual charger in a charging state by doing nothing.  This
            // will make it so that the next fire button press will fire immediately,
            // assuming there's been enough time for a charge.  It will also
            // prevent a smaller ammo from being output from leftover charge when
            // the fire button's released
        }
        public void OnFixedUpdate()
        {
            if (charger != null && powerSource != null)
            {
                if (charger.currentCharge == powerSource.power)
                {
                    charger.Discharge();
                    charger.Charge();
                }
            }
        }
    }
    /// <summary>
    /// UNFINISHED
    /// </summary>
    class ShieldCharger : Component, ICharger, IAmmoProductProducer, IFrameHandler
    {
        public ShieldCharger(string name, float rate) : base(name)
        {

        }
        public IAmmoProductConsumer output {  set; private get; }
        public Power powerSource {  set; private get; }

        public void Charge()
        {
        }
        public void Discharge()
        {
        }
        public float currentCharge { get {  return 0; } }
        public void OnFixedUpdate()
        {
        }
    }
    /// <summary>
    /// Envelope defines time to live for the ammo. Along with speed from the Accelerator, this determines the range of the shot
    /// </summary>
    class Envelope : Component, IAmmoProductConsumer, IAmmoProductProducer
    {
        public readonly float duration;
        public Envelope(string name, float duration) : base(name)
        {
            this.duration = duration;
        }
        public IAmmoProductConsumer output { set; private get; }
        public void ConsumeAmmoProduct(AmmoProduct product)
        {
            product.duration = duration;
            if (output != null)
            {
                output.ConsumeAmmoProduct(product);
            }
        }
    }
    /// <summary>
    /// Accelerator imparts speed to ammo projectile
    /// </summary>
    class Accelerator : Component, IAmmoProductConsumer, IAmmoProductProducer
    {
        public readonly float speed;
        public Accelerator(string name, float speed) : base(name) { this.speed = speed; }
        public IAmmoProductConsumer output { set; private get; }
        public void ConsumeAmmoProduct(AmmoProduct product)
        {
            product.speed = speed;
            if (output != null)
            {
                output.ConsumeAmmoProduct(product);
            }
        }
    }
    class LaserAccelerator : Component, IAmmoProductConsumer, IAmmoProductProducer
    {
        public readonly float width;
        public readonly float distance;
        public LaserAccelerator(string name, float distance, float width) : base(name) { this.width = width; this.distance = distance; }
        public IAmmoProductConsumer output { set; private get; }
        public void ConsumeAmmoProduct(AmmoProduct product)
        {
            product.type = AmmoProduct.Type.Laser;
            product.width = width;
            product.distance = distance;
            if (output != null)
            {
                output.ConsumeAmmoProduct(product);
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
