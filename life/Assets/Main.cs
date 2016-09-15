using UnityEngine;
using System.Collections;

using lifeEngine;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
    public Sprite[] glyphs;
    void Start()
    {
        Debug.Log("System.Environment.Version " + System.Environment.Version);

        var layer = Operations.LoadLayerFile("c:\\source\\cs\\life\\simplerooms1.txt");
        RenderLayer(layer);
    }

    void RenderLayer(Layer<Tile> layer)
    {
        var glyphLookup = new Dictionary<char, int>();
        glyphLookup[' '] = 5;
        glyphLookup['#'] = 6;
        glyphLookup['.'] = 3;

        Vector2 offset = new Vector2(layer.width / 2, layer.height / 2);
        layer.ForEach((x, y, tile) =>
        {
            var sprite = glyphs[glyphLookup[tile.type]];

            var gobj = new GameObject(string.Format("tile_{0:000}_{1:000}", x, y));
            gobj.transform.parent = gameObject.transform;

            var renderer = gobj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;

            gobj.transform.position = new Vector2(x, y) - offset;
        });
    }
}
