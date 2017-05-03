using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using kaiGameUtil;
using System;

namespace PvT3D.ShipComponents
{
    /// <summary>
    /// AmmoProduct can be thought of as the object on the assembly line.  It passes from Component to Component, which alter the product
    /// to produce something useful
    /// </summary>
    public class AmmoProduct
    {
        public enum Type { Normal, Laser }
        public Type type = Type.Normal;  //KAI: consider hierarchy instead, so that a method override actually does the launching based on the type

        public float damage = 0;
        public float duration = 0;
        public float speed = 0;

        // used only for Type.Laser at the moment
        public float width = 0;
        public float distance = 0;
    }
    interface IConsumer
    {
        void ConsumeProduct(AmmoProduct product);
    }
    interface IProducer
    {
        IConsumer output { set; }
    }
    interface IFrameHandler
    {
        void OnFixedUpdate();
    }
    interface ICharger
    {
        Power powerSource { set; }
        void Charge();
        void Discharge();
    }
    abstract class SimpleComponent
    {
        public readonly string name;
        public SimpleComponent(string name) { this.name = name; }
    }
    /// <summary>
    /// Power defines damage, and perhaps more things in the future
    /// </summary>
    class Power : SimpleComponent
    {
        public readonly float power;
        public Power(string name, float power) : base(name) { this.power = power; }
    }
    /// <summary>
    /// Chargers define rate of fire, and whether the weapon charges up or autofires
    /// </summary>
    class Charger : SimpleComponent, ICharger, IProducer
    {
        readonly public float capacity;
        public Charger(string name, float capacity) : base(name)
        {
            this.capacity = capacity;
        }
        public Power powerSource { set; private get; }
        public IConsumer output { set; private get; }

        class PowerState
        {
            public readonly float power;
            public readonly float startTime;

            public PowerState(float power, float startTime) { this.power = power;  this.startTime = startTime; }
        }
        PowerState _state;
        public void Charge()
        {
            _state = new PowerState(powerSource.power, Time.fixedTime);
        }
        public void Discharge()
        {
            if (output != null && _state != null)
            {
                var product = new AmmoProduct();
                product.damage = Mathf.Min((Time.fixedTime - _state.startTime) * _state.power, capacity);
                output.ConsumeProduct(product);
            }
            _state = null;
        }
    }
    class AutofireCharger : SimpleComponent, ICharger, IProducer, IFrameHandler
    {
        readonly public float chargeTime;
        public AutofireCharger(string name, float rate) : base(name)
        {
            this.chargeTime = 1 / Mathf.Max(rate, float.MinValue);
        }
        public Power powerSource { set; private get; }
        public IConsumer output { set; private get; }

        float _lastFireTime = 0;
        bool _firing = false;
        public void Charge()
        {
            _firing = true;
        }
        public void Discharge()
        {
            _firing = false;
        }
        public void OnFixedUpdate()
        {
            if (output != null && _firing)
            {
                var elapsed = (Time.fixedTime - _lastFireTime);
                if (elapsed >= chargeTime)
                {
                    var product = new AmmoProduct();
                    product.damage = powerSource.power;
                    output.ConsumeProduct(product);

                    _lastFireTime = Time.fixedTime;
                }
            }
        }
    }
    /// <summary>
    /// Envelope defines time to live for the ammo. Along with speed from the Emitter, this determines the range of the shot
    /// </summary>
    class Envelope : SimpleComponent, IConsumer, IProducer
    {
        public readonly float duration;
        public Envelope(string name, float duration) : base(name)
        {
            this.duration = duration;
        }
        public IConsumer output { set; private get; }
        public void ConsumeProduct(AmmoProduct product)
        {
            product.duration = duration;
            if (output != null)
            {
                output.ConsumeProduct(product);
            }
        }
    }
    /// <summary>
    /// Accelerator gives the ammo speed, and 
    /// </summary>
    class Accelerator : SimpleComponent, IConsumer, IProducer
    {
        public readonly float speed;
        public Accelerator(string name, float speed) : base(name) { this.speed = speed; }
        public IConsumer output { set; private get; }
        public void ConsumeProduct(AmmoProduct product)
        {
            product.speed = speed;
            if (output != null)
            {
                output.ConsumeProduct(product);
            }
        }
    }
    class LaserAccelerator : SimpleComponent, IConsumer, IProducer
    {
        public readonly float width;
        public readonly float distance;
        public LaserAccelerator(string name, float distance, float width) : base(name) { this.width = width; this.distance = distance; }
        public IConsumer output { set; private get; }
        public void ConsumeProduct(AmmoProduct product)
        {
            product.type = AmmoProduct.Type.Laser;
            product.width = width;
            product.distance = distance;
            if (output != null)
            {
                output.ConsumeProduct(product);
            }
        }
    }
    class Schematic
    {
        public readonly Layer<SimpleComponent> grid;
        public Schematic(int width, int height)
        {
            grid = new Layer<SimpleComponent>(width, height);
        }
        public override string ToString()
        {
            return grid.ToString();
        }
    }
}
