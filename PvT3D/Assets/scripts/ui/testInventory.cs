using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testInventory : MonoBehaviour
{
    [SerializeField]
    InventoryUI inventory;
    [SerializeField]
    InventoryUI schematic;

    [SerializeField]
    GameObject powerModulePrefab;
    [SerializeField]
    GameObject autofirePrefab;
    [SerializeField]
    GameObject chargerPrefab;
    [SerializeField]
    GameObject emitterPrefab;

	// Use this for initialization
	void Start()
    {
        GlobalEvent.Instance.InventoryItemMoved += OnItemMoved;

        inventory.AddItem(GameObject.Instantiate(powerModulePrefab));
	}
    void OnItemMoved(InventoryUI inventory, SlotUI slot)
    {
        //Debug.LogFormat("OnItemMoved {0}, {1}", inventory.name, slot.name);
    }
}
