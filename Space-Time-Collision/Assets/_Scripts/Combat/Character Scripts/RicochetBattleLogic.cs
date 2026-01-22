using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.UI;
using Random = System.Random;

public class RicochetBattleLogic : MonoBehaviour
{
    public enum BulletType
    {
        Normal,
        Critical,
        Incendiary,
        Blank
    }
    
    [SerializeField] private List<BulletType> bulletList;

    private const int CLIP_SIZE = 8;
    private const int BULLET_PREVIEW_MAX = 3;

    private readonly Color32 unknownColor = new Color32(190, 147, 81, 255);
    private readonly Color32 critColor = new Color32(42, 186, 219, 255);
    private readonly Color32 burnColor = new Color32(203, 92, 41, 255);
    private readonly Color32 normalColor = new Color32(196, 196, 196, 255);
    private readonly Color32 misfireColor = new Color32(60, 60, 60, 255);
    
    private Random rng = new Random();
    private BattleSystem battleSystem;
    private PartyManager partyManager;
    private BattleEntity meRicochet;
    private bool isSetup = false;
    private int strafeNormalCount = 0;

    private List<GameObject> bulletObjects;
    private List<BulletPreview> bulletPreviews = new List<BulletPreview>();
    [SerializeField] private List<BulletDisplay> bulletDisplays = new List<BulletDisplay>();

    private BulletDisplay baseBullet;

    private void Start()
    {
        baseBullet = new BulletDisplay {
            bulletColor = unknownColor,
            isRevealed = false,
            hoverText = "UNKNOWN"
        };
    }
    
    public void RicochetBattleSystemLink(BattleSystem battleSystem)
    {
        this.battleSystem = battleSystem;
    }
    
    public void InitializeRicochet(List<PartyMember> partyMembers)
    {
        isSetup = false;
        
        if (partyMembers.Any(t => t.memberName == "Tre")) {
            if (bulletList.Count != 0) {
                bulletList.Clear();
            }
            bulletList = GenerateBulletClip(BulletType.Critical, 2, 1);
            int ricochetPosition = partyMembers.FindIndex(t => t.memberName == "Tre");
            partyMembers[ricochetPosition].maxSpirit = bulletList.Count;
            partyMembers[ricochetPosition].currentSpirit = bulletList.Count;
        }
        
        isSetup = true;
    }

    public void GetRicochetBattleEntity(BattleEntity entity)
    {
        meRicochet = entity;
    }

    public void SetupBulletDisplays(List<GameObject> incomingDisplays)
    {
        bulletObjects = incomingDisplays;
        
        foreach (GameObject incomingBullet in bulletObjects) {
            bulletPreviews.Add(incomingBullet.GetComponent<BulletPreview>());
        }

        List<BulletType> tempList = new List<BulletType>();
        UpdateRevealedBullets(tempList);
    }

    public int CheckBulletCount()
    {
        return bulletList.Count;
    }

    private BulletDisplay CreateUnknownBullet()
    {
        BulletDisplay tempBullet = new BulletDisplay {
            bulletColor = baseBullet.bulletColor,
            isRevealed = baseBullet.isRevealed,
            hoverText = baseBullet.hoverText
        };

        return tempBullet;
    }
    
    private List<BulletType> GenerateBulletClip(BulletType specialBulletType, int specialBulletCount, int blanksCount)
    {
        List<BulletType> tempList = new List<BulletType>();

        int specialBulletsAdded = 0;
        int blanksAdded = 0;
        for (int i = 0; i < CLIP_SIZE; i++) {
            if (specialBulletsAdded < specialBulletCount) {
                tempList.Add(specialBulletType);
                specialBulletsAdded++;
            } else if (blanksAdded < blanksCount) {
                tempList.Add(BulletType.Blank);
                blanksAdded++;
            } else {
                tempList.Add(BulletType.Normal);
            }
        }

        List<BulletType> shuffledList = tempList.OrderBy(t => rng.Next()).ToList();

        if (isSetup) {
            List<BattleEntity> partyMembers = battleSystem.GetPartyList();

            if (partyMembers.Any(t => t.myName == "Tre")) {
                int ricochetPosition = partyMembers.FindIndex(t => t.myName == "Tre");
                partyMembers[ricochetPosition].currentSpirit = bulletList.Count;
            }
        }

        return shuffledList;
    }

