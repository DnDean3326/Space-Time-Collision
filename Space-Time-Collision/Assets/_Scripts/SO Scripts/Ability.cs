using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Ability", menuName = "Scriptable Objects/Ability")]
public class Ability : ScriptableObject
{
    public enum AbilityType
    {
        Damage,
        Heal,
        Buff,
        Debuff,
        Movement,
        Other
    }

    public enum AbilityWeight
    {
        Heavy,
        Medium,
        Light,
        Free,
    }

    public enum AttackType
    {
        Melee,
        Ranged,
        Magic,
        Tech,
        None
    }

    public enum CostResource
    {
        Null,
        Spirit,
        Health,
        Defense,
        SelfDmg,
        Armor,
        Special
    }
    
    public enum KeyStat
    {
        Null,
        Power,
        Skill,
        Wit,
        Mind,
        Speed,
        Luck,
        SelfBuffCount,
        TargetBuffCount,
        SelfDebuffCount,
        TargetDebuffCount
    }

    public enum SecondaryTarget
    {
        Null,
        Bonus,
        Spirit,
        Armor,
        ActionPoints,
    }

    public enum SelfTarget
    {
        Null,
        Health,
        Defense,
        Spirit,
        Armor,
        ActionPoints,
    }

    public enum TokenOption
    {
        Null,
        
        Block,
        BlockPlus,
        Boost,
        BoostPlus,
        Critical,
        Drain,
        Dodge,
        DodgePlus,
        Goad,
        Guard,
        Haste,
        Pierce,
        Precision,
        Quick,
        Ricochet,
        Riposte,
        Rush,
        Stealth,
        Taunt,
        Ward,
        
        AntiHeal,
        Blind,
        Break,
        Delay,
        Goaded,
        Isolation,
        Link,
        OffGuard,
        Restrict,
        Slow,
        Stagger,
        Stun,
        Vulnerable,
        
        Ascension,
        Vice,
        
        Bleed,
        Burn,
        Poison
    }
    
    [Header("Ability Basics")]
    public string abilityName;
    public Sprite abilityIcon;
    public AbilityType abilityType;
    public AbilityWeight abilityWeight;
    
    [Header("Resource Consumption")]
    public CostResource costResource;
    public int costAmount;
    public int cooldown;
    public int rangeMin;
    public int rangeMax;
    public bool hasAccuracy;
    public bool targetSelf;

    [Header("Ability Values")]
    public AttackType attackType;
    public KeyStat keyStat;
    public int statModifier;
    public int dmgMin;
    public int dmgMax;
    public int critChance;
    
    [Header("Secondary")]
    public SecondaryTarget secondaryTarget;
    public KeyStat secondaryStat;
    public int secondaryStatModifier;
    public int secondaryValue;

    [Header("Self-Gain")]
    public SelfTarget selfTarget;
    public KeyStat selfStat;
    public int selfStatModifier;
    public int selfMin;
    public int selfMax;

    [Header("Movement Values")]
    public int[] bannedColumns;
    public int selfXChange;
    public int selfYChange;
    public bool selfYChangeToCenter = false;
    public int targetXChange;
    public int targetYChange;
    public bool targetYChangeToCenter = false;

    [Header("Tokens Interactions")]
    public TokenOption[] selfTokensApplied;
    public int[] selfTokenCountApplied;
    public TokenOption[] targetTokensApplied;
    public int[] targetTokenCountApplied;
    public TokenOption[] selfCritTokensApplied;
    public int[] selfCritTokenCountApplied;
    public TokenOption[] targetCritTokensApplied;
    public int[] targetCritTokenCountApplied;
    public TokenOption[] selfTokensCleared;
    public TokenOption[] targetTokensCleared;
    public bool ignoreBlock;
    public bool ignoreDodge;
    public bool ignoreWard;
    public bool ignoreArmor;
    
    [Header("Description")]
    public string description;
}
