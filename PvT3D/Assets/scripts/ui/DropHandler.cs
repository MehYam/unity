using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class DropHandler : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (GetComponentInChildren<DragHandler>() == null)
        {
            eventData.pointerDrag.transform.SetParent(transform, false);

            var rt = eventData.pointerDrag.transform.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.one;
        }
    }
}
