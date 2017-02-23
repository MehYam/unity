using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using kaiGameUtil;

namespace Components
{
    abstract class Component
    {
        public readonly string name;

        public readonly List<Component> inputs = new List<Component>();
        public readonly List<Component> outputs = new List<Component>();

        public void AcceptPower(Component from, float power) { AcceptPowerImpl(from, power); }
        public Component(string name) { this.name = name; }

        abstract protected void AcceptPowerImpl(Component from, float power);

        public override string ToString()
        {
            return name;
        }
    }
    class PowerModule : Component
    {
        readonly float power;
        public PowerModule(string name, float power) : base(name) { this.power = power; }

        protected override void AcceptPowerImpl(Component from, float p)
        {
            // any power activates this power module
            var powerToSend = p > 0 ? this.power : 0;
            foreach (var component in outputs)
            {
                component.AcceptPower(this, powerToSend);
            }
        }
    }
    class AutofireDischarger : Component
    {
        readonly float capacity;
        readonly float time;
        public AutofireDischarger(string name, float capacity, float time) : base(name) { this.capacity = capacity; this.time = time; }

        protected override void AcceptPowerImpl(Component from, float power)
        {
            if (power > 0)
            {
                // add to current charge.  If current charge > limit, discharge
            }
            else
            {
                // set charge to 0
            }
        }
    }
    class ManualDischarger : Component
    {
        readonly float capacity;
        readonly float time;
        public ManualDischarger(string name, float capacity, float time) : base(name) { this.capacity = capacity; this.time = time; }

        protected override void AcceptPowerImpl(Component from, float power)
        {
            if (power > 0)
            {
                // add to current charge, up to limit
            }
            else
            {
                // release charge
                foreach (var component in outputs)
                {
                    component.AcceptPower(this, capacity);
                }
            }
        }
    }
    class Emitter : Component
    {
        public Emitter(string name) : base(name) { }

        protected override void AcceptPowerImpl(Component from, float power)
        {
            // raise an event that indicates it's time to fire
        }
    }
    class Schematic
    {
        public readonly Layer<Component> grid = new Layer<Component>(3, 3);

        public override string ToString()
        {
            return grid.ToString();
        }
    }
}
