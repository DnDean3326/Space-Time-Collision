using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EventNodeSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI eventNameText;
    [SerializeField] private TextMeshProUGUI eventDescriptionText;
    [SerializeField] private GameObject[] eventButtonObjects;
    
    private List<Button> eventButtons = new List<Button>();
    private List<TextMeshProUGUI> eventButtonTexts = new List<TextMeshProUGUI>();
    
    private EventNodeManager eventNodeManager;
    private ItemManager itemManager;
    private PartyManager partyManager;
    private RunInfo runInfo;
    private EventInfo myEvent;
    private bool optionSelected = false;
    
    private const string NODE_SCENE = "NodeScene";

    private void Awake()
    {
        eventNodeManager = FindFirstObjectByType<EventNodeManager>();
        itemManager = FindFirstObjectByType<ItemManager>();
        runInfo = FindFirstObjectByType<RunInfo>();
        foreach (GameObject obj in eventButtonObjects) {
            eventButtons.Add(obj.GetComponent<Button>());
            eventButtonTexts.Add(obj.GetComponentInChildren<TextMeshProUGUI>());
            obj.SetActive(false);
        }
    }

    private void Start()
    {
        eventNodeManager.GenerateNodeEvent();
        myEvent = eventNodeManager.GenerateNodeEvent();
        eventNameText.text = myEvent.eventName;
        eventDescriptionText.text = myEvent.eventDescription;
        for (int i = 0; i < myEvent.eventOptions.Count; i++) {
            eventButtonObjects[i].SetActive(true);
            eventButtonTexts[i].text = myEvent.eventOptions[i];
        }
    }

    public void ProgressEvent(int option)
    {
        if (!optionSelected) {
            ApplyEventEffects(option);
            eventDescriptionText.text = myEvent.eventResults[option];
            for (int i = 1; i < eventButtonObjects.Length; i++) {
                eventButtonObjects[i].SetActive(false);
            }
            eventButtonTexts[0].text = "Continue";
            optionSelected = true;
        } else {
            runInfo.MarkEventCompleted(myEvent);
            SceneManager.LoadScene(NODE_SCENE);
        }
    }

    private void ApplyEventEffects(int option)
    {
        List<PartyMember> partyMembers = partyManager.GetCurrentParty();
        switch (myEvent.eventName) {
            case "Polluted Loot":
                InitialTokenInfo tempToken;
                switch (option) {
                    case 0:
                        List<Talisman> droppedTalisman = itemManager.GetRandomTalisman(1);
                        runInfo.AddTalisman(droppedTalisman[0]);
                        
                        tempToken = new InitialTokenInfo("Poison", 4);
                        foreach (PartyMember member in partyMembers) {
                            member.initialTokens.Add(tempToken);
                        }
                        break;
                    case 1:
                        runInfo.ChangeFunds(5);
                        
                        if (runInfo.GetConsumableCount() < 5) {
                            Consumable droppedConsumable = itemManager.GetRandomConsumable();
                            runInfo.AddConsumable(droppedConsumable);
                        }
                        
                        foreach (PartyMember member in partyMembers) {
                            member.currentHealth -= 5;
                        }
                        break;
                    case 2:
                        PartyMember randomMember = partyMembers[Random.Range(0, partyMembers.Count)];
                        tempToken = new InitialTokenInfo("Dodge", 2);
                        randomMember.initialTokens.Add(tempToken);
                        break;
                }
                break;
            case "Weary Walkers":
                switch (option) {
                    case 0:
                        foreach (PartyMember member in partyMembers) {
                            member.currentHealth += 20;
                            if (member.currentHealth > member.maxHealth) {
                                member.currentHealth = member.maxHealth;
                            }
                            
                            member.currentSpirit += 8;
                            if (member.currentSpirit > member.maxSpirit) {
                                member.currentSpirit = member.maxSpirit;
                            }
                        }
                        
                        runInfo.ChangeEncounterStatus(true);
                        break;
                    case 1:
                        foreach (PartyMember member in partyMembers) {
                            member.currentHealth += 8;
                            if (member.currentHealth > member.maxHealth) {
                                member.currentHealth = member.maxHealth;
                            }
                        }

                        EnemyInitialTokenInfo enemyToken = new EnemyInitialTokenInfo("Block", 1, true);
                        runInfo.AddEnemyInitialToken(enemyToken);
                        break;
                    case 2:
                        tempToken = new InitialTokenInfo("Rush", 1);
                        foreach (PartyMember member in partyMembers) {
                            member.currentHealth -= 3;
                            member.initialTokens.Add(tempToken);
                        }
                        break;
                }
                break;
            case "Template":
                switch (option) {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                }
                break;
        }
        partyManager.UpdatePartyStatus();
    }
}
