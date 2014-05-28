using UnityEngine;
using System.Collections;

public class DropShadow : MonoBehaviour
{
    public float distance = 0.06f;
    public float angle = 45;

    GameObject _shadow;
	void Start()
    {
        var sourceRenderer = GetComponent<SpriteRenderer>();

        var shadow = new GameObject();
        shadow.name = "dropshadow";
        shadow.transform.localScale = transform.localScale;
        shadow.transform.localRotation = transform.localRotation;

        shadow.transform.parent = transform;

        var shadowRenderer = shadow.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = sourceRenderer.sprite;
        shadowRenderer.material = Resources.Load<Material>("DropShadowMaterial");

        _shadow = shadow;
	}
	
	void Update()
    {
        _shadow.transform.position = transform.position + new Vector3(distance, -distance);
    }
}
