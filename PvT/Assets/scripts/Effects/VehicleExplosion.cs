using UnityEngine;
using System.Collections;

using PvT.Util;

public class VehicleExplosion : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(RunExplosion());
    }

    IEnumerator RunExplosion()
    {
        var game = Main.Instance.game;

        var explosion = game.effects.GetRandomExplosion().ToRawGameObject(Consts.SortingLayer.EXPLOSIONS);
        explosion.transform.parent = transform;
        explosion.transform.localPosition = Util.ScatterRandomly(0.3f);

        yield return new WaitForSeconds(0.1f);

        for (var i = 0; i < 4; ++i)
        {
            var smoke = game.effects.GetRandomSmoke().ToRawGameObject(Consts.SortingLayer.SMOKE);
            smoke.transform.parent = transform;

            smoke.transform.Rotate(0, 0, Random.Range(0, 360));
            smoke.transform.localPosition = Util.ScatterRandomly(0.25f);

            explosion = game.effects.GetRandomSmallExplosion().ToRawGameObject(Consts.SortingLayer.EXPLOSIONS);
            explosion.transform.parent = transform;
            explosion.transform.localPosition = Util.ScatterRandomly(0.5f);

            yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));
        }
    }
}
