using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "Scriptable Objects/Ability")]
public class Ability : ScriptableObject
{
    public enum AbilityType
    {
        Damage,
        Heal,
        Buff,
        Debuff,
        Other
    }

    public enum AbilityWeight
    {
        Heavy,
        Light,
        Free
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
        Luck
    }

    public enum SecondaryTarget
    {
        Null,
        Bonus,
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
        Dodge,
        DodgePlus,
        Haste,
        Pierce,
        Precision,
        Swift,
        
        AntiHeal,
        Blind,
        Break,
        Delay,
        OffGuard,
        Slow,
        Vulnerable,
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
    public int range;
    
    [Header("Ability Values")]
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

    [Header("Tokens Applied")]
    public TokenOption[] selfTokensApplied;
    public int[] selfTokenCountApplied;
    public TokenOption[] targetTokensApplied;
    public int[] targetTokenCountApplied;
    public TokenOption[] selfCritTokensApplied;
    public int[] selfCritTokenCountApplied;
    public TokenOption[] targetCritTokensApplied;
    public int[] targetCritTokenCountApplied;
    
    [Header("Description")]
    public string description;
}
