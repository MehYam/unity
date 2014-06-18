using UnityEngine;
using System.Collections;

public sealed class HUD : MonoBehaviour
{
    public GameObject topLeftPanel;
    public TextMesh label1;
    public ProgressBar health;

    void Start()
    {
        // events
        var gge = GlobalGameEvent.Instance; 
        gge.PlayerSpawned += OnPlayerSpawned;
        gge.HerolingLaunched += OnHerolingChange;
        gge.HerolingDestroyed += OnHerolingChange;
        gge.HerolingAttached += OnHerolingAttached;
        gge.HerolingDetached += OnHerolingDetached;
        gge.HealthChange += OnHealthChange;

        // layout
        var rect = Consts.GetScreenRectInWorldCoords(Camera.main);
        topLeftPanel.transform.position = new Vector3(rect.left, rect.top);
    }

    void OnPlayerSpawned(GameObject player)
    {
        label1.text = "Launch " + player.name;

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
        var player = Main.Instance.game.player.GetComponent<Actor>();
        health.percent = player.health / player.worldObject.health;
    }
    void UpdateHerolings()
    {
        label1.text = "Herolings " + HerolingActor.ActiveHerolings;
    }
}
