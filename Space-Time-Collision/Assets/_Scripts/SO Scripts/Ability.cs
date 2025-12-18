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
        Power,
        Skill,
        Wit,
        Mind,
        Speed,
        Luck
    }
    
    public string abilityName;
    //public Sprite abilityIcon;
    public AbilityType abilityType;
    public AbilityWeight abilityWeight;

    public CostResource costResource;
    public int costAmount;
    public int cooldown;
    public int range;
    
    public KeyStat keyStat;
    public int statModifier;
    public int dmgMin;
    public int dmgMax;
    public int critChance;

    public int targetTokensApplied;
    public Token targetTokenOne;
    public int targetTokenOneCount;
    public Token targetTokenTwo;
    public int targetTokenTwoCount;
    public Token targetTokenThree;
    public int targetTokenThreeCount;

    public int selfTokensApplied;
    public Token selfTokenOne;
    public int selfTokenOneCount;
    public Token selfTokenTwo;
    public int selfTokenTwoCount;
    public Token selfTokenThree;
    public int selfTokenThreeCount;

    public string description;
}
