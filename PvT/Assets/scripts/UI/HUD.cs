using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;

using PvT.Util;

/// <summary>
/// HUD should really be called "UIController"
/// </summary>
public sealed class HUD : MonoBehaviour
{
    [Serializable]
    public sealed class CenterPrints
    {
        public Text top;
        public Text middle;
        public Text bottom;
    }
    public CenterPrints centerPrints;
    [Serializable]
    public sealed class Portrait
    {
        public Image image;
        public Text name;
        public Text health;
        public Text xp;
        public Text captures;
        public Text level;
    }
    public Portrait portrait;
    public Fader curtain;
    public Fader space;
    public GameObject joypadFeedback;

    void Start()
    {
        // events
        var gge = GlobalGameEvent.Instance; 
        gge.PlayerSpawned += OnPlayerSpawned;
        gge.HealthChange += OnHealthChange;

        gge.MobDeath += OnMobDeath;
        gge.PossessionStart += OnPossessionStart;
        gge.PossessionEnd += OnPossessionEnd;

        // layout
        //var rect = Util.GetScreenRectInWorldCoords(Camera.main);
    }
    void OnPlayerSpawned(Actor player)
    {
        UpdateHealth(player);
        UpdatePortraitImage(player);
        UpdateLevel(player.actorType);
        UpdateXP(player.actorType);
        UpdateCaptures(player.actorType);
    }
    void OnMobDeath(Actor mob)
    {
        var playerType = Main.Instance.game.player.GetComponent<Actor>().actorType;
        PlayerData.Instance.OnMobDeath(mob);
        UpdateXP(playerType);
        UpdateLevel(playerType);
    }
    void OnPossessionStart(Actor host)
    {
        PlayerData.Instance.OnPossessionStart(host);
    }
    void OnPossessionEnd()
    {
        PlayerData.Instance.OnPossessionEnd();
    }
    void OnHealthChange(Actor actor, float delta)
    {
        if (actor.isPlayer)
        {
            UpdateHealth(actor);
        }
    }
    void UpdateHealth(Actor player)
    {
        portrait.health.text = string.Format("Health: {0:f1}%", 100 * player.health / player.actorType.health);
    }
    void UpdatePortraitImage(Actor player)
    {
        //KAI: this won't work for tanks... need to take a 'snapshot' of current player appearance
        var sprite = player.GetComponent<SpriteRenderer>();
        portrait.image.sprite = sprite.sprite;

        portrait.name.text = player.actorType.name;
    }
    void UpdateLevel(ActorType type)
    {
        portrait.level.text = PlayerData.Instance.GetLevel(type).ToString();
    }
    void UpdateXP(ActorType type)
    {
        portrait.xp.text = "XP: " + PlayerData.Instance.GetXP(type);
    }
    void UpdateCaptures(ActorType type)
    {
        var stats = PlayerData.Instance.GetActorStats(type);
        portrait.captures.text = "Captured: " + stats.numCaptured;
    }
}
