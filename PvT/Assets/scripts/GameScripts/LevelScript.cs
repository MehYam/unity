using UnityEngine;
using System.Collections;

using PvT.Util;

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
        Main.Instance.PlayMusic(Main.Instance.music.duskToDawn);
    }
    void OnGameReady(IGame game)
    {
        game.SpawnPlayer(Vector3.zero);

        StartCoroutine(Tutorial());
        StartNextLevel();
    }
    IEnumerator Tutorial()
    {
        AnimatedText.FadeIn(Main.Instance.hud.centerPrintTop, "We were dropped somewhere strange.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);


        yield break;
    }



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

        mob.GetComponent<Actor>().trackingArrow = true;
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
