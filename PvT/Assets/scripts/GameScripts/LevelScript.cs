using UnityEngine;
using System;
using System.Collections;

using PvT.Util;

using Random = UnityEngine.Random;

public class LevelScript : MonoBehaviour
{
    void Start()
    {
        Debug.Log("LevelScript.Start");
        Main.Instance.hud.curtain.Fade(1, 0);

        GlobalGameEvent.Instance.EnemyDestroyed += OnEnemyDestroyed;
        GlobalGameEvent.Instance.GameReady += OnGameReady;

        Main.Instance.map.SetActive(true);
        Main.Instance.hud.curtain.Fade(0, Consts.TEXT_FADE_SECONDS);
    }
    void OnGameReady(IGame game)
    {
        StartCoroutine(TutorialSequence());
        //StartNextLevel();
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
        
        AnimatedText.FadeIn(hud.centerPrintBottom, "(Flee!)", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrintTop, "But I was not defenseless.", Consts.TEXT_FADE_SECONDS);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        AnimatedText.FadeIn(hud.centerPrintBottom, "(Aim with the mouse, fire the button)", Consts.TEXT_FADE_SECONDS);

        playerActor.firingEnabled = true;

        //KAI: left off here, need to trap the possession event, and detect it
        bool possession = false;
        Action possessionStarted = () => { possession = true; };
        GlobalGameEvent.Instance.PossessionStart += possessionStarted;

        yield return StartCoroutine(Util.YieldUntil(() => possession));

        AnimatedText.FadeIn(hud.centerPrintBottom, "YOU GONE DONE IT NOW", Consts.TEXT_FADE_SECONDS);

    }


    /// <summary>
    /// //////////////////// level runner, arguably this should be split from the tutorial sequence above
    /// </summary>
    void StartNextLevel()
    {
        StartNextWave();
    }

    int _liveEnemies = 0;
    void StartNextWave()
    {
        var game = Main.Instance.game;
        var loader = Main.Instance.game.loader;
        if (Main.Instance.runWaves)
        {
            var wave = loader.levels[Main.Instance.startWave].NextWave();
            foreach (var squad in wave.squads)
            {
                for (int i = 0; i < squad.count; ++i)
                {
                    var mob = game.SpawnMob(squad.enemyID);
                    PositionMob(mob);

                    ++_liveEnemies;
                }
            }
        }
    }

    void PositionMob(GameObject mob)
    {
        // put the actor at the edge
        Vector3 spawnLocation;
        var bounds = new XRect(Main.Instance.game.WorldBounds);
        bounds.Inflate(-1);

        if (Util.CoinFlip())
        {
            spawnLocation = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Util.CoinFlip() ? bounds.min.y : bounds.max.y);
        }
        else
        {
            spawnLocation = new Vector3(Util.CoinFlip() ? bounds.min.x : bounds.max.x, Random.Range(bounds.min.y, bounds.max.y));
        }
        mob.transform.localPosition = spawnLocation;

        mob.GetComponent<Actor>().trackingArrowEnabled = true;
    }
    void OnEnemyDestroyed()
    {
        --_liveEnemies;
        if (_liveEnemies == 0)
        {
            StartNextWave();
        }
    }
}
