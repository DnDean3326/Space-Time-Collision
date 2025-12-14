using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class BattleEntities
{
    public enum Action
    {
        Attack,
        Defend,
    }

    public Action battleAction;

    public string name;
    public int level;

    public int maxHealth;
    public int currentHealth;
    public int maxSpirit;
    public int currentSpirit;
    public int maxDefense;
    public int currentDefense;
    public int maxArmor;
    public int currentArmor;

    public int power;
    public int skill;
    public int wit;
    public int mind;
    public int speed;
    public int luck;

    public void SetEntityValue(string entityName, int entityMaxHealth, int entityCurrentHealth, int entityMaxSpirit,
        int entityCurrentSpirit, int entityMaxDefense, int entityMaxArmor, int entityPower, int entitySkill, int entityWit, 
        int entityMind, int entitySpeed, int entityLuck)
    {
        
    }
}