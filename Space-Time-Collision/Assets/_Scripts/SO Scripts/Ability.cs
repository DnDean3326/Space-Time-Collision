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

    public string description;
}
