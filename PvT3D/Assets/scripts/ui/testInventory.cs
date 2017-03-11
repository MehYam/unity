using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testInventory : MonoBehaviour
{
    Components.Schematic schematic = new Components.Schematic();

	// Use this for initialization
	void Start()
    {
        GlobalEvent.Instance.InventoryItemMoved += OnItemMoved;
	}
    void OnItemMoved(InventoryUI inventory, SlotUI slot)
    {
        Debug.LogFormat("OnItemMoved {0}, {1}", inventory.name, slot.name);
    }
}
