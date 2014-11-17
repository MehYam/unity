//#define DEBUG_INTRO

using UnityEngine;
using PvT.Util;

/// <summary>
/// Constants and basic utilities
/// </summary>
public static class Consts
{
    public enum CollisionLayer : int
    {
        DEFAULT = 0,
        MOB = 8, 
        MOB_AMMO = 9,
        FRIENDLY_AMMO = 10,
        FRIENDLY = 11,
        ENVIRONMENT = 12,
        HEROLINGS = 13,
        HEROLINGS_RETURNING = 14,
        BOSS_NO_MOB_COLLISION = 15  // so that the boss can contain and parent mobs
    };

    public enum SortingLayer : int
    {
        DEFAULT = 0,
        GROUND = 3,
        TANKTREAD = 4,
        TANKBODY = 5,
        TANKTURRET = 6,
        AMMO = 7,
        AMMO_UNUSED = 8,
        MOB = 9,
        FRIENDLY = 10,
        AMMO_TOP = 11,
        EXPLOSIONS = 12,
        SMOKE = 13,
        CLOUDS = 14,
        UI = 17,
        UI_OVERLAY1 = 18,
        UI_OVERLAY2 = 19
    }
    static public readonly int PixelsToUnits = 100;

    static public readonly Color TRACKING_ARROW_COLOR_NORMAL = new Color32(0xff, 0x50, 0x00, 0x96);
    static public readonly Color TRACKING_ARROW_COLOR_OVERWHELMED = new Color32(0x00, 0x99, 0xff, 0x96);
    static public readonly float HEROLING_UNABSORBABLE = 2;
    static public readonly float HEROLING_ROAM_BOREDOM = 4;
    static public readonly float HEROLING_ATTACH_BOREDOM = 6;
    static public readonly int HEROLING_LIMIT = 4;
    static public readonly float HEROLING_HEALTH_OVERWHELM = 20;
    static public readonly float HERO_REGEN = 1;

    static public readonly float HEROLING_OVERWHELM_DURATION = 2;
    static public readonly float HEROLING_OVERWHELM_ROTATIONS_PER_SEC = 5;

    static public readonly float HEALTH_BAR_TIMEOUT = 5;

    static public readonly float PLAYER_SPEED_MULTIPLIER = 1.2f;
    static public readonly float PLAYER_ACCEL_MULTIPLIER = 1.2f;
    static public readonly float SHIELD_HEALTH_MULTIPLIER = 1;
    static public readonly float SHIELD_BOOST = 3;
    static public readonly float FUSION_KNOCKBACK_MULTIPLIER = 0.33f;
    static public readonly float COLLISION_DAMAGE_MULTIPLIER = 1f;
    static public readonly float POST_DAMAGE_INVULN = 1.5f;
    static public readonly float POST_POSSESSION_INVULN = 2;
    static public readonly float POST_DEPOSSESSION_INVULN = 4;
    static public readonly float WEAPON_CHARGE_OVERLOAD_PCT_DMG = 0.05f;  // damages 5% remaining health per sec
    
    static public readonly float MAX_MOB_ROTATION_DEG_PER_SEC = 90;
    static public readonly float MAX_MOB_TURRET_ROTATION_DEG_PER_SEC = 45;
    static public readonly float MAX_MOB_HULL_ROTATION_DEG_PER_SEC = 1;
    static public readonly float MOB_SPAWN_DELAY = 0.5f;

    static public readonly float FLARE_ANIMATION_PEAK_SECONDS = 0.5f;

    static public readonly float RESPAWN_RATE = 15;

    static public readonly float SMOOTH_DAMP_MULTIPLIER = 0.5f;

    static public readonly string BLINKER_TAG = "blinker";
    static public readonly string SPAWNED_MOB_TAG = "spawnedMobTag";

    static public readonly int XP_PER_KILL = 10;
    static public readonly int XP_PER_CAPTURE = 50;
    static public readonly int XP_CURVE_MULTIPLIER = 100;  // i.e. 100 => 200, 400, 800, 1600, 3200

    // UI and cinematics ///////////////////////////////////////
#if DEBUG_INTRO
    static public readonly float TEXT_FADE_SECONDS = 0.2f;
#else
    static public readonly float TEXT_FADE_SECONDS = 3f;
#endif
    static public readonly float TEXT_FADE_SECONDS_FAST = TEXT_FADE_SECONDS / 2;
    static public readonly float TEXT_FADE_SECONDS_SLOW = TEXT_FADE_SECONDS * 2;
}
