using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PvT.Util;

public class LevelScript : MonoBehaviour
{
    public bool mobs = true;
    public GameObject map;
    public GameObject[] rooms;
    void Start()
    {
        DebugUtil.Log(this, "Start");
        Main.Instance.hud.curtain.Fade(1, 0);
        Main.Instance.hud.curtain.Fade(0, Consts.TEXT_FADE_SECONDS_FAST);

        GlobalGameEvent.Instance.EnemyDestroyed += OnEnemyDestroyed;
        GlobalGameEvent.Instance.GameReady += OnGameReady;

        Main.Instance.game.SetMap(map);
    }
    void OnGameReady(IGame game)
    {
        Debug.Log("LevelScript.OnGameReady");
        Main.Instance.game.SpawnPlayer(Vector3.zero);

        if (mobs)
        {
            StartCoroutine(RunLevels());
        }
    }

    IEnumerator RunLevels()
    {
        int iLevel = 0;

        var main = Main.Instance;
        foreach (var level in main.game.loader.levels)
        {
            AnimatedText.FadeIn(main.hud.centerPrintTop, "Chapter " + ++iLevel, Consts.TEXT_FADE_SECONDS);
            yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

            AnimatedText.FadeOut(main.hud.centerPrintTop, Consts.TEXT_FADE_SECONDS);

            yield return StartCoroutine(RunLevel(level));

            Main.Instance.hud.curtain.Fade(1, Consts.TEXT_FADE_SECONDS);
            yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

            Main.Instance.hud.curtain.Fade(0, Consts.TEXT_FADE_SECONDS_FAST);
        }
    }

    int _liveEnemies;
    IEnumerator RunLevel(Level level)
    {
        var game = Main.Instance.game;

        foreach (var wave in level.waves)
        {
            _liveEnemies = 0;
            foreach (var squad in wave.squads)
            {
                for (int i = 0; i < squad.count; ++i)
                {
                    SpawnMob(game, squad.enemyID);

                    yield return new WaitForSeconds(Consts.MOB_SPAWN_DELAY);
                }
            }

            // wait until all the enemies are dead - but spawn randomly too
            var spawnLimiter = new RateLimiter(Consts.GREENK_SPAWN_RATE, 0.5f);
            while (_liveEnemies > 0)
            {
                spawnLimiter.Start();
                yield return StartCoroutine(Util.YieldUntil(() =>
                    _liveEnemies == 0 || spawnLimiter.reached
                ));

                if (_liveEnemies > 0 && _liveEnemies < 6 && spawnLimiter.reached)
                {
                    // always keep a mob handy in case the player needs to recapture one
                    SpawnMob(game, "GREENK");
                }
            }
        }        
        yield return null;
    }

    void SpawnMob(IGame game, string id)
    {
        var mob = game.SpawnMob(id);
        PositionMob(mob);

        ++_liveEnemies;
    }

    static void PositionMob(GameObject mob)
    {
        // put the actor at the edge
        Vector3 spawnLocation;
        var bounds = new XRect(Main.Instance.game.WorldBounds);
        bounds.Inflate(-2);

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
    }
}
