using UnityEngine;
using System;
using System.Collections;

using Random = UnityEngine.Random;

using PvT.Util;

public sealed class TutorialScript : MonoBehaviour
{
    public GameObject map;
    void Start()
    {
        DebugUtil.Log(this, "Start");

        Debug.LogWarning("KAI: we need to check for script execution order dependencies like this one and figure them out.");

        if (Main.Instance == null)
        {
            GlobalGameEvent.Instance.MainReady += OnMainReady;
        }
        else
        {
            OnMainReady();
        }
    }
    void OnMainReady()
    {
        GlobalGameEvent.Instance.MainReady -= OnMainReady;
        GlobalGameEvent.Instance.GameReady += OnGameReady;
        Main.Instance.game.SetMap(map);
    }
    void OnGameReady(IGame game)
    {
        GlobalGameEvent.Instance.GameReady -= OnGameReady;
        Main.Instance.hud.curtain.Fade(1, 0);
        Main.Instance.hud.curtain.Fade(0, Consts.TEXT_FADE_SECONDS);

        StartCoroutine(TutorialSequence());
    }
    IEnumerator TutorialSequence()
    {
        // aliases for less typing
        var main = Main.Instance;
        var hud = main.hud;
        var game = main.game;

        ///////////////// Teach movement
        main.PlayMusic(main.music.duskToDawn);

        var playerActor = game.SpawnPlayer(Vector3.zero).GetComponent<Actor>();
        playerActor.firingEnabled = false;
        playerActor.thrustEnabled = false;
        playerActor.immortal = true;
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrints.top, "We were taken somewhere unfamiliar.", Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrints.bottom, "(Use W, A, S, D or arrow keys to explore)", Consts.TEXT_FADE_SECONDS);

        playerActor.thrustEnabled = true;
        yield return StartCoroutine(Util.YieldUntil(() =>
        {
            // Wait until the player's travelled some distance
            return Vector3.Distance(playerActor.transform.position, Vector3.zero) > game.WorldBounds.width / 3;
        }
        ));

        AnimatedText.FadeOut(hud.centerPrints.top, Consts.TEXT_FADE_SECONDS_FAST);
        AnimatedText.FadeOut(hud.centerPrints.bottom, Consts.TEXT_FADE_SECONDS_FAST);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        /////////////////// Introduce mob
        AnimatedText.FadeIn(hud.centerPrints.top, "In the distance, another appeared.", Consts.TEXT_FADE_SECONDS);

        var mobActor = game.SpawnMob("GREENK").GetComponent<Actor>();
        mobActor.isCapturable = true;

        MobAI.Instance.AttachAI(mobActor);

        mobActor.firingEnabled = false;
        mobActor.thrustEnabled = false;
        mobActor.transform.position = -playerActor.transform.position;
        mobActor.trackingArrowEnabled = true;
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrints.bottom, "(Seek out the newcomer)", Consts.TEXT_FADE_SECONDS);

        yield return StartCoroutine(Util.YieldUntil(() =>
        {
            // Wait until the player's close to the mob
            return Vector3.Distance(playerActor.transform.position, mobActor.transform.position) < 2.5f;
        }
        ));

        AnimatedText.FadeOut(hud.centerPrints.top, Consts.TEXT_FADE_SECONDS_FAST);
        AnimatedText.FadeOut(hud.centerPrints.bottom, Consts.TEXT_FADE_SECONDS_FAST);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        mobActor.thrustEnabled = true;
        //playerActor.takenDamageMultiplier = 0.25f;

        //////////////////// Wait until the player's been hit
        var startHealth = playerActor.health;
        yield return StartCoroutine(Util.YieldUntil(() => playerActor.health != startHealth));

        main.PlayMusic(main.music.duskToDawn);

        AnimatedText.FadeIn(hud.centerPrints.top, "This other was unfriendly.", Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        AnimatedText.FadeIn(hud.centerPrints.bottom, "(FLEE!)", Consts.TEXT_FADE_SECONDS);

        float distance = 0;
        Vector3 lastPosition = playerActor.transform.position;
        yield return StartCoroutine(Util.YieldUntil(() =>
        {
            // Wait until the player's travelled a while
            distance += Vector3.Distance(playerActor.transform.position, lastPosition);
            lastPosition = playerActor.transform.position;

            return distance > 30;
        }
        ));

        AnimatedText.FadeOut(hud.centerPrints.top, Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(hud.centerPrints.bottom, Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrints.top, "It persisted, leaving us no choice.", Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_SLOW);

        AnimatedText.FadeOut(hud.centerPrints.top, Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrints.top, "We had to reveal our secret.", Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        //////////////////// Teach possession mechanic
        do
        {
            AnimatedText.FadeIn(hud.centerPrints.bottom, "(Aim with the mouse, left button shoots)", Consts.TEXT_FADE_SECONDS);
            playerActor.firingEnabled = true;

            // wait until the other ship is subdued
            yield return StartCoroutine(Util.YieldUntil(() => mobActor.overwhelmPct == 1.0f));

            AnimatedText.FadeIn(hud.centerPrints.bottom, "(QUICKLY! Collide with it!)", Consts.TEXT_FADE_SECONDS);

            // wait until the player has either passed or failed the task of possessing the ship
            yield return StartCoroutine(Util.YieldUntil(() => (mobActor.overwhelmPct < 1) || game.enemyInPossession));
        }
        while (!game.enemyInPossession);

        AnimatedText.FadeOut(hud.centerPrints.top, Consts.TEXT_FADE_SECONDS_FAST);
        AnimatedText.FadeOut(hud.centerPrints.bottom, Consts.TEXT_FADE_SECONDS_FAST);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        AnimatedText.FadeIn(hud.centerPrints.top, "My Eukarytes overwhelmed the vessel.", Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrints.bottom, "We took it, and were safe.", Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_SLOW);

        AnimatedText.FadeOut(hud.centerPrints.top, Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(hud.centerPrints.bottom, Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrints.bottom, "Then more arrived.", Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_SLOW);

        AnimatedText.FadeOut(hud.centerPrints.bottom, Consts.TEXT_FADE_SECONDS_FAST);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        main.hud.curtain.Fade(1, Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_SLOW);

//HACK:
        GameObject.Destroy(main.game.player);
        GlobalGameEvent.Instance.FireTutorialOver(this);
    }
}
