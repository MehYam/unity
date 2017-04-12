using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using kaiGameUtil;
using System;

namespace Components
{
    interface IFrameHandler
    {
        void OnFixedUpdate(float time, float fixedDeltaTime);
    }
    interface IPowerable
    {
        void PowerOn(float time);
        void PowerOff(float time);
    }
    abstract class Component_fancyTypeHierarchyWeMaybeWant : IFrameHandler, IPowerable
    {
        public readonly string name;
        public Component_fancyTypeHierarchyWeMaybeWant(string name) { this.name = name; }

        public Component_fancyTypeHierarchyWeMaybeWant Output
        {
            set; protected get;
        }
        public override string ToString()
        {
            return name;
        }

        bool powered = false;
        public void PowerOn(float time)
        {
            if (Output != null)
            {
                powered = true;
                Output.PowerOn(time);
            }
        }
        public void PowerOff(float time)
        {
            if (Output != null)
            {
                Output.PowerOff(time);
                powered = false;
            }
        }
        public void OnFixedUpdate(float time, float fixedDeltaTime)
        {
            if (powered && Output != null)
            {
                Output.OnFixedUpdate(time, fixedDeltaTime);
            }
        }

        protected abstract void OnPowerOn();
        protected abstract void OnPowerOff();
        protected abstract void OnFixedFrame();
    }

    abstract class SimpleComponent
    {
        public readonly string name;
        public SimpleComponent(string name) { this.name = name; }
    }
    class PowerModule : SimpleComponent
    {
        readonly float power;
        public PowerModule(string name, float power) : base(name) { this.power = power; }

        public Charger output { set; private get; }
        public float powerOnTime = 0;
        public void PowerOn(float time)
        {
            if (output != null)
            {
                powerOnTime = time;
                output.StartCharging();
            }
        }
        public void PowerOff(float time)
        {
            if (output != null)
            {
                float elapsed = time - powerOnTime;
                output.ConsumePower(elapsed * power);
                output.Discharge();

                powerOnTime = 0;
            }
        }
    }
    class Charger : SimpleComponent
    {
        readonly public float capacity;
        public Charger(string name, float capacity) : base(name)
        {
            this.capacity = capacity;
        }
        public Emitter output { set; private get; }

        float storedPower = 0;
        public void StartCharging()
        {
        }
        public float ConsumePower(float power)
        {
            float remainingCapacity = capacity - storedPower;
            float toConsume = Mathf.Min(power, remainingCapacity);

            storedPower += toConsume;
            return power - toConsume;
        }
        public void Discharge()
        {
            if (output != null)
            {
                output.ConsumePower(storedPower);
                storedPower = 0;
            }
        }
    }
    class Emitter : SimpleComponent
    {
        public Action<float> PowerEmitted = delegate { };

        public Emitter(string name) : base(name) { }

        public void ConsumePower(float power)
        {
            // Fire an event here if the power is sufficient
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
