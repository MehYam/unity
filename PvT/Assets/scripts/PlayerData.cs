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
        public int numKilled = 0;
        public int numCaptured = 0;
        public int numKillsWithActor = 0;
    }
    [Serializable]
    public sealed class PlayerStats
    {
        public int totalKills = 0;
        public int totalCaptures = 0;
        public int totalDamageDone = 0;
    }
    Dictionary<string, ActorStats> _actorStatsLookup = new Dictionary<string, ActorStats>();

    public ActorStats GetActorStats(string actorName)
    {
        ActorStats retval = null;
        if (!_actorStatsLookup.TryGetValue(actorName, out retval))
        {
            retval = new ActorStats();
            _actorStatsLookup[actorName] = retval;
        }
        return retval;
    }
    public PlayerStats playerStats { get; private set; }

    public int GetXP(ActorType type)
    {
        var stats = GetActorStats(type.name);
        var xp = stats.numKillsWithActor * Consts.XP_PER_KILL + stats.numCaptured * Consts.XP_PER_CAPTURE;
        return xp;
    }
    public int GetLevel(ActorType type)
    {
        var xp = GetXP(type);
        int level = Util.FastLog2Floor(xp / Consts.XP_CURVE_MULTIPLIER);
        return Mathf.Max(1, level);
    }

    //KAI: the only reason OnMobDeath and OnPossessionStart are not subscribed to directly from
    // GlobalGameEvent is that the UIController needs to get them first
    public void OnMobDeath(Actor actor)
    {
        if (!actor.isPlayer && !actor.isAmmo)
        {
            var playerActor = Main.Instance.game.player.GetComponent<Actor>();
            AddKill(actor.actorType.name, playerActor.actorType.name);
        }
    }
    public void OnPossessionStart(Actor host)
    {
        AddCapture(host.actorType.name);
    }

    /// <summary>
    /// Should only be called periodically, say when user pauses, when app goes to background, when level's complete, etc
    /// </summary>
    public void Commit()
    {
        var actorStatsSerialized = Base64Serializer.ToBase64(_actorStatsLookup);
        PlayerPrefs.SetString(ACTOR_STATS, actorStatsSerialized);

        var playerStatsSerialized = Base64Serializer.ToBase64(playerStats);
        PlayerPrefs.SetString(PLAYER_STATS, playerStatsSerialized);

        PlayerPrefs.Save();
    }

    // Implementation
    static readonly string ACTOR_STATS = "actorStats1";
    static readonly string PLAYER_STATS = "playerStats1";
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
    }
    void AddKill(string victimTypeName, string playerTypeName)
    {
        var stats = GetActorStats(victimTypeName);
        ++stats.numKilled;
        ++playerStats.totalKills;

        stats = GetActorStats(playerTypeName);
        ++stats.numKillsWithActor;
    }
    void AddCapture(string actorTypeName)
    {
        var stats = GetActorStats(actorTypeName);
        ++stats.numCaptured;
        ++playerStats.totalCaptures;
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
}
