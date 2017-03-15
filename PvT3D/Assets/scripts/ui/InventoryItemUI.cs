using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum ItemType { PowerModule, Autofire, Charger, Emitter };
    public ItemType itemType;

    // Drag handling
    class StartOfDrag
    {
        public readonly Vector3 pos;
        public readonly Transform parent;
        public readonly bool blocksRaycasts;
        public StartOfDrag(Vector3 pos, Transform parent, bool blocksRaycasts) { this.pos = pos; this.parent = parent; this.blocksRaycasts = blocksRaycasts; }
    }

    StartOfDrag start = null;
    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.parent.SendMessage("OnItemBeginDrag", this);

        start = new StartOfDrag(transform.position, transform.parent, GetComponent<CanvasGroup>().blocksRaycasts);

        GetComponent<CanvasGroup>().blocksRaycasts = false;

        transform.SetParent(transform.parent.parent.parent); //KAI:
    }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent.GetComponent<InventorySlotUI>() == null)
        {
            // InventorySlotUI.OnDrop() has not parented this item when it was dropped, snatch it back
            transform.SetParent(start.parent);
            transform.position = start.pos;
        }
        GetComponent<CanvasGroup>().blocksRaycasts = start.blocksRaycasts;
        start = null;
    }
}
