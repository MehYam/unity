using UnityEngine;
using System.Collections;
using System;

public sealed class Main : MonoBehaviour, IGame
{
    [SerializeField] GameObject _actorParent = null;
    [SerializeField] GameObject _ammoParent = null;
    [SerializeField] GameObject _effectParent = null;

    [SerializeField] GameObject _playerPrefab = null;
    [SerializeField] GameObject _enemyPrefab = null;
    [SerializeField] GameObject _enemyPrefab2 = null;
    [SerializeField] GameObject _smallExplosionPrefab = null;
    [SerializeField] GameObject _bigExplosionPrefab = null;
    [SerializeField] GameObject _plasmaExplosionPrefab = null;
    [SerializeField] GameObject _damageSmokePrefab = null;
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
    public GameObject ammoParent { get { return _ammoParent; } }
    public GameObject effectParent { get { return _effectParent; } }

    public GameObject defaultPlayerPrefab { get { return _playerPrefab; } }
    public GameObject defaultEnemyPrefab { get { return _enemyPrefab; } }
    public GameObject defaultEnemyPrefab2 { get { return _enemyPrefab2; } }
    public GameObject smallExplosionPrefab { get { return _smallExplosionPrefab; } }
    public GameObject bigExplosionPrefab { get { return _bigExplosionPrefab; } }
    public GameObject plasmaExplosionPrefab { get { return _plasmaExplosionPrefab; } }
    public GameObject damageSmokePrefab { get { return _damageSmokePrefab; } }
}
