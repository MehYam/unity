using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IDropHandler
{
    public int index = -1;

    public void OnDrop(PointerEventData eventData)
    {
        //KAI: this should instead reflect an OnDrop call or event down to the draggee
        if (GetComponentInChildren<InventoryItemUI>() == null)
        {
            eventData.pointerDrag.transform.SetParent(transform, false);

            var rt = eventData.pointerDrag.transform.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.one;
        }

        var inventoryParent = transform.parent.GetComponent<InventoryUI>();
        if (inventoryParent != null)
        {
            inventoryParent.OnItemDropped(this, eventData.pointerDrag.GetComponent<InventoryItemUI>());
        }
    }
}
