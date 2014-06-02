using UnityEngine;
using System.Collections;

//KAI: this deserves some object composition
public sealed class VehicleType
{
    public readonly string name;
    public readonly string assetID;

    public readonly int health;
    public readonly float mass;
    public readonly float maxSpeed;
    public readonly float acceleration;
    public readonly float inertia;
    public readonly float collDmg;

    public readonly int reward;

    public readonly string behaviorKey;

    public sealed class Weapon
    {
        public readonly string type; // KAI: convert to enum
		public readonly int dmg;
		public readonly Vector2 offset;
		public readonly float angle;
		public readonly int level; // KAI: nuke this.... turn it instead into a new type

        public Weapon(string type, int dmg, Vector2 offset, float angle, int level) 
        {
            this.type = type;
            this.dmg = dmg;
            this.offset = offset;
            this.angle = angle;
            this.level = level;
        }

        static public Weapon FromString(string str)
        {
            var parts = str.Split(',');
            string type = parts[0];
            int dmg = int.Parse(parts[1]);
            float x = float.Parse(parts[2]) / Consts.PixelsToUnits;
            float y = -float.Parse(parts[3]) / Consts.PixelsToUnits;
            float angle = parts.Length > 4 ? float.Parse(parts[4]) : 0;
            int level = parts.Length > 5 ? int.Parse(parts[5]) : 0;

            return new Weapon(type, dmg, new Vector2(x, y), angle, level);
        }
    }

    public readonly Weapon[] weapons; // the behavior's in charge of which FirePoint to when firing ammo
    public readonly GameObject prefab;

    public VehicleType(string name, string assetID, int health, float mass, float maxSpeed, float acceleration, float inertia, float collDmg, int reward, string behaviorKey, Weapon[] firePoints, GameObject prefab)
    {
        this.name = name;
        this.assetID = assetID;
        this.health = health;
        this.mass = mass;
        this.maxSpeed = maxSpeed;
        this.acceleration = acceleration;
        this.inertia = inertia;
        this.collDmg = collDmg;
        this.reward = reward;
        this.behaviorKey = behaviorKey;
        this.weapons = firePoints;
        this.prefab = prefab;
    }
}
