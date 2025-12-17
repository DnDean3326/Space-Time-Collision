using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleVisuals : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider defenseBar;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private GameObject targetIndicator;

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
    private const string IS_DEAD_PARAM = "DeadTrigger";
    private const string MY_TURN_BOOL = "IsMyTurn";

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

    public void UpdateHealthBar()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }
    
    public void UpdateDefenseBar()
    {
        defenseBar.maxValue = maxDefense;
        defenseBar.value = currentDefense;
    }

    public void UpdateArmor()
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

    public void PlayAttackAnimation()
    {
        myAnimator.SetTrigger(IS_ATTACK_PARAM);
    }

    public void PlayHitAnimation()
    {
        myAnimator.SetTrigger(IS_HIT_PARAM);
    }
    
    public void PlayHealAnimation()
    {
        myAnimator.SetTrigger(IS_HEALED_PARAM);
    }

    public void PlayDeathAnimation()
    {
        //myAnimator.SetTrigger(IS_DEAD_PARAM);
    }

    public void SetMyTurnAnimation(bool myTurn)
    {
        myAnimator.SetBool(MY_TURN_BOOL, myTurn);
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
}
