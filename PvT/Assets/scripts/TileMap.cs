using UnityEngine;
using System.Collections.Generic;

// [ExecuteInEditMode] there are some issues with this, see comment in Start()
public class TileMap : MonoBehaviour
{
    public TextAsset Level;
    public Texture2D TileAsset;
    public int SpriteSortingOrder = -10;
    public GameObject Borders;

    struct Tile
    {
        public readonly int id;
        public readonly int x;
        public readonly int y;
        public Tile(int id, int x, int y)
        {
            this.id = id;
            this.x = x;
            this.y = y;
        }
        static public Tile FromString(string str)
        {
            var parts = str.Split('-');
            return new Tile(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }
    }
	void Start()
    {
        // need this for executing in the editor.  Would be better to have a destructor equivalent to run the clean up.
        // KAI: doesn't currently work - need to understand how to delete/manage GameObject instances while in edit mode.
        // Calling Destroy in edit mode fails.
        //Consts.RemoveAllChildren(transform); 

        var tileSpecs = Level.text.Split(',');
        int maxHorz = 0;
        int maxVert = 0;

        var parsedTiles = new List<Tile>();
        foreach (var tileSpec in tileSpecs)
        {
            var parsedTile = Tile.FromString(tileSpec);
            parsedTiles.Add(parsedTile);

            if (parsedTile.x > maxHorz)
            {
                maxHorz = parsedTile.x;
            }
            if (parsedTile.y > maxVert)
            {
                maxVert = parsedTile.y;
            }
        }

        var tiles = new Tile[maxHorz + 1, maxVert + 1];
        for (var i = 0; i < parsedTiles.Count; ++i)
        {
            tiles[i % (maxHorz + 1), i / (maxVert + 1)] = parsedTiles[i];
        }

        var sprites = Resources.LoadAll<Sprite>("tiles/tiled");
        Render(tiles, sprites);
	}

    void Render(Tile[,] tiles, Sprite[] sprites)
    {
        var i = 0;
        var rows = tiles.GetUpperBound(0);
        var cols = tiles.GetUpperBound(1);
        var size = sprites[0].bounds.size;
        var centerOffset = new Vector2(rows * size.x / 2, cols * size.y / 2);
        foreach (var tile in tiles)
        {
            var go = new GameObject("tile" + i++);
            go.transform.parent = transform;
            go.AddComponent<SpriteRenderer>();

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprites[tile.id];
            renderer.sortingOrder = SpriteSortingOrder;

            go.transform.localPosition = new Vector2(
                size.x * tile.x - centerOffset.x,
                -size.y * tile.y + centerOffset.y
            );
        }
    }
}
