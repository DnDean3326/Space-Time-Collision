using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private List<ConsumableInfo> allConsumables;

    [SerializeField] private int maxPriceIncrease = 5;

    public ConsumableInfo GetRandomConsumable()
    {
        ConsumableInfo consumable = allConsumables[Random.Range(0, allConsumables.Count)];
        consumable.consumablePrice += Random.Range(0, (maxPriceIncrease + 1));
        return consumable;
    }
}
