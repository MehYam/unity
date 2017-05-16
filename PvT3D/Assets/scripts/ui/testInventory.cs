using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using kaiGameUtil;
using ShipComponent = PvT3D.ShipComponent;

public class testInventory : MonoBehaviour
{
    [SerializeField]
    InventoryUI inventoryUI = null;
    [SerializeField]
    InventoryUI schematicUI = null;

    [SerializeField]
    GameObject powerModulePrefab = null;
    [SerializeField]
    GameObject chargerPrefab = null;
    [SerializeField]
    GameObject emitterPrefab = null;

    readonly ShipComponent.Schematic inventory = new ShipComponent.Schematic(3, 6);
    readonly ShipComponent.Schematic schematic = new ShipComponent.Schematic(5, 3);
	void Start()
    {
        StartCoroutine(DelayedStart());  
    }
    IEnumerator DelayedStart()
    {
        // because the grids are never ready to render on the first frame.
        yield return new WaitForEndOfFrame();

        LoadInventory();
        PopulateInventoryUI();

        schematicUI.ItemBeginDrag += OnItemDraggingFromSchematic;
        inventoryUI.ItemBeginDrag += OnItemDraggingFromInventory;
        schematicUI.ItemDropped += OnItemPlacedInSchematic;
        inventoryUI.ItemDropped += OnItemPlacedInInventory;
    }
    void OnDestroy()
    {
        schematicUI.ItemBeginDrag -= OnItemDraggingFromSchematic;
        inventoryUI.ItemBeginDrag -= OnItemDraggingFromInventory;
        schematicUI.ItemDropped -= OnItemPlacedInSchematic;
        inventoryUI.ItemDropped -= OnItemPlacedInInventory;
    }
    void LoadInventory()
    {
        inventory.grid.Set(0, 0, new ShipComponent.Power("P", 1));
        inventory.grid.Set(1, 0, new ShipComponent.Charger("C", 10));
        inventory.grid.Set(2, 0, new ShipComponent.Charger("C", 10));
        inventory.grid.Set(0, 1, new ShipComponent.Speed("A", 100));
    }
    void PopulateInventoryUI()
    {
        // set up the map of model objects to UI objects
        var mapComponentToPrefab = new Dictionary<System.Type, GameObject>()
        {
            { typeof(ShipComponent.Power), powerModulePrefab },
            { typeof(ShipComponent.Charger), chargerPrefab },
            { typeof(ShipComponent.Speed), emitterPrefab }
        };

        // walk the inventory object, populating the UI as necessary
        inventory.grid.ForEach((x, y, component) =>
        {
            if (component != null)
            {
                //Debug.LogFormat("adding component {0} at {1},{2}", component, x, y);
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
    class DragState
    {
        public readonly ShipComponent.Schematic from;
        public readonly Point<int> pos;

        public DragState(ShipComponent.Schematic from, Point<int> pos) { this.from = from; this.pos = pos; }
    }
    DragState dragState;
    void OnItemDraggingFromSchematic(InventoryItemUI item, Point<int> pos)
    {
        dragState = new DragState(schematic, pos);
    }
    void OnItemDraggingFromInventory(InventoryItemUI item, Point<int> pos)
    {
        dragState = new DragState(inventory, pos);
    }
    void OnItemPlacedInSchematic(InventoryItemUI item, Point<int> pos)
    {
        var component = dragState.from.grid.Get(dragState.pos);
        dragState.from.grid.Set(dragState.pos, null);
        schematic.grid.Set(pos, component);
        dragState = null;
    }
    void OnItemPlacedInInventory(InventoryItemUI item, Point<int> pos)
    {
        var component = dragState.from.grid.Get(dragState.pos);

        dragState.from.grid.Set(dragState.pos, null);
        inventory.grid.Set(pos, component);
        dragState = null;
    }
}
