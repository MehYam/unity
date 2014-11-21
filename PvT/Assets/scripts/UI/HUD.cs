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
        public UIProgressBar xp;
        public Text captures;
        public Text level;
    }
    public Portrait portrait;
    public Fader curtain;
    public Fader space;
    public GameObject joypadFeedback;
    public GameObject bling;
    public GameObject levelUpBadge;

    void Start()
    {
        // events
        var gge = GlobalGameEvent.Instance; 
        gge.PlayerSpawned += OnPlayerSpawned;
        gge.HealthChange += OnHealthChange;
        gge.PlayerDataUpdated += OnPlayerDataUpdated;
        gge.GainingXP += OnXPGain;
        gge.LevelUp += OnLevelUp;
        gge.TierUp += OnTierUp;
    }
    void OnPlayerSpawned(Actor player)
    {
        UpdateHealth(player);
        UpdatePortraitImage(player);
        UpdateLevel(player.actorType);
        UpdateXP(player.actorType);
        UpdateCaptures(player.actorType);
    }
    void OnPlayerDataUpdated(PlayerData playerData)
    {
        var player = Main.Instance.game.player.GetComponent<Actor>();

        UpdateLevel(player.actorType);
        UpdateXP(player.actorType);
        UpdateCaptures(player.actorType);
    }
    void OnHealthChange(Actor actor, float delta)
    {
        if (actor.isPlayer)
        {
            UpdateHealth(actor);
        }
    }
    static readonly Vector2 xpUpOffset = new Vector2(0, 0.5f);
    void OnXPGain(int xp, Vector2 where)
    {
        var blingObj = (GameObject)Instantiate(bling);
        blingObj.GetComponent<DumbTextDropShadow>().text = string.Format("+ {0} XP", xp);
        blingObj.transform.position = where + xpUpOffset;
    }
    void OnTierUp(ActorType newType)
    {
        StartCoroutine(ShowBadge());
    }
    void OnLevelUp(int newLevel)
    {
        StartCoroutine(ShowBadge());
    }
    IEnumerator ShowBadge()
    {
        var badgeGO = (GameObject)Instantiate(levelUpBadge);
        var badge = badgeGO.GetComponent<Graphic>();

        badge.rectTransform.SetParent(transform, false);
        badge.rectTransform.anchoredPosition = Vector2.zero;

        const float FADE_IN_SECONDS = 0.5f;
        const float HOLD_SECONDS = 2;
        const float FADE_OUT_SECONDS = 2;

        var fader = badgeGO.GetOrAddComponent<Fader>();
        fader.Fade(0, 0, false);
        fader.Fade(1, FADE_IN_SECONDS);

        yield return new WaitForSeconds(FADE_IN_SECONDS + HOLD_SECONDS);

        fader.Fade(0, FADE_OUT_SECONDS);

        yield return new WaitForSeconds(FADE_OUT_SECONDS);

        Destroy(badgeGO);
    }
    void UpdateHealth(Actor player)
    {
        portrait.health.text = string.Format("Health: {0}/{1} ({2:f1}%)", (int)player.health, player.attrs.maxHealth, 100 * player.health / player.actorType.attrs.maxHealth);
    }
    void UpdatePortraitImage(Actor player)
    {
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
        var pct = PlayerData.Instance.GetLevelProgress(type);
        portrait.xp.percent = pct;
    }
    void UpdateCaptures(ActorType type)
    {
        var stats = PlayerData.Instance.GetActorStats(type);
        portrait.captures.text = "Captured: " + stats.captures;
    }
}
