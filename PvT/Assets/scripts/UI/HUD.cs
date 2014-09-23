using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using PvT.Util;

public sealed class HUD : MonoBehaviour
{
    public Fader curtain;
    public Fader space;

    public Text centerPrintTop;
    public Text centerPrintMiddle;
    public Text centerPrintBottom;

    public Text score;
    void Start()
    {
        // events
        var gge = GlobalGameEvent.Instance; 
        gge.PlayerSpawned += OnPlayerSpawned;
        gge.HerolingLaunched += OnHerolingChange;
        //gge.HerolingDestroyed += OnHerolingChange;
        gge.HerolingAttached += OnHerolingAttached;
        gge.HerolingDetached += OnHerolingDetached;
        gge.HealthChange += OnHealthChange;

        gge.CenterPrint += OnCenterPrint;

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
    void OnCenterPrint(string print)
    {
        StartCoroutine(CenterPrintAnim(print));
    }
    IEnumerator CenterPrintAnim(string print)
    {
        centerPrintTop.gameObject.SetActive(true);
        centerPrintTop.text = print;

        yield return new WaitForSeconds(3);
        centerPrintTop.gameObject.SetActive(false);
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
