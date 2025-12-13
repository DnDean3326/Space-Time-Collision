using System.Collections.Generic;
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
        }
        
        //DontDestroyOnLoad(gameObject);
    }

    public void AddMemberToPartyByName(string memberName)
    {
        for (int i = 0; i < allMembers.Length; i++) {
            if (allMembers[i].allyName == memberName) {
                PartyMember newPartyMember = new PartyMember();
                
                newPartyMember.memberName = allMembers[i].allyName;
                newPartyMember.level = BASE_LEVEL;
                
                newPartyMember.maxHealth = allMembers[i].baseHealth;
                newPartyMember.currentHealth = newPartyMember.maxHealth;
                newPartyMember.maxDefense = allMembers[i].baseDefense;
                newPartyMember.currentDefense = newPartyMember.maxDefense;
                newPartyMember.maxArmor = allMembers[i].baseArmor;
                newPartyMember.currentArmor = newPartyMember.maxArmor;
                newPartyMember.maxSpirit = allMembers[i].baseSpirit;
                newPartyMember.currentSpirit = newPartyMember.maxDefense;
                
                newPartyMember.power = allMembers[i].basePower;
                newPartyMember.skill = allMembers[i].baseSkill;
                newPartyMember.wit = allMembers[i].baseWit;
                newPartyMember.mind = allMembers[i].baseMind;
                newPartyMember.speed = allMembers[i].baseSpeed;
                newPartyMember.luck = allMembers[i].baseLuck;
                
                newPartyMember.allyBattleVisualPrefab = allMembers[i].allyBattleVisualPrefab;
                newPartyMember.allyMapVisualPrefab = allMembers[i].allyMapVisualPrefab;
                
                currentParty.Add(newPartyMember);
            }
        }
        
    }

    List<PartyMember> GetCurrentParty()
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

    public void SaveHealth(int partyMember, int health)
    {
        currentParty[partyMember].currentHealth = health;
    }
    
}

[System.Serializable]
public class PartyMember
{
    public string memberName;
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
    
}
