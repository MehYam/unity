using UnityEngine;
using System;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    GameObject slotPrefab;
    public int slots
    {
        get
        {
            var slotObjects = transform.GetComponentsInChildren<SlotUI>();
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
    public void AddItem(GameObject go)
    {
        var slotObjects = transform.GetComponentsInChildren<SlotUI>();

        // find the next empty slot
        foreach (var slot in slotObjects)
        {
            var item = slot.transform.GetComponentInChildren<InventoryItemUI>();
            if (item == null)
            {
                go.transform.SetParent(slot.transform, false);
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
        SetSlotNames();
    }
    void DestroySlots(int n)
    {
        var slotObjects = transform.GetComponentsInChildren<SlotUI>();
        n = Mathf.Min(slotObjects.Length, n);
        while (n-- > 0)
        {
#if UNITY_EDITOR
            DestroyImmediate(slotObjects[n].gameObject);
#else
            Destroy(slots[n].gameObject);
#endif
        }
        SetSlotNames();
    }
    void SetSlotNames()
    {
        var slotObjects = transform.GetComponentsInChildren<SlotUI>();
        var n = 0;
        foreach (var slot in slotObjects)
        {
            slot.name = "slot " + n++;
        }
    }
}
