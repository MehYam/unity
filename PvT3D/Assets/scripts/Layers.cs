using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Layers
{
    public enum bitnum : byte
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
    public const int Environment = (int)bitnum.Environment;
    public const int Friendly = (int)bitnum.Friendly;
    public const int Enemy = (int)bitnum.Enemy;
    public const int FriendlyAmmo = (int)bitnum.FriendlyAmmo;
    public const int EnemyAmmo = (int)bitnum.EnemyAmmo;
    public const int FriendlyShield = (int)bitnum.FriendlyShield;
    public const int EnemyShield = (int)bitnum.EnemyShield;
    public const int Inanimate = (int)bitnum.Inanimate;
    public static class Masks
    {
        public const int Environment = 1 << Layers.Environment;
        public const int Friendly = 1 << Layers.Friendly;
        public const int Enemy = 1 << Layers.Enemy;
        public const int FriendlyAmmo = 1 << Layers.FriendlyAmmo;
        public const int EnemyAmmo = 1 << Layers.EnemyAmmo;
        public const int FriendlyShield = 1 << Layers.FriendlyShield;
        public const int EnemyShield = 1 << Layers.EnemyShield;
        public const int Inanimate = 1 << Layers.Inanimate;
    }
}
