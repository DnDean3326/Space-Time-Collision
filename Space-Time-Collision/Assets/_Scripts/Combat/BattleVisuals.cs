using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class BattleVisuals : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private GameObject myVisuals;
    [SerializeField] private GameObject myAura;
    [SerializeField] private GameObject myUI;
    
    [Header("Value Info")]
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject healthText;
    [SerializeField] private Image defenseBar;
    [SerializeField] private GameObject defenseText;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI extraText;
    [SerializeField] private GameObject targetIndicator;

    [SerializeField] private GameObject tokenGrid;
    [SerializeField] private GameObject ailmentGrid;
    [SerializeField] private GameObject tokenPrefab;
    [SerializeField] private GameObject ailmentPrefab;

    private RectTransform tokenDisplayRect;
    private RectTransform ailmentDisplayRect;

    private BattleEntity me;
    //private int maxHealth;
    //private int currentHealth;
    //private int maxDefense;
    //private int currentDefense;
    //private int armor;
    private bool wasSharingRow;
    
    private Animator myAnimator;
    private Animator indicatorAnimator;
    private SpriteRenderer visualsSprite;
    private SpriteRenderer auraSprite;
    private Canvas uiCanvas;
    private TextMeshProUGUI healthTMP;
    private TextMeshProUGUI defenseTMP;

    private List<Image> ailmentImages = new List<Image>();
    private List<TextMeshProUGUI> ailmentCountText = new List<TextMeshProUGUI>();
    private List<BattleToken> myAilments = new List<BattleToken>();
    private List<Image> tokenImages = new List<Image>();
    private List<TextMeshProUGUI> tokenCountText = new List<TextMeshProUGUI>();
    private List<BattleToken> myTokens = new List<BattleToken>();
        
    private const string IS_ATTACK_PARAM = "AttackTrigger";
    private const string IS_HIT_PARAM = "HitTrigger";
    private const string IS_HEALED_PARAM = "HealedTrigger";
    private const string MISS_PARAM = "MissTrigger";
    private const string IS_DEAD_PARAM = "DeadTrigger";
    private const string MY_TURN_BOOL = "IsMyTurn";
    private const string SHARED_ROW_BOOL = "IsSharingRow";
    private const string ACT_OUT = "ActOut";

    private const string TARGET_ENEMY_ACTIVE = "TargetingEnemy";
    private const string TARGET_ALLY_ACTIVE = "TargetingAlly";

    private const float ROW_MAX = 7f;
    private const float TOKEN_HEIGHT = 34f;
    private const float TOKEN_WIDTH = 24f;

    private void Awake()
    {
        // Components for animation
        myAnimator = GetComponent<Animator>();
        indicatorAnimator = targetIndicator.GetComponent<Animator>();

        // Components for layering
        visualsSprite = myVisuals.GetComponent<SpriteRenderer>();
        auraSprite = myAura.GetComponent<SpriteRenderer>();
        uiCanvas = myUI.GetComponent<Canvas>();
        
        // Components for HP/Defense text display
        healthTMP = healthText.GetComponent<TextMeshProUGUI>();
        defenseTMP = defenseText.GetComponent<TextMeshProUGUI>();
        
        // Components for Tokens
        tokenDisplayRect = tokenGrid.GetComponent<RectTransform>();
        ailmentDisplayRect = ailmentGrid.GetComponent<RectTransform>();
        
    }

    private void Start()
    {
        HideHealth();
    }

    public void DisableUIBar()
    {
        myUI.SetActive(false);
    }

    public void SetStartingValues(BattleEntity entity)
    {
        me = entity;

        healthTMP.text = me.currentHealth + " / " + me.maxHealth;
        defenseTMP.text = me.currentDefense + " / " + me.maxDefense;
        
        armorText.text = me.currentArmor.ToString();
        UpdateHealthBar();
        UpdateDefenseBar();
        UpdateArmor();
    }

    public void UpdateHealthBar()
    {
        float hpPercent = ((float)me.currentHealth / me.maxHealth);
        
        healthTMP.text = me.currentHealth + " / " + me.maxHealth;
        
        healthBar.fillAmount = hpPercent;
        
        if (me.currentHealth <= 0) {
            PlayDeathAnimation();
            Destroy(gameObject, 1f);
        }
    }

    public void UpdateDefenseBar()
    {
        float defensePercent = ((float)me.currentDefense / me.maxDefense);
        
        defenseTMP.text = me.currentDefense + " / " + me.maxDefense;
        
        defenseBar.fillAmount = defensePercent;
    }

    public void UpdateArmor()
    {
        armorText.text = me.currentArmor.ToString();
    }

    public void PreviewHealth(int reduction)
    {
        float previewValue = me.currentHealth - reduction;
        UpdateDefenseBar();
        
        float healthPercent = (previewValue / me.maxHealth);
        
        healthTMP.text = previewValue + " / " + me.maxHealth;
        
        healthBar.fillAmount = healthPercent;
    }

    public void PreviewDefense(int reduction)
    {
        float previewValue = me.currentDefense - reduction;
        UpdateDefenseBar();
        
        float defensePercent = (previewValue / me.maxDefense);
        
        defenseTMP.text = previewValue + " / " + me.maxDefense;
        
        defenseBar.fillAmount = defensePercent;
    }

    public void PreviewArmor(int reduction)
    {
        float previewValue = me.currentArmor - reduction;
        armorText.text = previewValue.ToString();
    }

    public void UpdateTokens(List<BattleToken> activeTokens)
    {
        int tokenSlotCount = 0;
        int ailmentSlotCount = 0;
        
        foreach (Transform child in tokenGrid.transform) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in ailmentGrid.transform) {
            Destroy(child.gameObject);
        }
        
        foreach (var token in activeTokens) {
            if (token.tokenType != Token.TokenType.Ailments) {
                GameObject tempObject = Instantiate(tokenPrefab, tokenGrid.transform);
                TokenDisplay tempDisplay = tempObject.GetComponent<TokenDisplay>();
                tempDisplay.SetMyToken(token);
                tempDisplay.DisplayToken();
                tokenSlotCount++;
            } else if (token.tokenType == Token.TokenType.Ailments) {
                GameObject tempObject = Instantiate(ailmentPrefab, ailmentGrid.transform);
                TokenDisplay tempDisplay = tempObject.GetComponent<TokenDisplay>();
                tempDisplay.SetMyToken(token);
                tempDisplay.DisplayToken();
                ailmentSlotCount++;
            }
        }

        float tempWidth = 0;
        switch (tokenSlotCount) {
            case 0:
                break;
            case 1:
                tempWidth = TOKEN_WIDTH * 1;
                break;
            case 2:
                tempWidth = TOKEN_WIDTH * 2;
                break;
            case 3:
                tempWidth = TOKEN_WIDTH * 3;
                break;
            case 4:
                tempWidth = TOKEN_WIDTH * 4;
                break;
            default:
                tempWidth = TOKEN_WIDTH * 5;
                break;
        }
        
        float width = tempWidth;
        float rowCount = Mathf.Ceil(tokenSlotCount / ROW_MAX);
        float height = rowCount * TOKEN_HEIGHT;
        
        tokenDisplayRect.sizeDelta = new Vector2(width, height);

        tempWidth = 0;
        switch (ailmentSlotCount) {
            case 0:
                break;
            case 1:
                tempWidth = TOKEN_WIDTH * 1;
                break;
            case 2:
                tempWidth = TOKEN_WIDTH * 2;
                break;
            case 3:
                tempWidth = TOKEN_WIDTH * 3;
                break;
            case 4:
                tempWidth = TOKEN_WIDTH * 4;
                break;
            default:
                tempWidth = TOKEN_WIDTH * 5;
                break;
        }

        width = tempWidth;
        rowCount = Mathf.Ceil(tokenSlotCount / ROW_MAX);
        height = rowCount * TOKEN_HEIGHT;
        
        ailmentDisplayRect.sizeDelta = new Vector2(width, height);
    }

    public void SetExtraTextContent(string text)
    {
        extraText.text = text;
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

    public void PlayActOutAnimation()
    {
        myAnimator.SetTrigger(ACT_OUT);
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
        if (uiCanvas is not null) {
            uiCanvas.sortingOrder = newOrder;
        } else {
            return;
        }
        visualsSprite.sortingOrder = newOrder;
        auraSprite.sortingOrder = (newOrder - 1);
    }
    
    // OnPointerEnter Methods

    public void DisplayHealth()
    {
        wasSharingRow = myAnimator.GetBool(SHARED_ROW_BOOL);
        healthText.SetActive(true);
        defenseText.SetActive(true);
        if (wasSharingRow) {
            myAnimator.SetBool(SHARED_ROW_BOOL, false);
        }
    }
    
    // OnPointerExit Methods

    public void HideHealth()
    {
        healthText.SetActive(false);
        defenseText.SetActive(false);
        if (wasSharingRow) {
            myAnimator.SetBool(SHARED_ROW_BOOL, true);
        }
    }
}
