using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;

using PvT.Util;

public sealed class HUD : MonoBehaviour
{
    [Serializable]
    public class CenterPrints
    {
        public Text top;
        public Text middle;
        public Text bottom;
    }
    public CenterPrints centerPrints;
    public Fader curtain;
    public Fader space;

    public Text score;
    void Start()
    {
        // events
        var gge = GlobalGameEvent.Instance; 
        gge.PlayerSpawned += OnPlayerSpawned;
        gge.HerolingLaunched += OnHerolingChange;
        gge.HerolingAttached += OnHerolingAttached;
        gge.HerolingDetached += OnHerolingDetached;
        gge.HealthChange += OnHealthChange;

        // layout
        //var rect = Util.GetScreenRectInWorldCoords(Camera.main);
    }

    void OnPlayerSpawned(GameObject player)
    {
        UpdateHealth();
    }
    void OnHerolingChange()
    {
        UpdateHerolings();
    }
    void OnHerolingAttached(Actor host)
    {
        UpdateHerolings();
    }
    void OnHerolingDetached(Actor host)
    {
        UpdateHerolings();
    }
    void OnHealthChange(Actor actor, float delta)
    {
        if (actor.gameObject == Main.Instance.game.player)
        {
            UpdateHealth();
        }
    }

    void UpdateHealth()
    {
        //var player = Main.Instance.game.player.GetComponent<Actor>();
        //health.percent = player.health / player.worldObject.health;
    }
    void UpdateHerolings()
    {
        //label1.text = "Herolings " + HerolingActor.ActiveHerolings;
    }
}
