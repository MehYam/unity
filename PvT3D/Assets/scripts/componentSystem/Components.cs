using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using kaiGameUtil;
using System;

namespace PvT3D.ShipComponent
{
    /// <summary>
    /// ComponentProduct can be thought of as the object on the assembly line.  It passes from Component to Component, which alter the product
    /// to produce something useful
    /// </summary>
    public class ComponentProduct
    {
        public enum Type { Plasma, Laser, Shield }

        public Type  type = Type.Plasma;  //KAI: consider hierarchy instead, so that a method override actually does the launching based on the type
        public float power = 0;
        public float duration = 0;
        public float speed = 0;
        public float width = 0;
        public float distance = 0;

        public bool inheritShipVelocity = false;
    }
    interface IProductProducer
    {
        IProductConsumer productOutput { set; }
    }
    interface IProductConsumer
    {
        void ConsumeProduct(ComponentProduct product);
    }
    interface ILiveProductProducer
    {
        IProductConsumer liveProductOutput { set; }
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
    interface IChargerWrapper: ICharger
    {
        ICharger charger { set; }
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
    class Charger : Component, ICharger, IProductProducer
    {
        readonly public float rate;
        public Charger(string name, float rate) : base(name)
        {
            this.rate = rate;
        }
        public Power powerSource { set; private get; }
        public IProductConsumer productOutput { set; private get; }

        float _startOfCharge = -1;
        public void Charge()
        {
            _startOfCharge = Time.fixedTime;
        }
        public void Discharge()
        {
            if (productOutput != null && _startOfCharge >= 0)
            {
                var product = new ComponentProduct();
                product.power = currentCharge;
                productOutput.ConsumeProduct(product);
            }
            _startOfCharge = 0;
        }
        public float currentCharge
        {
            get
            {
                return _startOfCharge >= 0 ? Mathf.Min((Time.fixedTime - _startOfCharge) * rate, powerSource.power) : 0;
            }
        }
    }
    /// <summary>
    /// Repeater is how we get autofire - it basically connects to the charger and automates it
    /// </summary>
    class Repeater : Component, IChargerWrapper, IFrameHandler
    {
        public Repeater(string name) : base(name) { }

        ICharger _charger;
        public ICharger charger
        {
            set
            {
                _charger = value;

                // the charger is basically always charging, so that the fire button shoots immediate when not pressed for a while
                _charger.Charge();
            }
        }

        public float currentCharge { get {  return _charger.currentCharge; } }
        public Power powerSource { set; private get; }

        public void Charge()
        {
            if (_charger.currentCharge == powerSource.power)
            {
                _charger.Discharge();
            }
            _charger.Charge();
        }
        public void Discharge()
        {
            // leave the actual charger in a charging state by doing nothing.  This
            // will make it so that the next fire button press will fire immediately,
            // given time to charge.  Doing nothing also prevents a small ammo from 
            // being output from leftover charge when the fire button's released
        }
        public void OnFixedUpdate()
        {
            if (_charger != null && powerSource != null)
            {
                if (_charger.currentCharge == powerSource.power)
                {
                    _charger.Discharge();
                    _charger.Charge();
                }
            }
        }
    }
    /// <summary>
    /// Shield is a complicated component - it needs to produce a shield when charging begins, maintain the shield
    /// strength based on how much damage it's taking and how quickly it's being charged, and then launch it
    /// when charging is finished
    /// </summary>
    class Shield : Component, IChargerWrapper, IFrameHandler, IProductProducer, ILiveProductProducer
    {
        public Shield(string name) : base(name)
        {
        }

        public ICharger charger { set; private get;}
        public IProductConsumer productOutput { set; private get; }
        public IProductConsumer liveProductOutput { set; private get; }

        public Power powerSource { set; private get; }
        public float currentCharge
        {
            get
            {
                return charger.currentCharge;
            }
        }
        ComponentProduct _currentProduct;
        public void Charge()
        {
            // create the shield
            if (_currentProduct == null)
            {
                _currentProduct = new ComponentProduct();
                _currentProduct.type = ComponentProduct.Type.Shield;
                _currentProduct.inheritShipVelocity = true;

                UpdateOutput(_currentProduct);
            }
        }
        public void Discharge()
        {
            // release the shield
            productOutput.ConsumeProduct(_currentProduct);
            _currentProduct = null;
        }
        public void OnFixedUpdate()
        {
            // add to the shield's charge
            if (_currentProduct != null)
            {
                UpdateOutput(_currentProduct);
            }
        }
        void UpdateOutput(ComponentProduct product)
        {
            if (liveProductOutput != null)
            {
                liveProductOutput.ConsumeProduct(_currentProduct);
            }
        }
    }
    /// <summary>
    /// Envelope defines time to live for the ammo. Along with speed from the Accelerator, this determines the range of the shot
    /// </summary>
    class Lifetime : Component, IProductConsumer, IProductProducer
    {
        public readonly float duration;
        public Lifetime(string name, float duration) : base(name)
        {
            this.duration = duration;
        }
        public IProductConsumer productOutput { set; private get; }
        public void ConsumeProduct(ComponentProduct product)
        {
            product.duration = duration;
            if (productOutput != null)
            {
                productOutput.ConsumeProduct(product);
            }
        }
    }
    /// <summary>
    /// Launcher imparts speed to ammo projectile
    /// </summary>
    class Speed : Component, IProductConsumer, IProductProducer
    {
        public readonly float speed;
        public Speed(string name, float speed) : base(name) { this.speed = speed; }
        public IProductConsumer productOutput { set; private get; }
        public void ConsumeProduct(ComponentProduct product)
        {
            product.speed = speed;
            if (productOutput != null)
            {
                productOutput.ConsumeProduct(product);
            }
        }
    }
    class Laser : Component, IProductConsumer, IProductProducer
    {
        public readonly float width;
        public readonly float distance;
        public Laser(string name, float distance, float width) : base(name) { this.width = width; this.distance = distance; }
        public IProductConsumer productOutput { set; private get; }
        public void ConsumeProduct(ComponentProduct product)
        {
            product.type = ComponentProduct.Type.Laser;
            product.width = width;
            product.distance = distance;
            if (productOutput != null)
            {
                productOutput.ConsumeProduct(product);
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
