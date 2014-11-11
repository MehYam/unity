using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PvT.Util;

public class LevelScript : MonoBehaviour
{
    public bool mobs = true;
    public int startLevel = 1;
    public float spawnDistanceFromEdge = 4;

    public GameObject map;
    public GameObject[] rooms;
    void Start()
    {
        DebugUtil.Log(this, "Start");
        Main.Instance.hud.curtain.Fade(1, 0);

        GlobalGameEvent.Instance.MobDeath += OnMobDeath;
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
        else
        {
            Main.Instance.hud.curtain.Fade(0, 0);
        }
    }

    IEnumerator RunLevels()
    {
        var main = Main.Instance;
        for (int iLevel = startLevel; iLevel < main.game.loader.levels.Count; ++iLevel)
        {
            var level = main.game.loader.levels[iLevel];

            yield return StartCoroutine(RunLevel(level, iLevel));
        }
    }

    IEnumerator RunLevel(Level level, int chapterNumber)
    {
        Debug.Log("LevelScript.RunLevel starting chapter " + chapterNumber);
        foreach (var evt in level.events)
        {
            if (evt is Level.MobSpawnEvent)
            {
                yield return StartCoroutine(RunMobSpawnEvent((Level.MobSpawnEvent)evt));
            }
            else if (evt is Level.ScriptEvent)
            {
                var scriptEvent = (Level.ScriptEvent)evt;
                var main = Main.Instance;

                switch(scriptEvent.id) {
                    case "EVENT_CHAPTER":
                        AnimatedText.FadeIn(main.hud.centerPrints.top, "Chapter " + chapterNumber, 0);
                        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

                        AnimatedText.FadeOut(main.hud.centerPrints.top, Consts.TEXT_FADE_SECONDS);
                        break;
                    case "EVENT_DROP_CURTAIN":
                        Main.Instance.hud.curtain.Fade(1, Consts.TEXT_FADE_SECONDS_FAST);
                        break;
                    case "EVENT_RAISE_CURTAIN":
                        Main.Instance.hud.curtain.Fade(0, Consts.TEXT_FADE_SECONDS_FAST);
                        break;
                    case "EVENT_COMMENT_1":
                        break;
                }
            }
        }        
        yield return null;
    }

    HashSet<GameObject> _mobsSpawned = new HashSet<GameObject>();
    IEnumerator RunMobSpawnEvent(Level.MobSpawnEvent evt)
    {
        var game = Main.Instance.game;
        foreach (var mobs in evt.mobs)
        {
            for (int i = 0; i < mobs.count; ++i)
            {
                SpawnMob(game, mobs.mobID);
                yield return new WaitForSeconds(Consts.MOB_SPAWN_DELAY);
            }
        }

        // wait until all the enemies are dead - but spawn randomly too
        var spawnLimiter = new RateLimiter(Consts.GREENK_SPAWN_RATE, 0.5f);
        while (_mobsSpawned.Count > 0)
        {
            spawnLimiter.Start();
            yield return StartCoroutine(Util.YieldUntil(() =>
                _mobsSpawned.Count == 0 || spawnLimiter.reached
            ));

            if (_mobsSpawned.Count > 0 && _mobsSpawned.Count < 6 && spawnLimiter.reached)
            {
                // always keep a mob handy in case the player needs to recapture one
                SpawnMob(game, "GREENK");
            }
        }
    }

    void SpawnMob(IGame game, string id)
    {
        var mob = game.SpawnMob(id);
        var mobActor = mob.GetComponent<Actor>();
        mobActor.isCapturable = true;

        MobAI.Instance.AttachAI(mobActor);

        PositionMob(mob, spawnDistanceFromEdge);
        _mobsSpawned.Add(mob.gameObject);
    }
    void OnMobDeath(Actor mob)
    {
        _mobsSpawned.Remove(mob.gameObject);
    }

    static void PositionMob(GameObject mob, float offset)
    {
        // put the actor at the edge
        Vector3 spawnLocation;
        var bounds = new XRect(Main.Instance.game.WorldBounds);
        bounds.Inflate(offset);

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
}
