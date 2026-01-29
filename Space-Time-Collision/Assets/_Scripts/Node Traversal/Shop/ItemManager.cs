using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private List<ConsumableInfo> allConsumables;
    [SerializeField] private int maxPriceIncrease = 10;

    public Consumable GetRandomConsumable()
    {
        ConsumableInfo consumable = allConsumables[Random.Range(0, allConsumables.Count)];
        Consumable newConsumable = new Consumable(consumable.consumableName, consumable.consumableIcon,
            consumable.consumablePrice, consumable.consumableAbility);
        newConsumable.IncreasePrice(Random.Range(0, (maxPriceIncrease + 1))); 
        return newConsumable;
    }
}

[Serializable]
public class Consumable
{
    private string _consumableName;
    private Sprite _consumableIcon;
    private int _consumablePrice;

    private Ability _linkedAbility;

    public Consumable(string  consumableName, Sprite consumableIcon, int consumablePrice,  Ability linkedAbility)
    {
        _consumableName = consumableName;
        _consumableIcon = consumableIcon;
        _consumablePrice = consumablePrice;
        _linkedAbility = linkedAbility;
    }

    public void IncreasePrice(int priceIncrease)
    {
        _consumablePrice += priceIncrease;
    }

    public string GetName()
    {
        return _consumableName;
    }

    public Sprite GetIcon()
    {
        return _consumableIcon;
    }
    
    public int GetPrice()
    {
        return _consumablePrice;
    }

    public Ability GetLinkedAbility()
    {
        return _linkedAbility;
    }
}

[Serializable]
public class Talisman
{
    private string _talismanName;
    private Sprite _talismanIcon;
    private int _talismanPrice;

    public Talisman(string talismanName, Sprite talismanIcon, int talismanPrice)
    {
        _talismanName = talismanName;
        _talismanIcon = talismanIcon;
        _talismanPrice = talismanPrice;
    }

    public void IncreasePrice(int priceIncrease)
    {
        _talismanPrice += priceIncrease;
    }

    public string GetName()
    {
        return _talismanName;
    }

    public Sprite GetIcon()
    {
        return _talismanIcon;
    }
    
    public int GetPrice()
    {
        return _talismanPrice;
    }
}
