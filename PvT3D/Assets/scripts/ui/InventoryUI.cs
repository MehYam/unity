using UnityEngine;
using kaiGameUtil;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    GameObject slotPrefab;
    public int slots
    {
        get
        {
            var slotObjects = transform.GetComponentsInChildren<InventorySlotUI>();
            return slotObjects.Length;
        }
        set
        {
            var delta = value - slots;
            if (delta > 0)
            {
                CreateSlots(delta);
            }
            else if (delta < 0)
            {
                DestroySlots(-delta);
            }
        }
    }
    public void AddItem(InventoryItemUI go)
    {
        var slotObjects = transform.GetComponentsInChildren<InventorySlotUI>();

        // find the next empty slot
        foreach (var slot in slotObjects)
        {
            var item = slot.transform.GetComponentInChildren<InventoryItemUI>();
            if (item == null)
            {
                go.transform.SetParent(slot.transform, false);
                break;
            }
        }
    }
    void CreateSlots(int n)
    {
        if (slotPrefab != null)
        {
            for (int i = 0; i < n; ++i)
            {
                var slot = GameObject.Instantiate(slotPrefab);
                slot.transform.SetParent(transform, false);
            }
        }
        FixUpSlots();
    }
    void DestroySlots(int n)
    {
        var slotObjects = transform.GetComponentsInChildren<InventorySlotUI>();
        n = Mathf.Min(slotObjects.Length, n);
        while (n-- > 0)
        {
#if UNITY_EDITOR
            DestroyImmediate(slotObjects[n].gameObject);
#else
            Destroy(slots[n].gameObject);
#endif
        }
        FixUpSlots();
    }
    void FixUpSlots()
    {
        var slotObjects = transform.GetComponentsInChildren<InventorySlotUI>();
        var n = 0;
        foreach (var slot in slotObjects)
        {
            slot.index = n;
            slot.name = "slot " + n++;
        }
    }
    /// <summary>
    /// MIND BLOWING.  GridLayoutGroup gives you no facility to know the row/col of an element.  So we'll figure it
    /// out ourselves, and cache the values in the SlotUI for convenience.
    /// 
    /// You can iterate the cell x,y coordinates to divine the row,col position, but only if you wait a frame for the
    /// grid to do layout.
    /// </summary>
    Point<int> SlotIndexToPoint(int slotIndex)
    {
        var slotObjects = transform.GetComponentsInChildren<InventorySlotUI>();
        if (slotObjects.Length > 0)
        {
            var parentSize = GetComponent<RectTransform>().rect;
            var cellSize = slotObjects[0].GetComponent<RectTransform>().rect;

            int columnWidth = (int)(parentSize.x / cellSize.x);
            int columns = slotObjects.Length < columnWidth ? (slotObjects.Length % columnWidth) : columnWidth;
            int rows = slotObjects.Length / columnWidth;

            return new Point<int>(slotIndex % columnWidth, slotIndex / columnWidth);
        }
        return new Point<int>(-1, -1);
    }
    public void OnItemDropped(InventorySlotUI slot, InventoryItemUI item)
    {
        Debug.LogFormat("{0} dropped on {1}, position {2}", item.name, slot.index, SlotIndexToPoint(slot.index));
    }
}
