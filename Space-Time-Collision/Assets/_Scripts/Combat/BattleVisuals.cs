using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleVisuals : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private GameObject myVisuals;
    [SerializeField] private GameObject myAura;
    
    [Header("Value Info")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider defenseBar;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private GameObject targetIndicator;
    [SerializeField] private GameObject[] tokenSlots;

    private int maxHealth;
    private int currentHealth;
    private int maxDefense;
    private int currentDefense;
    private int armor;
    private Animator myAnimator;
    private Animator indicatorAnimator;
    
    private const string IS_ATTACK_PARAM = "AttackTrigger";
    private const string IS_HIT_PARAM = "HitTrigger";
    private const string IS_HEALED_PARAM = "HealedTrigger";
    private const string MISS_PARAM = "MissTrigger";
    private const string IS_DEAD_PARAM = "DeadTrigger";
    private const string MY_TURN_BOOL = "IsMyTurn";
    private const string SHARED_ROW_BOOL = "IsSharingRow";

    private const string TARGET_ENEMY_ACTIVE = "TargetingEnemy";
    private const string TARGET_ALLY_ACTIVE = "TargetingAlly";

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        indicatorAnimator = targetIndicator.GetComponent<Animator>();
    }

    public void SetStartingValues(int maxHealth, int currentHealth, int maxDefense,  int armor)
    {
        this.maxHealth = maxHealth;
        this.currentHealth = currentHealth;
        this.maxDefense = maxDefense;
        this.currentDefense = this.maxDefense;
        this.armor = armor;
        
        armorText.text = armor.ToString();
        UpdateHealthBar();
        UpdateDefenseBar();
        UpdateArmor();
    }

    private void UpdateHealthBar()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    private void UpdateDefenseBar()
    {
        defenseBar.maxValue = maxDefense;
        defenseBar.value = currentDefense;
    }

    private void UpdateArmor()
    {
        armorText.text = armor.ToString();
    }

    public void ChangeHealth(int newHealth)
    {
        currentHealth = newHealth;
        UpdateHealthBar();
        // if health is 0 -> play death animation -> destroy the battle visual
        if (currentHealth <= 0) {
            PlayDeathAnimation();
            Destroy(gameObject, 1f);
        }
    }

    public void ChangeDefense(int newDefense)
    {
        currentDefense = newDefense;
        UpdateDefenseBar();
    }

    public void ChangeArmor(int newArmor)
    {
        armor = newArmor;
        UpdateArmor();
    }

    public void UpdateTokens(List<BattleToken> activeTokens)
    {
        for (int i = 0; i < activeTokens.Count; i++) {
            tokenSlots[i].SetActive(true);
            tokenSlots[i].GetComponent<Image>().sprite = activeTokens[i].tokenIcon;
        }
        for (int j = activeTokens.Count; j < tokenSlots.Length; j++) {
            tokenSlots[j].SetActive(false);
        }
    }

    public void PlayAttackAnimation()
    {
        myAnimator.SetTrigger(IS_ATTACK_PARAM);
    }

    public void PlayHitAnimation(int damageDealt, bool isCrit)
    {
        myAnimator.SetTrigger(IS_HIT_PARAM);
        if (isCrit) {
            damageText.text = damageDealt + " CRIT!";
        } else {
            damageText.text = damageDealt.ToString();
        }
        
    }
    
    public void AbilityMisses()
    {
        myAnimator.SetTrigger(MISS_PARAM);
        damageText.text = "MISS!";
    }
    
    public void PlayHealAnimation(int restoreApplied, bool isCrit)
    {
        myAnimator.SetTrigger(IS_HEALED_PARAM);
        if (isCrit) {
            damageText.text = restoreApplied + "!";
        } else {
            damageText.text = restoreApplied.ToString();
        }
    }

    public void PlayDeathAnimation()
    {
        //myAnimator.SetTrigger(IS_DEAD_PARAM);
    }

    public void SetMyTurnAnimation(bool myTurn)
    {
        myAnimator.SetBool(MY_TURN_BOOL, myTurn);
        if (myTurn) {
            SetMyOrder(6);
        }
    }

    public void SetSharedRowAnimation(bool sharedRow)
    {
        myAnimator.SetBool(SHARED_ROW_BOOL, sharedRow);
    }

    public void TargetEnemyActive()
    {
        targetIndicator.SetActive(true);
        indicatorAnimator.SetBool(TARGET_ENEMY_ACTIVE, true);
    }

    public void TargetAllyActive()
    {
        targetIndicator.SetActive(true);
        indicatorAnimator.SetBool(TARGET_ALLY_ACTIVE, true);
    }

    public void TargetInactive()
    {
        targetIndicator.SetActive(false);
        indicatorAnimator.SetBool(TARGET_ENEMY_ACTIVE, false);
        indicatorAnimator.SetBool(TARGET_ALLY_ACTIVE, false);
    }

    public void SetMyOrder(int newOrder)
    {
        print("SetMyOrder() called with int of " + newOrder);
        print(myVisuals.GetComponent<SpriteRenderer>().sortingOrder);
        myVisuals.GetComponent<SpriteRenderer>().sortingOrder = newOrder;
        print(myVisuals.GetComponent<SpriteRenderer>().sortingOrder);
        myAura.GetComponent<SpriteRenderer>().sortingOrder = (newOrder - 1);
    }
}
