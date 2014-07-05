using UnityEngine;
using System.Collections;

using PvT.Util;

/// <summary>
///  This is essentially a DIY skybox for 2D using a Sprite.  The built-in skybox is transforming my images
///  in ways I don't like, and using a GameObjects allows you flexibility to fade, crossfade, etc, easily.
/// </summary>
public class SceneCurtain : MonoBehaviour
{
    SpriteRenderer sprite;
    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();

        var rect = Util.GetScreenRectInWorldCoords(Camera.main);
        var bounds = sprite.bounds;

        var scaleX = rect.width / bounds.size.x;
        var scaleY = rect.height / bounds.size.y;

        // scale up to cover the entire screen, preserving the aspect ratio of the original image 
        var scale = Mathf.Max(scaleX, scaleY);
        sprite.transform.localScale = new Vector3(scale, scale, 1);
    }
}
