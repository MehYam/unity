using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using kaiGameUtil;
using System;

namespace Components
{
    abstract class Component
    {
        public readonly string name;

        public readonly List<Component> inputs = new List<Component>();
        public readonly List<Component> outputs = new List<Component>();

        public void ReceivePower(Component from, float power) { ReceivePower_Impl(from, power); }
        public Component(string name) { this.name = name; }

        abstract protected void ReceivePower_Impl(Component from, float power);

        public override string ToString()
        {
            return name;
        }
    }
    class PowerModule : Component
    {
        readonly float power;
        public PowerModule(string name, float power) : base(name) { this.power = power; }
         
        protected override void ReceivePower_Impl(Component from, float p)
        {
            // any power activates this power module
            var powerToSend = p > 0 ? this.power : 0;
            foreach (var component in outputs)
            {
                component.ReceivePower(this, powerToSend);
            }
        }
    }
    interface ICharger
    {
        void Charge();
        void Discharge();
    }
    class Charger : Component, ICharger
    {
        readonly public float capacity;
        public Charger(string name, float capacity) : base(name)
        {
            this.capacity = capacity;
        }
        public void Charge() { }
        public void Discharge() { }

        protected override void ReceivePower_Impl(Component from, float power)
        {
        }
    }
    class ChargerAutofire : Component, ICharger
    {
        readonly public float capacity;
        public ChargerAutofire(string name, float capacity) : base(name)
        {
            this.capacity = capacity;
        }
        public void Charge() { }
        public void Discharge() { }

        protected override void ReceivePower_Impl(Component from, float power)
        {
        }
    }
    class Emitter : Component
    {
        public Emitter(string name) : base(name) { }

        protected override void ReceivePower_Impl(Component from, float power)
        {
            // raise an event that indicates it's time to fire
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
