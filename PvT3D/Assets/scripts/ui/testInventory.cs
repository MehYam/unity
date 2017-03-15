using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using kaiGameUtil;

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

    readonly Components.Schematic inventory = new Components.Schematic(3, 6);
    readonly Components.Schematic schematic = new Components.Schematic(5, 3);
	void Start()
    {
        StartCoroutine(DelayedStart());  // because the grids are never ready on the first frame...
    }
    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1);

        LoadInventory();
        PopulateInventoryUI();

        schematicUI.ItemDropped += OnItemPlacedInSchematic;
        inventoryUI.ItemDropped += OnItemPlacedInInventory;

        yield return 0;
    }
    void OnDestroy()
    {
        schematicUI.ItemDropped -= OnItemPlacedInSchematic;
        inventoryUI.ItemDropped -= OnItemPlacedInInventory;
    }
    void LoadInventory()
    {
        inventory.grid.Set(0, 0, new Components.PowerModule("power", 1));
        inventory.grid.Set(1, 0, new Components.ChargerAutofire("autofire", 10));
        inventory.grid.Set(2, 0, new Components.Charger("charger", 10));
        inventory.grid.Set(0, 1, new Components.Emitter("emitter"));
    }
    void PopulateInventoryUI()
    {
        // set up the map of model objects to UI objects
        var mapComponentToPrefab = new Dictionary<System.Type, GameObject>();
        mapComponentToPrefab[typeof(Components.PowerModule)] = powerModulePrefab;
        mapComponentToPrefab[typeof(Components.ChargerAutofire)] = autofirePrefab;
        mapComponentToPrefab[typeof(Components.Charger)] = chargerPrefab;
        mapComponentToPrefab[typeof(Components.Emitter)] = emitterPrefab;

        // walk the inventory object, populating the UI as necessary
        inventory.grid.ForEach((x, y, component) =>
        {
            if (component != null)
            {
                Debug.LogFormat("adding component {0} at {1},{2}", component, x, y);
                var prefab = mapComponentToPrefab[component.GetType()];
                AddInventoryItem(prefab, new Point<int>(x, y));
            }
        });
    }
    void AddInventoryItem(GameObject prefab, Point<int> at)
    {
        var go = GameObject.Instantiate(prefab);
        var item = go.GetComponent<InventoryItemUI>();

        inventoryUI.AddItem(item, at);
    }
    void OnItemPlacedInSchematic(InventoryItemUI item, Point<int> pos)
    {
        Debug.LogFormat("Dropped {0} to {1} in schematic", item.name, pos);
    }
    void OnItemPlacedInInventory(InventoryItemUI item, Point<int> pos)
    {
        Debug.LogFormat("Dropped {0} to {1} in inventory", item.name, pos);
    }
}
