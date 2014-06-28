using UnityEngine;
using PvT.Util;

/// <summary>
/// Constants and basic utilities
/// </summary>
public static class Consts
{
    // Scene layers
    public enum Layer : int
    {
        MOB = 8, 
        MOB_AMMO = 9,
        FRIENDLY_AMMO = 10,
        FRIENDLY = 11,
        ENVIRONMENT = 12,
        HEROLINGS = 13,
        HEROLINGS_RETURNING = 14
    };

    public enum SortingLayer : int
    {
        DEFAULT = 0,
        GROUND = 3,
        TANKTREAD = 4,
        TANKBODY = 5,
        TANKTURRET = 6,
        MOB_AMMO = 7,
        FRIENDLY_AMMO = 8,
        MOB = 9,
        FRIENDLY = 10,
        HEROLINGS = 11,
        EXPLOSIONS = 12,
        SMOKE = 13,
        CLOUDS = 14,
        UI = 17,
        UI_OVERLAY1 = 18,
        UI_OVERLAY2 = 19
    }
    static public readonly int PixelsToUnits = 100;

    static public readonly float HEROLING_UNABSORBABLE = 2;
    static public readonly float HEROLING_ROAM_BOREDOM = 2;
    static public readonly float HEROLING_ATTACH_BOREDOM = 5;
    static public readonly int HEROLING_LIMIT = 3;
    static public readonly float HERO_REGEN = 1;

    static public readonly int POSSESSION_THRESHHOLD = 2;
    static public readonly float DEPOSSESSION_DURATION = 2;
    static public readonly float DEPOSSESSION_ROTATIONS_PER_SEC = 5;

    static public readonly float HEALTH_BAR_TIMEOUT = 5;

    static public readonly float SHIELD_HEALTH_MULTIPLIER = 1;
    static public readonly float SHIELD_BOOST = 0.5f;
    static public readonly float COLLISION_DAMAGE_MULTIPLIER = 0.5f;
    static public readonly float POST_DAMAGE_INVULN = 1;
    static public readonly float POST_DEPOSSESSION_INVULN = 4;
    
    static public readonly float MAX_MOB_ROTATION_DEG_PER_SEC = 90;
    static public readonly float MAX_MOB_TURRET_ROTATION_DEG_PER_SEC = 45;
    static public readonly float MAX_MOB_HULL_ROTATION_DEG_PER_SEC = 1;
}
