using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
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

    [SerializeField] private List<Consumable> consumablesForSale = new List<Consumable>();
    [SerializeField] private List<Talisman> talismansForSale = new List<Talisman>();
    private List<bool> consumableSold = new List<bool>();
    private List<bool> talismanSold = new List<bool>();
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
        SetConsumables();
        SetTalisman();
        UpdateShopButtons();
    }
    
    private void UpdateShopButtons()
    {
        fundDisplay.text = "$" + runInfo.GetFunds();
        for (int i = 0; i < maxConsumables; i++) {
            if (consumableSold[i]) {
                // Place SOLD-OUT! graphic
                consumableButtons[i].interactable = false;
                continue;
            }
            if (runInfo.GetFunds() < consumablesForSale[i].GetPrice() || runInfo.GetConsumableCount() >= 5) {
                consumableButtons[i].interactable = false;
            } else {
                consumableButtons[i].interactable = true;
            }
        }
        for (int i = 0; i < maxTalisman; i++) {
            if (talismanSold[i]) {
                // Place SOLD-OUT! graphic
                talismanButtons[i].interactable = false;
                continue;
            }
            if (runInfo.GetFunds() < consumablesForSale[i].GetPrice()) {
                talismanButtons[i].interactable = false;
            } else {
                talismanButtons[i].interactable = true;
            }
        }
    }
    
    private void SetConsumables()
    {
        for (int i = 0; i < maxConsumables; i++) {
            Consumable consumable = itemManager.GetRandomConsumable();
            consumablesForSale.Add(consumable);
            consumableImages[i].sprite = consumable.GetIcon();
            consumableText[i].text = "$" + consumable.GetPrice();
            consumableSold.Add(false);
        }
    }
    
    private void SetTalisman()
    {
        for (int i = 0; i < maxTalisman; i++) {
            Talisman talisman = itemManager.GetRandomTalisman();
            talismansForSale.Add(talisman);
            talismanImages[i].sprite = talisman.GetIcon();
            talismanText[i].text = "$" + talisman.GetPrice();
            talismanSold.Add(false);
        }
    }

    public void BuyConsumable(int index)
    {
        if (runInfo.GetFunds() > consumablesForSale[index].GetPrice()) {
            runInfo.ChangeFunds(-consumablesForSale[index].GetPrice());
            runInfo.AddConsumable(consumablesForSale[index]);
            consumableSold[index] = true;
        }
        UpdateShopButtons();
    }
    
    public void BuyTalisman(int index)
    {
        if (runInfo.GetFunds() > talismansForSale[index].GetPrice()) {
            runInfo.ChangeFunds(-talismansForSale[index].GetPrice());
            runInfo.AddTalisman(talismansForSale[index]);
            talismanSold[index] = true;
        }
        UpdateShopButtons();
    }
}
