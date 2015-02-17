using UnityEngine;
using System.Collections;

public sealed class Map : MonoBehaviour
{
    public GameObject tilePrefab;
    public Sprite foo;

	// Use this for initialization
	void Start()
    {
	    var renderer = (SpriteRenderer) tilePrefab.renderer;

        Debug.Log(renderer.sprite.rect);
        Debug.Log(renderer.sprite.textureRect);

        var obj = (GameObject)Instantiate(tilePrefab);
        renderer = (SpriteRenderer) obj.renderer;
        //renderer.sprite.textureRect = new Rect(50, 50, 150, 150);
    }
}
