using UnityEngine;
using System.Collections;

public class LevelScript : MonoBehaviour
{
    void Start()
    {
        GlobalGameEvent.Instance.EnemyDestroyed += OnEnemyDestroyed;
        GlobalGameEvent.Instance.GameReady += OnGameReady;
    }
    void OnGameReady(IGame game)
    {
        game.SpawnPlayer(Vector3.zero);
        StartNextLevel();
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
                    game.SpawnMob(squad.enemyID);
                    ++_liveEnemies;
                }
            }
        }
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
