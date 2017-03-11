using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class DropHandler : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        //KAI: this should instead reflect an OnDrop call or event down to the draggee
        if (GetComponentInChildren<DragHandler>() == null)
        {
            eventData.pointerDrag.transform.SetParent(transform, false);

            var rt = eventData.pointerDrag.transform.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.one;
        }

        //KAI: hard time putting into words how bad this is, but we're in "it works for now" mode
        var inventoryParent = transform.parent.GetComponent<InventoryUI>();
        var slot = transform.GetComponent<SlotUI>();
        if (inventoryParent != null && slot != null)
        {
            GlobalEvent.Instance.FireInventoryItemMoved(inventoryParent, slot);
        }
    }
}