    public List<BulletType> CheckCurrentBullets(int bulletsToCheck)
    {
        List<BulletType> tempList = new List<BulletType>();
        if (bulletsToCheck > bulletList.Count) { bulletsToCheck = bulletList.Count; }
        for (int i = 0; i < bulletsToCheck; i++) {
            tempList.Add(bulletList[i]);
        }
        return tempList;
    }
    
    public void ReduceBulletCount(Ability activeAbility)
    {
        if (activeAbility.extraCasts == 0) {
            for (int i = 0; i < activeAbility.costAmount; i++) {
                if (bulletList.Count == 0) {
                    print("Hey, my ammo is bugged. If this appears in the console please pass along the info to programming staff.");
                    return;
                }
                bulletList.RemoveAt(0);
                bulletDisplays.RemoveAt(0);
                BulletDisplay tempBullet = CreateUnknownBullet();
                bulletDisplays.Add(tempBullet);
            }
        } else {
            bulletList.RemoveAt(0);
            BulletDisplay tempBullet = CreateUnknownBullet();
            bulletDisplays.Add(tempBullet);
        }
        
        List<BattleEntity> partyMembers = battleSystem.GetPartyList();
        
        if (partyMembers.Any(t => t.myName == "Tre")) {
            int ricochetPosition = partyMembers.FindIndex(t => t.myName == "Tre");
            partyMembers[ricochetPosition].currentSpirit = bulletList.Count;
            partyMembers[ricochetPosition].UpdatePlayerUI();
        }

        UpdateBulletPreviews();
    }

