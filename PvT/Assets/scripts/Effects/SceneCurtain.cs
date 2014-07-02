using UnityEngine;
using System.Collections;

using PvT.Util;

public class SceneCurtain : MonoBehaviour
{
    SpriteRenderer sprite;
    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();

        var pixels = Camera.main.pixelRect;
        sprite.transform.localScale = new Vector2(pixels.width * 2, pixels.height * 2);
    }
}
