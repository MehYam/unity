using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testInventory : MonoBehaviour
{
    [SerializeField]
    InventoryUI inventoryUI;
    [SerializeField]
    InventoryUI schematicUI;

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
        AddInventoryItem(powerModulePrefab);
        AddInventoryItem(autofirePrefab);
        AddInventoryItem(chargerPrefab);
        AddInventoryItem(emitterPrefab);
    }
    void AddInventoryItem(GameObject prefab)
    {
        var go = GameObject.Instantiate(prefab);
        var item = go.GetComponent<InventoryItemUI>();

        inventoryUI.AddItem(item);
    }
}
