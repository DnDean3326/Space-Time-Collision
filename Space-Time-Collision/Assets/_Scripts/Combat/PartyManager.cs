using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private AllyInfo[] allMembers;
    [SerializeField] private List<PartyMember> currentParty;
    
    [SerializeField] private AllyInfo[] defaultPartyMembers;

    private static GameObject _instance;

    private const int BASE_LEVEL = 1;

    private RicochetBattleLogic ricochetLogic;

    private void Awake()
    {
        ricochetLogic = FindFirstObjectByType<RicochetBattleLogic>();
        
        if (_instance != null) {
            Destroy(gameObject);
        } else {
            _instance = gameObject;
            AddMemberToPartyByName(defaultPartyMembers[0].allyName, BASE_LEVEL);
        }
        
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ricochetLogic.InitializeRicochet(currentParty);
    }

    public void AddMemberToPartyByName(string memberName, int level)
    {
        for (int i = 0; i < allMembers.Length; i++) {
            if (allMembers[i].allyName == memberName) {
                PartyMember newPartyMember = new PartyMember();
                
                newPartyMember.memberName = allMembers[i].allyName;
                newPartyMember.memberPortrait = allMembers[i].allyTurnPortrait;
                newPartyMember.memberSquarePortrait = allMembers[i].allySquarePortrait;
                newPartyMember.level = level;

                // TODO let players set their party formation
                newPartyMember.xPos = 4 - currentParty.Count;
                newPartyMember.yPos = 4 - currentParty.Count;
                
                newPartyMember.maxHealth = allMembers[i].baseHealth;
                newPartyMember.currentHealth = newPartyMember.maxHealth;
                newPartyMember.maxDefense = allMembers[i].baseDefense;
                newPartyMember.currentDefense = newPartyMember.maxDefense;
                newPartyMember.maxArmor = allMembers[i].baseArmor;
                newPartyMember.currentArmor = newPartyMember.maxArmor;
                newPartyMember.maxSpirit = allMembers[i].baseSpirit;
                newPartyMember.currentSpirit = newPartyMember.maxSpirit;
                
                newPartyMember.power = allMembers[i].basePower;
                newPartyMember.skill = allMembers[i].baseSkill;
                newPartyMember.wit = allMembers[i].baseWit;
                newPartyMember.mind = allMembers[i].baseMind;
                newPartyMember.speed = allMembers[i].baseSpeed;
                newPartyMember.luck = allMembers[i].baseLuck;
                
                newPartyMember.stunResist = allMembers[i].baseStunResist;
                newPartyMember.debuffResist = allMembers[i].baseDebuffResist;
                newPartyMember.ailmentResist = allMembers[i].baseAilmentResist;
                
                newPartyMember.allyBattleVisualPrefab = allMembers[i].allyBattleVisualPrefab;
                newPartyMember.allyMenuVisualPrefab = allMembers[i].allyMenuVisualPrefab;

                // TODO Remove this once ability selection is implemented
                newPartyMember.activeAbilities = new List<Ability> { allMembers[i].equippedAbilities[0],  allMembers[i].equippedAbilities[1], 
                    allMembers[i].equippedAbilities[2],  allMembers[i].equippedAbilities[3], allMembers[i].equippedAbilities[4], 
                    allMembers[i].stepAbility, allMembers[i].sprintAbility };
                
                newPartyMember.lineBreakToken = allMembers[i].baseLineBreakToken;
                newPartyMember.lineBreakTokenCount = allMembers[i].baseLineBreakTokenCount;
                
                currentParty.Add(newPartyMember);
            }
        }
    }

    public List<AllyInfo> GetAllAllies()
    {
        List<AllyInfo> allyList = new List<AllyInfo>();
        foreach (AllyInfo ally in allMembers) {
            allyList.Add(ally);
        }
        return allyList;
    }
    
    public List<PartyMember> GetCurrentParty()
    {
        List<PartyMember> aliveParty = new List<PartyMember>();
        aliveParty = currentParty;
        for (int i = 0; i < aliveParty.Count; i++) {
            if (aliveParty[i].currentHealth <= 0) {
                aliveParty.RemoveAt(i);
            }
        }
        return aliveParty;
        
    }

    public List<Ability> GetActiveAbilities(int partyIndex)
    {
        List<Ability> activeAbilities = currentParty[partyIndex].activeAbilities;
        return activeAbilities;
    }

    public void SaveHealth(int partyMember, int health)
    {
        currentParty[partyMember].currentHealth = health;
    }

    public void SaveSpirit(int partyMember, int spirit)
    {
        currentParty[partyMember].currentSpirit = spirit;
    }
    
}

[System.Serializable]
public class PartyMember
{
    public string memberName;
    public Sprite memberPortrait;
    public Sprite memberSquarePortrait;
    public int level;

    public int xPos;
    public int yPos;
    
    public int maxHealth;
    public int currentHealth;
    public int maxDefense;
    public int currentDefense;
    public int maxArmor;
    public int currentArmor;
    public int maxSpirit;
    public int currentSpirit;
    
    public int power;
    public int skill;
    public int wit;
    public int mind;
    public int speed;
    public int luck;
    
    public int stunResist;
    public int debuffResist;
    public int ailmentResist;

    public int maxExp;
    public int currentExp;
    
    public GameObject allyBattleVisualPrefab; // what will be displayed in the battle scene
    public GameObject allyMenuVisualPrefab; // what will be displayed on the map

    public List<Ability> activeAbilities;
    
    public Token lineBreakToken;
    public int lineBreakTokenCount;
}
