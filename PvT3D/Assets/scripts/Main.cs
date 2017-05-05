using UnityEngine;
using System.Collections;
using System;

public sealed class Main : MonoBehaviour, IGame
{
    [SerializeField] GameObject _actorParent;
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] GameObject _enemyPrefab;
    [SerializeField] GameObject _smallExplosionPrefab;
    [SerializeField] GameObject _bigExplosionPrefab;
    [SerializeField] GameObject _plasmaExplosionPrefab;
    void Start()
    {
        //Physics.gravity = Vector3.zero;
        game = this;
	}
    void OnDestroy()
    {
        game = null;
    }

    /// <summary>
    /// IGame implementation
    /// </summary>
    static public IGame game
    {
        get; private set;
    }
    Actor _player;
    public Actor player
    {
        get
        {
            return _player;
        }
        set
        {
            _player = value;
            Camera.main.GetComponent<CameraFollow>().target = _player.gameObject;
        }
    }
    public GameObject actorParent { get { return _actorParent; } }
    public GameObject defaultPlayerPrefab { get { return _playerPrefab; } }
    public GameObject defaultEnemyPrefab { get { return _enemyPrefab; } }
    public GameObject smallExplosionPrefab { get { return _smallExplosionPrefab; } }
    public GameObject bigExplosionPrefab { get { return _bigExplosionPrefab; } }
    public GameObject plasmaExplosionPrefab { get { return _plasmaExplosionPrefab; } }
}
