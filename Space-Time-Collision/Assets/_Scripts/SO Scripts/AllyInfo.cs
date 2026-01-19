using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "AllyInfo", menuName = "Scriptable Objects/AllyInfo")]
public class AllyInfo : ScriptableObject
{
    public string allyName;
    public Sprite allyTurnPortrait;
    public Sprite allyCombatSprite;
    public Sprite allySquarePortrait;
    public Sprite allyShadowPortrait;
    public Sprite allySignature;
    
    public int baseHealth;
    public int baseDefense;
    public int baseArmor;
    public int baseSpirit;
    
    public int basePower;
    public int baseSkill;
    public int baseWit;
    public int baseMind;
    public int baseSpeed;
    public int baseLuck;

    public int baseStunResist;
    public int baseDebuffResist;
    public int baseAilmentResist;
    
    public GameObject allyBattleVisualPrefab; // what will be displayed in the battle scene
    public GameObject allyMenuVisualPrefab; // what will be displayed on the map

    public Ability[] abilities;
    public List<Ability> equippedAbilities;
    public Ability stepAbility;
    public Ability sprintAbility;
    
    public Token baseLineBreakToken;
    public int baseLineBreakTokenCount;

    [TextArea(15,20)]
    public string characterDescription;
}