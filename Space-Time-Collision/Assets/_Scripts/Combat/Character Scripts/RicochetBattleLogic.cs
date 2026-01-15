using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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

    private const int CLIP_SIZE = 8;
    private bool isSetup = false;
    
    [SerializeField] private List<BulletType> bulletList;
    Random rng = new Random();
    private BattleSystem battleSystem;
    private PartyManager partyManager;
    
    public void RicochetBattleSystemLink(BattleSystem battleSystem)
    {
        this.battleSystem = battleSystem;
    }
    
    public void InitializeRicochet (List<PartyMember> partyMembers)
    {
        if (partyMembers.Any(t => t.memberName == "Tre")) {
            bulletList = GenerateBulletClip(BulletType.Critical, 2, 1);
            int ricochetPosition = partyMembers.FindIndex(t => t.memberName == "Tre");
            partyMembers[ricochetPosition].maxSpirit = bulletList.Count;
            partyMembers[ricochetPosition].currentSpirit = bulletList.Count;
        }
        
        isSetup = true;
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
            List<BattleEntities> partyMembers = battleSystem.GetPartyList();

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
        for (int i = 0; i < bulletsToCheck; i++) {
            tempList.Add(bulletList[i]);
        }

        return tempList;
    }
    
    public void ReduceBulletCount(Ability activeAbility)
    {
        if (activeAbility.extraCasts == 0) {
            for (int i = 0; i < activeAbility.costAmount; i++) {
                bulletList.RemoveAt(0);
            }
        } else {
            bulletList.RemoveAt(0);
        }
        
        List<BattleEntities> partyMembers = battleSystem.GetPartyList();
        
        if (partyMembers.Any(t => t.myName == "Tre")) {
            int ricochetPosition = partyMembers.FindIndex(t => t.myName == "Tre");
            partyMembers[ricochetPosition].currentSpirit = bulletList.Count;
            partyMembers[ricochetPosition].UpdatePlayerUI();
        }
    }

    public void RicochetAttackLogic(BattleEntities ricochet, Ability activeAbility, ref int minDamage, ref int maxDamage, ref int critChance,
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
                    int normalCount = bulletsUsed.Count(t => t == BulletType.Normal);
                    bulletsUsed = CheckCurrentBullets(normalCount);
                    // TODO Reveal the next (bulletsUsed) bullets in the clip
                    foreach (var bullet in bulletsUsed) {
                        print(bullet.ToString());
                    }
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
                GenerateBulletClip(BulletType.Critical, 2, 2);
                bulletsUsed = CheckCurrentBullets(bulletCountUsed);
                
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
    }

    public void RicochetBuffLogic(BattleEntities ricochet, Ability activeAbility, ref List<BattleToken> selfTokens,
        ref List<int> selfTokensCount)
    {
        List<BulletType> bulletsUsed;
        switch (activeAbility.abilityName) {
            case "Lethal Reload":
                GenerateBulletClip(BulletType.Critical, 2, 1);
                break;
            case "Incendiary Reload":
                GenerateBulletClip(BulletType.Incendiary, 3, 1);
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
                break;
            case "Calculate":
                bulletsUsed = CheckCurrentBullets(1);
                // TODO Reveal the next (bulletsUsed) bullets in the clip
                foreach (var bullet in bulletsUsed) {
                    print(bullet.ToString());
                }
                break;
        }
    }
}
