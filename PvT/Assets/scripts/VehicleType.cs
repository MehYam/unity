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

    public sealed class FirePoint
    {
        public readonly Vector2 point;
        public readonly float angle;

        public FirePoint(Vector2 point, float angle) { this.point = point; this.angle = angle; }
    }

    public readonly FirePoint[] firePoints;
    public readonly GameObject prefab;

    public VehicleType(string name, string assetID, int health, float mass, float maxSpeed, float acceleration, float inertia, float collDmg, int reward, string behaviorKey, FirePoint[] firePoints, GameObject prefab)
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
        this.firePoints = firePoints;
        this.prefab = prefab;
    }
}
