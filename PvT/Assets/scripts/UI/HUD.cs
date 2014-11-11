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
    [Serializable]
    public class Portrait
    {
        public Image image;
        public Text name;
        public Text health;
        public Text kills;
        public Text captures;
    }
    public Portrait portrait;
    public Fader curtain;
    public Fader space;

    void Start()
    {
        // events
        var gge = GlobalGameEvent.Instance; 
        gge.PlayerSpawned += OnPlayerSpawned;
        gge.HealthChange += OnHealthChange;

        // layout
        //var rect = Util.GetScreenRectInWorldCoords(Camera.main);
    }

    void OnPlayerSpawned(Actor player)
    {
        UpdateHealth(player);
        
        //KAI: this won't work for tanks... need to take a 'snapshot' of current player appearance
        var sprite = player.GetComponent<SpriteRenderer>();
        portrait.image.sprite = sprite.sprite;
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
}
