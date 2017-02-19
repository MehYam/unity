using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class DropHandler : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop " + name);

        if (GetComponentInChildren<DragHandler>() == null)
        {
            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
}
