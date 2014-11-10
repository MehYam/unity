using UnityEngine;
using System.Collections;

using PvT.Util;

public class DropShadow : MonoBehaviour
{
    public float distance = 0.08f;
    public float distanceModifier = 0;
    public float angle = 45;

    struct Renderers
    {
        public readonly SpriteRenderer source;
        public readonly SpriteRenderer shadow;
        public Renderers(SpriteRenderer source, SpriteRenderer shadow) { this.source = source; this.shadow = shadow; }
    }
    Renderers _state;
    void Start()
    {
        var shadow = new GameObject();
        var shadowRenderer = shadow.AddComponent<SpriteRenderer>();

        _state = new Renderers(GetComponent<SpriteRenderer>(), shadowRenderer);

        shadow.name = "dropshadow";
        shadow.transform.parent = transform;
        shadow.transform.localScale = Vector3.one;
        shadow.transform.rotation = transform.rotation;
        
        shadowRenderer.sprite = _state.source.sprite;
        shadowRenderer.material = Resources.Load<Material>("DropShadowMaterial");
        shadowRenderer.sortingLayerID = _state.source.sortingLayerID;
        shadowRenderer.sortingOrder = -1;

        Util.SetAlpha(_state.shadow, 0);

        LateUpdate();
	}
	
	void LateUpdate()
    {
        var trueDistance = distance + distanceModifier;
        _state.shadow.transform.position = transform.position + new Vector3(trueDistance, -trueDistance);

        if (_state.shadow.color.a != _state.source.color.a)
        {
            Util.SetAlpha(_state.shadow, _state.source.color.a);
        }
    }
}
