using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Overengineer: all this to avoid 1) lookups based on layer name strings, 2) casting Layers.ID to int everywhere
public static class Layers
{
    public enum ID : byte
    {
        Environment = 8,
        Friendly,
        Enemy,
        FriendlyAmmo,
        EnemyAmmo,
        FriendlyShield,
        EnemyShield,
        Inanimate
    }
    public struct Layer
    {
        public readonly ID id;
        public readonly int idNum;
        public readonly int mask;
        public Layer(ID bn) { this.id = bn; this.idNum = (int)bn; mask = 1 << idNum; }
    }
    static public readonly Layer Environment = new Layer(ID.Environment);
    static public readonly Layer Friendly = new Layer(ID.Friendly);
    static public readonly Layer Enemy = new Layer(ID.Enemy);
    static public readonly Layer FriendlyAmmo = new Layer(ID.FriendlyAmmo);
    static public readonly Layer EnemyAmmo = new Layer(ID.EnemyAmmo);
    static public readonly Layer FriendlyShield = new Layer(ID.FriendlyShield);
    static public readonly Layer EnemyShield = new Layer(ID.EnemyShield);
    static public readonly Layer Inanimate = new Layer(ID.Inanimate);
}
