using UnityEngine;
using System.Collections;

public sealed class GameState : MonoBehaviour
{
    public GameObject Explosion;

    static GameState _instance;
    GameState()
    {
        _instance = this;
    }
    static public GameState Instance { get { return _instance; } }

    public void HandleCollision(Vector2 where)
    {
        var boom = (GameObject)GameObject.Instantiate(Explosion);
        boom.transform.localPosition = where;

        var anim = boom.GetComponent<Animation>();
        anim.Play();
    }
}