    private void UpdateRevealedBullets(List<BulletType> bulletsRevealed)
    {
        bulletDisplays.Clear();
        
        for (int i = 0; i < bulletsRevealed.Count; i++) {
            var bullet = bulletsRevealed[i];
            Color32 tempColor;
            string tempString;
            switch (bullet) {
                case BulletType.Normal:
                    tempColor = normalColor;
                    tempString = "Normal Ammo";
                    break;
                case BulletType.Critical:
                    tempColor = critColor;
                    tempString = "Critical Ammo!";
                    break;
                case BulletType.Incendiary:
                    tempColor = burnColor;
                    tempString = "Incendiary Ammo!";
                    break;
                case BulletType.Blank:
                    tempColor = misfireColor;
                    tempString = "Misfire!";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            BulletDisplay tempBullet = new BulletDisplay() {
                bulletColor = tempColor,
                isRevealed = true,
                hoverText = tempString,
            };
            
            bulletDisplays.Add(tempBullet);
        }

        if (bulletsRevealed.Count < BULLET_PREVIEW_MAX) {
            for (int i = bulletsRevealed.Count; i < BULLET_PREVIEW_MAX; i++) {
                BulletDisplay tempBullet = CreateUnknownBullet();
                bulletDisplays.Add(tempBullet);
            }
        }

        UpdateBulletPreviews();
    }

    private void UpdateBulletPreviews()
    {
        for (var i = 0; i < bulletDisplays.Count; i++) {
            bulletPreviews[i].SetMyBulletDisplay(bulletDisplays[i]);
        }
    }

    private void SetBulletText(BattleEntity ricochet, List<BulletType> bulletsUsed)
    {
        string bulletText = null;
        if (bulletsUsed.Count == 1) {
            switch (bulletsUsed[0]) {
                case BulletType.Critical:
                    bulletText = "<color=#2abadbff>CRITICAL</color>";
                    break;
                case BulletType.Incendiary:
                    bulletText = "<color=#cb5c29ff>INCENDIARY</color>";
                    break;
                case BulletType.Normal:
                    bulletText = "<color=#c4c4c4ff>Normal</color>";
                    break;
                case BulletType.Blank:
                    bulletText = "<color=#3c3c3cff>MISFIRE!</color>";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        } else {
            for (var i = 0; i < bulletsUsed.Count; i++) {
                var bulletType = bulletsUsed[i];
                if (i > 0) {
                    bulletText += "\n";
                }
                switch (bulletType) {
                    case BulletType.Critical:
                        bulletText += "<color=#2abadbff>CRITICAL</color>";
                        break;
                    case BulletType.Incendiary:
                        bulletText += "<color=#cb5c29ff>INCENDIARY</color>";
                        break;
                    case BulletType.Normal:
                        bulletText += "<color=#c4c4c4ff>Normal</color>";
                        break;
                    case BulletType.Blank:
                        bulletText += "<color=#3c3c3cff>MISFIRE!</color>";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        ricochet.battleVisuals.SetExtraTextContent(bulletText);
    }

    public void RicochetAttackLogic(BattleEntity ricochet, Ability activeAbility, ref int minDamage, ref int maxDamage, ref int critChance,
        ref int selfXTravel, ref int selfYTravel, ref List<BattleToken> selfTokens, ref List<int> selfTokensCount, 
        ref List<BattleToken> targetTokens, ref List<int> targetTokensCount)
    {
        int bulletCountUsed = activeAbility.costAmount;
        List<BulletType> bulletsUsed = CheckCurrentBullets(bulletCountUsed);

        switch (activeAbility.abilityName) {
            case "Single Shot":
                if (bulletsUsed.Any(t => t == BulletType.Critical)) {
                    // Force the attack to crit
                    critChance = 300;
                } else if (bulletsUsed.Any(t => t == BulletType.Incendiary)) {
                    // Force the attack to apply Burn
                    targetTokens.Add(battleSystem.GetTokenIdentity("Burn"));
                    targetTokensCount.Add(3);
                } else if (bulletsUsed.Any(t => t == BulletType.Blank)) {
                    minDamage = 0;
                    maxDamage = 0;
                }
                break;
            case "Percent Pusher":
                if (ricochet.activeTokens.Any(t => t.tokenName == "BoostPlus")) {
                    BattleToken boostPlus = ricochet.activeTokens.First(t => t.tokenName == "BoostPlus");
                    
                    // Reset Min and Max damage values in order to use new Boost Plus formulas
                    minDamage = activeAbility.dmgMin + ricochet.skill;
                    maxDamage = activeAbility.dmgMax + ricochet.skill;
                    
                    minDamage = Mathf.FloorToInt(minDamage * (1 + (boostPlus.tokenValue * 2)));
                    maxDamage = Mathf.FloorToInt(maxDamage * (1 + (boostPlus.tokenValue * 2)));
                } else if (ricochet.activeTokens.Any(t => t.tokenName == "Boost")) {
                    BattleToken boost = ricochet.activeTokens.First(t => t.tokenName == "Boost");
                    
                    // Reset Min and Max damage values in order to use new Boost formulas
                    minDamage = activeAbility.dmgMin + ricochet.skill;
                    maxDamage = activeAbility.dmgMax + ricochet.skill;
                    
                    minDamage = Mathf.FloorToInt(minDamage * (1 + (boost.tokenValue * 2)));
                    maxDamage = Mathf.FloorToInt(maxDamage * (1 + (boost.tokenValue * 2)));
                }
                
                if (bulletsUsed.Any(t => t == BulletType.Critical)) {
                    // Force the attack to crit
                    critChance = 300;
                } else if (bulletsUsed.Any(t => t == BulletType.Incendiary)) {
                    // Force the attack to apply Burn
                    targetTokens.Add(battleSystem.GetTokenIdentity("Burn"));
                    targetTokensCount.Add(3);
                } else if (bulletsUsed.Any(t => t == BulletType.Blank)) {
                    minDamage = 0;
                    maxDamage = 0;
                }
                break;
            case "Point Blank Shot":
                if (bulletsUsed.Any(t => t == BulletType.Critical)) {
                    // Force the attack to crit
                    critChance = 300;
                } else if (bulletsUsed.Any(t => t == BulletType.Incendiary)) {
                    // Force the attack to apply Burn
                    targetTokens.Add(battleSystem.GetTokenIdentity("Burn"));
                    targetTokensCount.Add(4);
                } else if (bulletsUsed.Any(t => t == BulletType.Blank)) {
                    minDamage = 0;
                    maxDamage = 0;
                }
                break;
            case "Double Tap":
                if (bulletsUsed.Any(t => t == BulletType.Critical)) {
                    // Force the attack to crit
                    critChance = 300;
                } else if (bulletsUsed.Any(t => t == BulletType.Incendiary)) {
                    // Force the attack to apply Burn
                    targetTokens.Add(battleSystem.GetTokenIdentity("Burn"));
                    targetTokensCount.Add(2);
                } else if (bulletsUsed.Any(t => t == BulletType.Blank)) {
                    minDamage = 0;
                    maxDamage = 0;
                }
                break;
            case "Strafe Shooting":
                if (bulletsUsed.Any(t => t == BulletType.Critical)) {
                    // Count crit bullets
                    int critCount = bulletsUsed.Count(t => t == BulletType.Critical);
                    
                    // Force the attack to crit
                    critChance = 300;
                    selfYTravel *= (critCount + 1);
                }
                if (bulletsUsed.Any(t => t == BulletType.Incendiary)) {
                    // Count incendiary bullets
                    int incendiaryCount = bulletsUsed.Count(t => t == BulletType.Incendiary);
                    
                    // Force the attack to apply Burn
                    targetTokens.Add(battleSystem.GetTokenIdentity("Burn"));
                    targetTokensCount.Add(2 * incendiaryCount);
                }
                if (bulletsUsed.Any(t => t == BulletType.Blank)) {
                    // Count misfire bullets
                    int misfireCount = bulletsUsed.Count(t => t == BulletType.Blank);

                    minDamage -= (4 * misfireCount);
                    maxDamage -= (4 * misfireCount);
                }

                if (bulletsUsed.Any(t => t == BulletType.Normal)) {
                    strafeNormalCount = bulletsUsed.Count(t => t == BulletType.Normal);
                    
                }
                break;
            case "Slide Fire":
                if (bulletsUsed.Any(t => t == BulletType.Critical)) {
                    // Count crit bullets
                    int critCount = bulletsUsed.Count(t => t == BulletType.Critical);
                    
                    // Force the attack to crit
                    critChance = 300;
                    selfXTravel *= (critCount + 1);
                }
                if (bulletsUsed.Any(t => t == BulletType.Incendiary)) {
                    // Count incendiary bullets
                    int incendiaryCount = bulletsUsed.Count(t => t == BulletType.Incendiary);
                    
                    // Force the attack to apply Burn
                    targetTokens.Add(battleSystem.GetTokenIdentity("Burn"));
                    targetTokensCount.Add(2 * incendiaryCount);
                }
                if (bulletsUsed.Any(t => t == BulletType.Blank)) {
                    // Count misfire bullets
                    int misfireCount = bulletsUsed.Count(t => t == BulletType.Blank);

                    minDamage -= (4 * misfireCount);
                    maxDamage -= (4 * misfireCount);
                }
                
                selfTokens.Add(battleSystem.GetTokenIdentity("Dodge"));
                selfTokensCount.Add(selfYTravel);
                break;
            case "Computerized Fire":
                if (bulletsUsed.Any(t => t == BulletType.Critical)) {
                    // Count crit bullets
                    int critCount = bulletsUsed.Count(t => t == BulletType.Critical);
                    
                    // Increase damage
                    minDamage += (2 * critCount);
                    maxDamage += (2 * critCount);
                }
                if (bulletsUsed.Any(t => t == BulletType.Incendiary)) {
                    // Count incendiary bullets
                    int incendiaryCount = bulletsUsed.Count(t => t == BulletType.Incendiary);
                    
                    // Force the attack to apply Burn
                    targetTokens.Add(battleSystem.GetTokenIdentity("Burn"));
                    targetTokensCount.Add(incendiaryCount);
                }
                break;
            case "Risky Reload":
                bulletList = GenerateBulletClip(BulletType.Critical, 2, 2);
                bulletsUsed = CheckCurrentBullets(bulletCountUsed);
                meRicochet.currentSpirit = bulletList.Count;
                
                if (bulletsUsed.Any(t => t == BulletType.Critical)) {
                    // Force the attack to crit
                    critChance = 300;
                } else if (bulletsUsed.Any(t => t == BulletType.Blank)) {
                    minDamage = 0;
                    maxDamage = 0;
                    
                    selfTokens.Add(battleSystem.GetTokenIdentity("OffGuard"));
                    selfTokensCount.Add(1);
                    selfTokens.Add(battleSystem.GetTokenIdentity("Slow"));
                    selfTokensCount.Add(1);
                }
                break;
        }

        SetBulletText(ricochet, bulletsUsed);
    }

    public void RicochetHealLogic(BattleEntity ricochet, Ability activeAbility, ref List<BattleToken> selfTokens,
        ref List<int> selfTokensCount)
    {
        List<BulletType> bulletsUsed;
        switch (activeAbility.abilityName) {
            case "Shield Battery":
                bulletsUsed = CheckCurrentBullets(1);
                UpdateRevealedBullets(bulletsUsed); // TODO Reveal the next (bulletsUsed) bullets in the clip
                break;
        }
            
    }

    public void RicochetBuffLogic(BattleEntity ricochet, Ability activeAbility, ref List<BattleToken> selfTokens,
        ref List<int> selfTokensCount)
    {
        List<BulletType> bulletsUsed;
        switch (activeAbility.abilityName) {
            case "Lethal Reload":
                bulletList = GenerateBulletClip(BulletType.Critical, 2, 1);
                meRicochet.currentSpirit = bulletList.Count;
                break;
            case "Incendiary Reload":
                bulletList = GenerateBulletClip(BulletType.Incendiary, 3, 1);
                meRicochet.currentSpirit = bulletList.Count;
                break;
            case "Eject":
                int bulletCountUsed = activeAbility.costAmount;
                bulletsUsed = CheckCurrentBullets(bulletCountUsed);

                if (bulletsUsed.Any(t => t == BulletType.Critical)) {
                    selfTokens.Add(battleSystem.GetTokenIdentity("Blind"));
                    selfTokensCount.Add(1);
                } else if (bulletsUsed.Any(t => t == BulletType.Incendiary)) {
                    selfTokens.Add(battleSystem.GetTokenIdentity("Burn"));
                    selfTokensCount.Add(2);
                } else if (bulletsUsed.Any(t => t == BulletType.Normal)) {
                    selfTokens.Add(battleSystem.GetTokenIdentity("Pierce"));
                    selfTokensCount.Add(1);
                } else if (bulletsUsed.Any(t => t == BulletType.Blank)) {
                    selfTokens.Add(battleSystem.GetTokenIdentity("Boost"));
                    selfTokensCount.Add(2);
                    selfTokens.Add(battleSystem.GetTokenIdentity("Ricochet"));
                    selfTokensCount.Add(1);
                }
                SetBulletText(ricochet, bulletsUsed);
                break;
            case "Calculate":
                bulletsUsed = CheckCurrentBullets(1);
                UpdateRevealedBullets(bulletsUsed);
                break;
        }
    }

    public void RicochetTurnEndLogic(BattleEntity ricochet, Ability activeAbility)
    {
        if (activeAbility.abilityName == "Strafe Shooting") {
            List<BulletType> bulletsUsed = CheckCurrentBullets(strafeNormalCount);
            UpdateRevealedBullets(bulletsUsed);
            strafeNormalCount = 0;
        }
    }
}

[Serializable]
public class BulletDisplay
{
    public Color32 bulletColor;
    public bool isRevealed;
    public String hoverText;
}
