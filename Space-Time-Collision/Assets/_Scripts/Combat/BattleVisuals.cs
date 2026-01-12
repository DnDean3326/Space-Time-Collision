using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleVisuals : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private GameObject myVisuals;
    [SerializeField] private GameObject myAura;
    [SerializeField] private GameObject myUI;
    
    [Header("Value Info")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image defenseBar;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private GameObject targetIndicator;
    
    [SerializeField] private GameObject[] tokenSlots;
    [SerializeField] private GameObject[] tokenSlotShadows;
    [SerializeField] private GameObject[] tokenTextSlots;
    
    [SerializeField] private GameObject[] ailmentSlots;
    [SerializeField] private GameObject[] ailmentSlotShadows;
    [SerializeField] private GameObject[] ailmentTextSlots;

    private int maxHealth;
    private int currentHealth;
    private int maxDefense;
    private int currentDefense;
    private int armor;
    
    private Animator myAnimator;
    private Animator indicatorAnimator;
    private SpriteRenderer visualsSprite;
    private SpriteRenderer auraSprite;
    private Canvas uiCanvas;

    private List<Image> ailmentImages;
    private List<TextMeshProUGUI> ailmentCountText;
    
    private List<Image> tokenImages;
    private List<TextMeshProUGUI> tokenCountText;
        
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
        // Components for animation
        myAnimator = GetComponent<Animator>();
        indicatorAnimator = targetIndicator.GetComponent<Animator>();

        // Components for layering
        visualsSprite = myVisuals.GetComponent<SpriteRenderer>();
        auraSprite = myAura.GetComponent<SpriteRenderer>();
        uiCanvas = myUI.GetComponent<Canvas>();
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
        float currentHP = (float)currentHealth;
        float maxHP = (float)maxHealth;
        float hpPercent = (currentHP / maxHP);
        healthBar.fillAmount = hpPercent;
    }

    private void UpdateDefenseBar()
    {
        float currentDP = (float)currentDefense;
        float maxHP = (float)maxDefense;
        float defensePercent = (currentDP / maxHP);
        defenseBar.fillAmount = defensePercent;
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
        int tokenSlotIndex = 0;
        int ailmentSlotIndex = 0;
        
        for (int i = 0; i < activeTokens.Count; i++) {
            if (activeTokens[i].tokenType == Token.TokenType.Ailments) {
                if (activeTokens[i].tokenCount >= 1) {
                    ailmentSlotShadows[ailmentSlotIndex].SetActive(true);
                    ailmentSlots[ailmentSlotIndex].SetActive(true);
                    ailmentSlots[tokenSlotIndex].GetComponent<Image>().sprite = activeTokens[i].tokenIcon;
                    
                    if (activeTokens[i].tokenCount > 1) {
                        ailmentTextSlots[ailmentSlotIndex].SetActive(true);
                        ailmentTextSlots[tokenSlotIndex].GetComponentInChildren<TextMeshProUGUI>().text = "x" + activeTokens[i].tokenCount;
                    } else {
                        ailmentTextSlots[ailmentSlotIndex].SetActive(false);
                    }
                } else {
                    ailmentTextSlots[ailmentSlotIndex].SetActive(false);
                    ailmentSlots[ailmentSlotIndex].SetActive(false);
                    ailmentSlotShadows[ailmentSlotIndex].SetActive(false);
                }
                ailmentSlotIndex++;
            } else {
                if (activeTokens[i].tokenCount >= 1) {
                    tokenSlotShadows[tokenSlotIndex].SetActive(true);
                    tokenSlots[tokenSlotIndex].SetActive(true);
                    tokenSlots[tokenSlotIndex].GetComponent<Image>().sprite = activeTokens[i].tokenIcon;

                    if (activeTokens[i].tokenCount > 1) {
                        tokenTextSlots[tokenSlotIndex].SetActive(true);
                        tokenTextSlots[tokenSlotIndex].GetComponentInChildren<TextMeshProUGUI>().text = "x" + activeTokens[i].tokenCount;
                    } else {
                        ailmentTextSlots[ailmentSlotIndex].SetActive(false);
                    }
                } else {
                    tokenTextSlots[tokenSlotIndex].SetActive(false);
                    tokenSlots[tokenSlotIndex].SetActive(false);
                    tokenSlotShadows[tokenSlotIndex].SetActive(false);
                }
                tokenSlotIndex++;
            }
        }
        for (int j = tokenSlotIndex; j < tokenSlots.Length; j++) {
            tokenSlots[j].SetActive(false);
        }
        for (int j = ailmentSlotIndex; j < ailmentSlots.Length; j++)
        {
            ailmentSlots[j].SetActive(false);
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
            damageText.text = "CRIT!\n" + damageDealt;
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
            damageText.text = "CRIT!\n" + restoreApplied;
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
            SetMyOrder(10);
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
        uiCanvas.sortingOrder = newOrder;
        visualsSprite.sortingOrder = newOrder;
        auraSprite.sortingOrder = (newOrder - 1);
    }
}
