using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
    void DestroySlots(int n)
    {
        var slotObjects = transform.GetComponentsInChildren<SlotUI>();
        n = Mathf.Min(slotObjects.Length, n);

        Debug.LogFormat("destroy {0} slots of {1}", n, slotObjects.Length);
        while (n-- > 0)
        {
#if UNITY_EDITOR
            DestroyImmediate(slotObjects[n].gameObject);
#else
            Destroy(slots[n].gameObject);
#endif
        }
    }
}
