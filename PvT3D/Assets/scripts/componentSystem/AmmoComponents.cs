using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PvT3D.ShipComponent2
{
    public abstract class Ammo
    {
        public float power = 0;
        public float duration = 0;
    }
    public abstract class ProjectileAmmo : Ammo
    {
        public float speed = 0;
        public float distance = 0;
        public bool inheritShipVelocity = false;
    }
    public class PlasmaAmmo : ProjectileAmmo
    {
    }
    public class LaserAmmo : Ammo
    {
        public float width = 0;
    }
    public class Shield : ProjectileAmmo
    {
        public float health = 0;
        public float damagePct = 0;
    }
}
