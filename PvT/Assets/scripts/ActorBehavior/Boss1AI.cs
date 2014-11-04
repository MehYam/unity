using UnityEngine;
using System.Collections;

public sealed class Boss1AI : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Script());
    }

    IEnumerator Script()
    {
        var main = Main.Instance;

        transform.position = Vector2.up * main.game.WorldBounds.top;
        transform.eulerAngles = new Vector3(0, 0, 180);

        main.game.PlaySound(main.sounds.roar, Camera.main.transform.position);

        Main.Instance.game.ShakeGround();

        yield return new WaitForSeconds(3);
    }
}
