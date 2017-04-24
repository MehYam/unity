using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using kaiGameUtil;
using System;

namespace ShipComponents
{
    interface IFrameHandler
    {
        void OnFixedUpdate();
    }
    interface IPowerable
    {
        void PowerOn();
        void PowerOff();
    }
    interface ICharger : IFrameHandler
    {
        void StartCharging(float power);
        void Discharge();

        Emitter output { set; }
    }
    abstract class SimpleComponent
    {
        public readonly string name;
        public SimpleComponent(string name) { this.name = name; }
    }
    class PowerModule : SimpleComponent, IFrameHandler
    {
        readonly float power;
        public PowerModule(string name, float power) : base(name) { this.power = power; }

        public ICharger output { set; private get; }
        public void PowerOn()
        {
            if (output != null)
            {
                output.StartCharging(power);
            }
        }
        public void PowerOff()
        {
            if (output != null)
            {
                output.Discharge();
            }
        }
        public void OnFixedUpdate()
        {
            if (output != null)
            {
                output.OnFixedUpdate();
            }
        }
    }
    class Charger : SimpleComponent, ICharger
    {
        readonly public float capacity;
        public Charger(string name, float capacity) : base(name)
        {
            this.capacity = capacity;
        }
        public Emitter output { set; private get; }

        class PowerState
        {
            public readonly float power;
            public readonly float startTime;

            public PowerState(float power, float startTime) { this.power = power;  this.startTime = startTime; }
        }
        PowerState _state;
        public void StartCharging(float power)
        {
            _state = new PowerState(power, Time.fixedTime);
        }
        public void Discharge()
        {
            if (output != null && _state != null)
            {
                var charge = Mathf.Min((Time.fixedTime - _state.startTime) * _state.power, capacity);

                output.EmitPower(charge);
            }
            _state = null;
        }
        public void OnFixedUpdate()
        {
        }
    }
    class AutofireCharger : SimpleComponent, ICharger
    {
        readonly public float chargeTime;
        public AutofireCharger(string name, float rate) : base(name)
        {
            this.chargeTime = 1 / Mathf.Max(rate, float.MinValue);
        }
        public Emitter output { set; private get; }

        float _lastFireTime = 0;
        float _power = 0;
        public void StartCharging(float power)
        {
            _power = power;
        }
        public void Discharge()
        {
            _power = 0;
        }
        public void OnFixedUpdate()
        {
            if (output != null && _power > 0 && (Time.fixedTime - _lastFireTime) >= chargeTime)
            {
                output.EmitPower(_power);

                _lastFireTime = Time.fixedTime;
            }
        }
    }
    class Emitter : SimpleComponent
    {
        /// <summary>
        ///  PowerEmitted(damage, speed, lifetime)
        /// </summary>
        public Action<float, float, float> AmmoEmitted = delegate { };

        public readonly float speed;
        public readonly float lifetime;
        public Emitter(string name, float speed, float lifetime) : base(name) { this.speed = speed; this.lifetime = lifetime; }
        public void EmitPower(float power)
        {
            AmmoEmitted(power, speed, lifetime);
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
