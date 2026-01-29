using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    private List<Consumable> soldConsumables = new List<Consumable>();

    [SerializeField] private Image[] consumableImages;
    [SerializeField] private Image[] talismanImages;
    [SerializeField] private TextMeshProUGUI[] consumableText;
    [SerializeField] private TextMeshProUGUI[] talismanText;
    [SerializeField] private Button[] consumableButtons;
    [SerializeField] private Button[] talismanButtons;
    
    [SerializeField] private TextMeshProUGUI itemText;
    [SerializeField] private TextMeshProUGUI fundDisplay;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private int maxConsumables = 4;
    [SerializeField] private int maxTalisman = 4;

    private RunInfo runInfo;
    private ItemManager itemManager;

    private void Awake()
    {
        runInfo = FindFirstObjectByType<RunInfo>();
        itemManager = FindFirstObjectByType<ItemManager>();
    }

    private void Start()
    {
        fundDisplay.text = "$" + runInfo.GetFunds();
        for (int i = 0; i < maxConsumables; i++) {
            Consumable consumable = itemManager.GetRandomConsumable();
            soldConsumables.Add(consumable);
        }
        SetConsumables();
        UpdateShopButtons();
    }
    
    private void UpdateShopButtons()
    {
        fundDisplay.text = "$" + runInfo.GetFunds();
        for (int i = 0; i < maxConsumables; i++) {
            consumableButtons[i].interactable = runInfo.GetFunds() > soldConsumables[i].GetPrice();
        }
    }
    
    private void SetConsumables()
    {
        for (int i = 0; i < maxConsumables; i++) {
            Consumable consumable = itemManager.GetRandomConsumable();
            soldConsumables.Add(consumable);
            consumableImages[i].sprite = consumable.GetIcon();
            consumableText[i].text = "$" + consumable.GetPrice();
        }
    }
    
    private void SetTalisman()
    {
        
    }

    public void BuyConsumable(int index)
    {
        if (runInfo.GetFunds() > soldConsumables[index].GetPrice()) {
            runInfo.ChangeFunds(-soldConsumables[index].GetPrice());
            runInfo.AddConsumable(soldConsumables[index]);
            soldConsumables[index] = null;
        }
        UpdateShopButtons();
    }
    
    public void BuyTalisman(int index)
    {
        
    }
}
