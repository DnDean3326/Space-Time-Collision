using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    private List<ConsumableInfo> soldConsumables = new List<ConsumableInfo>();

    private int maxConsumables;
    private int maxTalisman;

    private RunInfo runInfo;
    private ItemManager itemManager;

    private void Awake()
    {
        runInfo = FindFirstObjectByType<RunInfo>();
        itemManager = FindFirstObjectByType<ItemManager>();
    }

    private void Start()
    {
        for (int i = 0; i < maxConsumables; i++) {
            ConsumableInfo consumable = itemManager.GetRandomConsumable();
            soldConsumables.Add(consumable);
        }
    }

    public void BuyConsumable(int index)
    {
        if (runInfo.GetFunds() > soldConsumables[index].consumablePrice) {
            runInfo.ChangeFunds(-soldConsumables[index].consumablePrice);
            runInfo.AddConsumable(soldConsumables[index]);
            soldConsumables[index] = null;
        }
    }
}
