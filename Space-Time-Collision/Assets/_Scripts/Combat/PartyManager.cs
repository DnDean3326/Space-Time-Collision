using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private AllyInfo[] allMembers;
    [SerializeField] private List<PartyMember> currentParty;
    
    [SerializeField] private AllyInfo defaultPartyMember;

    private static GameObject _instance;

    private const int BASE_LEVEL = 1;

    private void Awake()
    {
        if (_instance != null) {
            Destroy(gameObject);
        } else {
            _instance = gameObject;
            AddMemberToPartyByName(defaultPartyMember.allyName, BASE_LEVEL);
            AddMemberToPartyByName(defaultPartyMember.allyName, BASE_LEVEL);
            //AddMemberToPartyByName(defaultPartyMember.allyName, BASE_LEVEL);
            //AddMemberToPartyByName(defaultPartyMember.allyName, BASE_LEVEL);
        }
        
        DontDestroyOnLoad(gameObject);
    }

    private void AddMemberToPartyByName(string memberName, int level)
    {
        for (int i = 0; i < allMembers.Length; i++) {
            if (allMembers[i].allyName == memberName) {
                PartyMember newPartyMember = new PartyMember();
                
                newPartyMember.memberName = allMembers[i].allyName;
                newPartyMember.memberPortait = allMembers[i].allyPortrait;
                newPartyMember.level = level;
                
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
                
                newPartyMember.allyBattleVisualPrefab = allMembers[i].allyBattleVisualPrefab;
                newPartyMember.allyMapVisualPrefab = allMembers[i].allyMapVisualPrefab;

                // TODO Remove this once ability selection is implements
                newPartyMember.activeAbilities = new List<Ability> { allMembers[i].abilityOne,  allMembers[i].abilityTwo, allMembers[i].abilityThree, allMembers[i].abilityFour };
                
                currentParty.Add(newPartyMember);
            }
        }
    }
    
    // TODO Build a function to allow the election of abilities from a list

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
    public Sprite memberPortait;
    public int level;
    
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

    public int maxExp;
    public int currentExp;
    
    public GameObject allyBattleVisualPrefab; // what will be displayed in the battle scene
    public GameObject allyMapVisualPrefab; // what will be displayed on the map

    public List<Ability> activeAbilities;

}
