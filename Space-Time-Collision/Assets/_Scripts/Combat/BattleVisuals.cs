using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleVisuals : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider spiritBar;
    [SerializeField] private TextMeshProUGUI armorText;

    private int maxHealth;
    private int currentHealth;
    private int maxSpirit;
    private int currentSpirit;
    private int armor;
    private Animator myAnimator;

    private const string IS_ATTACK_PARAM = "IsAttack";
    private const string IS_HIT_PARAM = "IsHit";
    private const string IS_DEAD_PARAM = "IsDead";
    private const string MY_TURN_BOOL = "IsMyTurn";

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
    }

    public void SetStartingValues(int maxHealth, int currentHealth, int maxSpirit, int currentSpirit,  int armor)
    {
        this.maxHealth = maxHealth;
        this.currentHealth = currentHealth;
        this.maxSpirit = maxSpirit;
        this.currentSpirit = currentSpirit;
        this.armor = armor;
        armorText.text = armor.ToString();
        UpdateHealthBar();
        UpdateSpiritBar();
        UpdateArmor();
    }

    public void UpdateHealthBar()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }
    
    public void UpdateSpiritBar()
    {
        spiritBar.maxValue = maxSpirit;
        spiritBar.value = currentSpirit;
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
    
    public void ChangeSpirit(int newSpirit)
    {
        currentSpirit = newSpirit;
        UpdateSpiritBar();
    }

    public void ChangeArmor(int newArmor)
    {
        armor = newArmor;
        UpdateArmor();
    }

    public void PlayAttackAnimation()
    {
        //myAnimator.SetTrigger(IS_ATTACK_PARAM);
        print("");
    }

    public void PlayHitAnimation()
    {
        //myAnimator.SetTrigger(IS_HIT_PARAM);
    }

    public void PlayDeathAnimation()
    {
        //myAnimator.SetTrigger(IS_DEAD_PARAM);
    }

    public void SetMyTurnAnimation(bool myTurn)
    {
        myAnimator.SetBool(MY_TURN_BOOL, myTurn);
    }
    
}
