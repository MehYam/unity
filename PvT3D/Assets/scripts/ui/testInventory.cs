using UnityEngine;

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

    Components.Schematic inventory = new Components.Schematic();
	void Start()
    {
        AddInventoryItem(powerModulePrefab);
        AddInventoryItem(autofirePrefab);
        AddInventoryItem(chargerPrefab);
        AddInventoryItem(emitterPrefab);

        schematicUI.ItemDropped += OnItemDropped;
    }
    void OnDestroy()
    {
        schematicUI.ItemDropped -= OnItemDropped;
    }
    void OnItemDropped(InventoryItemUI inventory, Point<int> pos)
    {
        Debug.LogFormat("Dropped {0} to {1}", inventory.name, pos);
    }
    void AddInventoryItem(GameObject prefab)
    {
        var go = GameObject.Instantiate(prefab);
        var item = go.GetComponent<InventoryItemUI>();

        inventoryUI.AddItem(item);
    }
}
