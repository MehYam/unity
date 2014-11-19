using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using PvT.Util;

public sealed class PlayerData 
{
    //KAI: publics in here......
    [Serializable]
    public sealed class ActorStats
    {
        public int deaths = 0;
        public int kills = 0;
        public int captures = 0;
        public int xpWith = 0;
    }
    [Serializable]
    public sealed class PlayerStats
    {
        public int kills = 0;
        public int captures = 0;
        public int damageDealth = 0;
        public int xp = 0;
    }
    Dictionary<string, ActorStats> _actorStatsLookup = new Dictionary<string, ActorStats>();

    public ActorStats GetActorStats(ActorType type)
    {
        ActorStats retval = null;
        if (!_actorStatsLookup.TryGetValue(type.name, out retval))
        {
            retval = new ActorStats();
            _actorStatsLookup[type.name] = retval;
        }
        return retval;
    }
    public PlayerStats playerStats { get; private set; }

    static public int GetXPAtLevel(int level)
    {
        --level;
        return (int)(level > 0 ? (Mathf.Pow(2, level) * Consts.XP_CURVE_MULTIPLIER) : 0);
    }
    static public int GetLevelAtXP(int xp)
    {
        return Util.FastLog2Floor(xp / Consts.XP_CURVE_MULTIPLIER) + 1;
    }
    static public int GetKillXP(int playerLevel, int victimLevel)
    {
        int levelGap = playerLevel - victimLevel;
        float xpPercent = Mathf.Max(0,
            (float)(Consts.XP_MAX_LEVEL_GAP - levelGap) / (float)Consts.XP_MAX_LEVEL_GAP
        );
        return (int)(xpPercent * Consts.XP_PER_KILL_PER_LEVEL * (float)victimLevel);
    }
    /// <summary>
    /// Returns the upgraded version of the actor, if the actor has the XP.
    /// </summary>
    /// <param name="actorType">The actor type to upgrade</param>
    /// <returns>The actor type's upgrade, or the same actor type no upgrade is available</returns>
    public ActorType GetTierUpgrade(ActorType actorType)
    {
        if (!string.IsNullOrEmpty(actorType.upgradesTo))
        {
            var nextTier = Main.Instance.game.loader.GetActorType(actorType.upgradesTo);
            int level = GetLevel(actorType);
            if (level >= nextTier.level)
            {
                return GetTierUpgrade(nextTier);
            }
        }
        return actorType;
    }

    public int GetLevel(ActorType type)
    {
        return GetLevel(GetActorStats(type), type.level);
    }
    public int GetLevel(ActorStats stats, int baseLevel)
    {
        return GetLevelAtXP(GetXPAtLevel(baseLevel) + stats.xpWith);
    }
    public float GetLevelProgress(ActorType type)
    {
        var intrinsicLevelXP = GetXPAtLevel(type.level);
        var accumulatedXPWithMob = GetActorStats(type).xpWith;
        var xp = intrinsicLevelXP + accumulatedXPWithMob;

        var level = GetLevelAtXP(xp);
        var levelXP = GetXPAtLevel(level);

        return (float)(xp - levelXP) / (float)(GetXPAtLevel(level + 1) - levelXP);
    }

    /// <summary>
    /// Should only be called periodically, say when user pauses, when app goes to background, when level's complete, etc
    /// </summary>
    public void Commit()
    {
        var actorStatsBase64 = Base64Serializer.ToBase64(_actorStatsLookup);
        PlayerPrefs.SetString(ACTOR_STATS, actorStatsBase64);

        var playerStatsBase64 = Base64Serializer.ToBase64(playerStats);
        PlayerPrefs.SetString(PLAYER_STATS, playerStatsBase64);

        PlayerPrefs.Save();
    }

    // Implementation
    static readonly int VERSION = 1;
    static readonly string ACTOR_STATS = "actorStats" + VERSION;
    static readonly string PLAYER_STATS = "playerStats" + VERSION;
    PlayerData()
    {
        playerStats = new PlayerStats();

        // Deserialize from the previous session
        string base64 = PlayerPrefs.GetString(ACTOR_STATS, "");
        if (!string.IsNullOrEmpty(base64))
        {
            _actorStatsLookup = Base64Serializer.FromBase64<Dictionary<string, ActorStats>>(base64);
        }
        base64 = PlayerPrefs.GetString(PLAYER_STATS, "");
        if (!string.IsNullOrEmpty(base64))
        {
            playerStats = Base64Serializer.FromBase64<PlayerStats>(base64);
        }
        GlobalGameEvent.Instance.MobDeath += OnMobDeath;
        GlobalGameEvent.Instance.PossessionComplete += OnPossessionComplete;
    }
    void Destroy()
    {
        GlobalGameEvent.Instance.MobDeath -= OnMobDeath;
        GlobalGameEvent.Instance.PossessionComplete -= OnPossessionComplete;
    }
    void OnMobDeath(Actor actor)
    {
        if (!actor.isPlayer && !actor.isAmmo)
        {
            var playerActorType = Main.Instance.game.player.GetComponent<Actor>().actorType;
            var playerActorStats = GetActorStats(playerActorType);
            var prevXP = playerActorStats.xpWith;

            AddKill(actor.actorType, playerActorType, playerActorStats);

            if (playerActorStats.xpWith > prevXP)
            {
                GlobalGameEvent.Instance.FireGainingXP(playerActorStats.xpWith - prevXP, actor.gameObject.transform.position);
            }
            GlobalGameEvent.Instance.FirePlayerDataUpdated(this);
        }
    }
    void OnPossessionComplete(Actor host)
    {
        var playerActor = Main.Instance.game.player.GetComponent<Actor>();
        var playerActorStats = GetActorStats(playerActor.actorType);
        var prevXP = playerActorStats.xpWith;

        AddCapture(playerActorStats, playerActor.actorType.level);

        if (playerActorStats.xpWith > prevXP)
        {
            GlobalGameEvent.Instance.FireGainingXP(playerActorStats.xpWith - prevXP, host.gameObject.transform.position);
        }
        GlobalGameEvent.Instance.FirePlayerDataUpdated(this);
    }
    void AddKill(ActorType victimType, ActorType playerType, ActorStats playerActorStats)
    {
        var victimStats = GetActorStats(victimType);
        ++victimStats.deaths;
        ++playerStats.kills;
        ++playerActorStats.kills;

        var level = GetLevel(playerActorStats, playerType.level);
        var xp = GetKillXP(level, victimType.level);
        playerStats.xp += xp;
        playerActorStats.xpWith += xp;
    }
    void AddCapture(ActorStats captee, int level)
    {
        ++captee.captures;
        ++playerStats.captures;

        var xp = level * Consts.XP_PER_CAPTURE_PER_LEVEL;
        playerStats.xp += xp;
        captee.xpWith += xp;
    }

    // Instance management
    static PlayerData s_instance;
    static public PlayerData Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = new PlayerData();
            }
            return s_instance;
        }
    }
    static public void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        if (s_instance != null)
        {
            s_instance.Destroy();
        }
        s_instance = new PlayerData();
    }

}
