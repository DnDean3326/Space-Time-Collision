using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private List<ConsumableInfo> allConsumables;
    [SerializeField] private List<TalismanInfo> allTalisman;
    [SerializeField] private int maxPriceIncrease = 10;
    
    private static GameObject _instance;

    private void Awake()
    {
        if (_instance != null) {
            Destroy(gameObject);
        } else {
            _instance = gameObject;
        }
        DontDestroyOnLoad(gameObject);
    }
    
    public Consumable GetRandomConsumable()
    {
        ConsumableInfo consumable = allConsumables[Random.Range(0, allConsumables.Count)];
        Consumable newConsumable = new Consumable(consumable.consumableName, consumable.consumableIcon,
            consumable.consumablePrice, consumable.consumableAbility, consumable.itemDescription);
        newConsumable.IncreasePrice(Random.Range(0, (maxPriceIncrease + 1))); 
        return newConsumable;
    }

    public List<Talisman> GetRandomTalisman(int count)
    {
        List<Talisman> talismanList = new List<Talisman>();
        List<TalismanInfo> talismanPool = new List<TalismanInfo>();
        foreach (TalismanInfo talisman in allTalisman) {
            talismanPool.Add(talisman);
        }
        for (int i = 0; i < count; i++) {
            TalismanInfo talisman = talismanPool[Random.Range(0, talismanPool.Count)];
            talismanPool.Remove(talisman);
            Talisman newTalisman = new Talisman(talisman.talismanName, talisman.talismanIcon, talisman.talismanPrice,
                talisman.talismanDescription);
            newTalisman.IncreasePrice(Random.Range(0, (maxPriceIncrease + 1)));
            talismanList.Add(newTalisman);
        }
        return talismanList;
    }

    public int GetTalismanCount()
    {
        return allTalisman.Count;
    }
}

[Serializable]
public class Consumable
{
    [SerializeField] private string _consumableName;
    [SerializeField]private Sprite _consumableIcon;
    private int _consumablePrice;
    private Ability _linkedAbility;
    private string _itemDescription;

    public Consumable(string  consumableName, Sprite consumableIcon, int consumablePrice,  Ability linkedAbility,  string itemDescription)
    {
        _consumableName = consumableName;
        _consumableIcon = consumableIcon;
        _consumablePrice = consumablePrice;
        _linkedAbility = linkedAbility;
        _itemDescription = itemDescription;
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
    
    public string GetItemDescription()
    {
        return _itemDescription;
    }
}

[Serializable]
public class Talisman
{
    [SerializeField] private string _talismanName;
    [SerializeField] private Sprite _talismanIcon;
    private int _talismanPrice;
    private string _itemDescription;

    public Talisman(string talismanName, Sprite talismanIcon, int talismanPrice, string itemDescription)
    {
        _talismanName = talismanName;
        _talismanIcon = talismanIcon;
        _talismanPrice = talismanPrice;
        _itemDescription = itemDescription;
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
    
    public string GetItemDescription()
    {
        return _itemDescription;
    }
}
