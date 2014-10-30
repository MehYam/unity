using UnityEngine;
using System.Collections;

public class DropShadow : MonoBehaviour
{
    public float distance = 0.08f;
    public float distanceModifier = 0;
    public float angle = 45;

    GameObject _shadow;
	void Start()
    {
        var sourceRenderer = GetComponent<SpriteRenderer>();

        var shadow = new GameObject();
        shadow.name = "dropshadow";
        shadow.transform.parent = transform;
        shadow.transform.localScale = Vector3.one;
        shadow.transform.rotation = transform.rotation;
        
        var shadowRenderer = shadow.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = sourceRenderer.sprite;
        shadowRenderer.material = Resources.Load<Material>("DropShadowMaterial");
        shadowRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        shadowRenderer.sortingOrder = -1;

        _shadow = shadow;
	}
	
	void LateUpdate()
    {
        var trueDistance = distance + distanceModifier;
        _shadow.transform.position = transform.position + new Vector3(trueDistance, -trueDistance);
    }
}
