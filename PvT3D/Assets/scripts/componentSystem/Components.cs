using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using kaiGameUtil;
using System;

namespace Components
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
        readonly public float capacity;
        readonly public float dischargeDelay;
        public AutofireCharger(string name, float capacity, float rate) : base(name)
        {
            this.capacity = capacity;
            this.dischargeDelay = 1 / Mathf.Max(rate, float.MinValue);
        }
        public Emitter output { set; private get; }

        class PowerState
        {
            public readonly float power;
            public readonly float startTime;

            public PowerState(float power, float startTime) { this.power = power; this.startTime = startTime; }
        }
        PowerState _state;
        public void StartCharging(float power)
        {
            _state = new PowerState(power, Time.fixedTime);
        }
        public void Discharge()
        {
            _state = null;
        }
        public void OnFixedUpdate()
        {
            if (output != null && _state != null && (Time.fixedTime - _state.startTime) >= dischargeDelay)
            {
                var charge = Mathf.Min((Time.fixedTime - _state.startTime) * _state.power, capacity);
                output.EmitPower(charge);

                _state = new PowerState(_state.power, _state.startTime + dischargeDelay);
            }
        }
    }
    class Emitter : SimpleComponent
    {
        public Action<float> PowerEmitted = delegate { };
        public Emitter(string name) : base(name) { }
        public void EmitPower(float power)
        {
            PowerEmitted(power);
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
