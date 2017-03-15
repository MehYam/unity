using UnityEngine;
using System;
using kaiGameUtil;

public class InventoryUI : MonoBehaviour
{
    public Action<InventoryItemUI, Point<int>> ItemDropped = delegate { };

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
    public Point<int> size
    {
        get
        {
            var retval = Util.zero;
            var slotObjects = transform.GetComponentsInChildren<InventorySlotUI>();
            if (slotObjects.Length > 0)
            {
                var parentSize = GetComponent<RectTransform>().rect;
                var cellSize = slotObjects[0].GetComponent<RectTransform>().rect;

                int columnWidth = (int)(parentSize.x / cellSize.x);
                int columns = slotObjects.Length < columnWidth ? (slotObjects.Length % columnWidth) : columnWidth;
                int rows = slotObjects.Length / columnWidth;

                retval.x = columns;
                retval.y = rows;
            }
            return retval;
        }
    }
    public void AddItem(InventoryItemUI item)
    {
        var slotObjects = transform.GetComponentsInChildren<InventorySlotUI>();

        // find the next empty slot
        foreach (var slot in slotObjects)
        {
            var existing = slot.transform.GetComponentInChildren<InventoryItemUI>();
            if (existing == null)
            {
                item.transform.SetParent(slot.transform, false);
                break;
            }
        }
    }
    public void AddItem(InventoryItemUI item, Point<int> pos)
    {
        var existingItem = GetItemAt(pos);
        if (existingItem == null)
        {
            var slot = GetSlotAt(pos);
            if (slot != null)
            {
                item.transform.SetParent(slot.transform, false);
            }
        }
    }
    public InventoryItemUI GetItemAt(Point<int> pos)
    {
        var slot = GetSlotAt(pos);
        return slot ? slot.transform.GetComponentInChildren<InventoryItemUI>() : null;
    }
    InventorySlotUI GetSlotAt(Point<int> pos)
    {
        var slotObjects = transform.GetComponentsInChildren<InventorySlotUI>();
        var index = PointToSlotIndex(pos);
        return (index < slotObjects.Length) ? slotObjects[index] : null;
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
        var s = size;
        if (s != Util.zero)
        {
            int columnWidth = s.x;
            return new Point<int>(slotIndex % columnWidth, slotIndex / columnWidth);
        }
        return new Point<int>(-1, -1);
    }
    int PointToSlotIndex(Point<int> point)
    {
        var s = size;
        return point.y * s.x + point.x;
    }

    public void OnItemBeginDrag(InventorySlotUI slot, InventoryItemUI item) //KAI: wish there was a way to hide this
    {

    }
    public void OnItemDropped(InventorySlotUI slot, InventoryItemUI item)  //KAI: wish there were a way to hide this 
    {
        var point = SlotIndexToPoint(slot.index);
        ItemDropped(item, point);
        //Debug.LogFormat("{0} dropped on {1}, position {2}", item.name, slot.index, point);
    }
}
