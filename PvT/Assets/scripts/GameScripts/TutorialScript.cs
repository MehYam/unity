using UnityEngine;
using System;
using System.Collections;

using Random = UnityEngine.Random;

using PvT.Util;

public sealed class TutorialScript : MonoBehaviour
{
    void Start()
    {
        DebugUtil.Log(this, "Start");
        Main.Instance.hud.curtain.Fade(1, 0);

        Main.Instance.map.SetActive(true);
        Main.Instance.hud.curtain.Fade(0, Consts.TEXT_FADE_SECONDS);

        GlobalGameEvent.Instance.GameReady += OnGameReady;
    }
    void OnGameReady(IGame game)
    {
        StartCoroutine(TutorialSequence());
    }
    IEnumerator TutorialSequence()
    {
        // aliases for less typing
        var main = Main.Instance;
        var hud = main.hud;
        var game = main.game;

        main.PlayMusic(main.music.duskToDawn);

        var playerActor = game.SpawnPlayer(Vector3.zero).GetComponent<Actor>();
        playerActor.firingEnabled = false;
        playerActor.thrustEnabled = false;
        playerActor.immortal = true;

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrintTop, "We were brought somewhere unfamiliar.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrintBottom, "(Use W, A, S, D or arrow keys to explore)", Consts.TEXT_FADE_SECONDS);

        playerActor.thrustEnabled = true;

        yield return StartCoroutine(Util.YieldUntil(() =>
        {
            // Wait until the player's travelled some distance
            return Vector3.Distance(playerActor.transform.position, Vector3.zero) > game.WorldBounds.width / 3;
        }
        ));

        AnimatedText.FadeOut(hud.centerPrintTop, Consts.TEXT_FADE_SECONDS_FAST);
        AnimatedText.FadeOut(hud.centerPrintBottom, Consts.TEXT_FADE_SECONDS_FAST);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        AnimatedText.FadeIn(hud.centerPrintTop, "Suddenly, another appeared.", Consts.TEXT_FADE_SECONDS);

        var mobActor = game.SpawnMob("GREENK").GetComponent<Actor>();
        mobActor.firingEnabled = false;
        mobActor.thrustEnabled = false;
        mobActor.transform.position = -playerActor.transform.position;
        mobActor.trackingArrowEnabled = true;
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrintBottom, "(Investigate the newcomer)", Consts.TEXT_FADE_SECONDS);

        yield return StartCoroutine(Util.YieldUntil(() =>
        {
            // Wait until the player's close to the mob
            return Vector3.Distance(playerActor.transform.position, mobActor.transform.position) < 2.5f;
        }
        ));

        AnimatedText.FadeOut(hud.centerPrintTop, Consts.TEXT_FADE_SECONDS_FAST);
        AnimatedText.FadeOut(hud.centerPrintBottom, Consts.TEXT_FADE_SECONDS_FAST);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        mobActor.thrustEnabled = true;
        playerActor.takenDamageMultiplier = 0.25f;

        var startHealth = playerActor.health;
        yield return StartCoroutine(Util.YieldUntil(() =>
        {
            // Wait until the player's been hit
            return playerActor.health != startHealth;
        }
        ));

        AnimatedText.FadeIn(hud.centerPrintTop, "This other was not friendly.", Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        AnimatedText.FadeIn(hud.centerPrintBottom, "(Run!)", Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        AnimatedText.FadeOut(hud.centerPrintTop, Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(hud.centerPrintBottom, Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrintTop, "It would not stop.", Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrintTop, "It left us no choice.", Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_SLOW);

        AnimatedText.FadeIn(hud.centerPrintBottom, "(Aim with the mouse, left button to fire)", Consts.TEXT_FADE_SECONDS);

        playerActor.firingEnabled = true;

        //KAI: left off here, need to trap the possession event, and detect it
        bool possession = false;
        Action possessionStarted = () => { DebugUtil.Log(this, "HERE"); possession = true; };
        GlobalGameEvent.Instance.PossessionStart += possessionStarted;

        yield return StartCoroutine(Util.YieldUntil(() => possession));

        GlobalGameEvent.Instance.PossessionStart -= possessionStarted;

        AnimatedText.FadeIn(hud.centerPrintBottom, "YOU GONE DONE IT NOW", Consts.TEXT_FADE_SECONDS);
    }
}
