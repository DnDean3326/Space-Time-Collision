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
    
    [SerializeField] private List<BulletType> bulletList;
    Random rng = new Random();
    private BattleSystem battleSystem;
    private PartyManager partyManager;

    void Awake()
    {
        battleSystem = FindFirstObjectByType<BattleSystem>();
    }
    
    void Start()
    {
        bulletList = GenerateBulletClip(BulletType.Critical, 2, 1);
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
    
    public int FindBulletsUsed(string abilityName)
    {
        int bulletsUsed = 0;
        switch (abilityName) {
            case "Single Shot":
            case "Sinful Shell": 
                bulletsUsed = 1;
                break;
            case "Double Tap":
                bulletsUsed = 2;
                break;
            case "Strafe Shooting":
            case "Slide Fire":
                bulletsUsed = 3;
                break;
            default:
                break;
        }

        return bulletsUsed;
    }
    
    public void ReduceBulletCount(int bulletReduction)
    {
        for (int i = 0; i < bulletReduction; i++) {
            bulletList.RemoveAt(0);
        }
    }

    public void RicochetAttackLogic(Ability activeAbility, ref int minDamage, ref int maxDamage, ref int critChance,
        ref int selfXTravel, ref int selfYTravel, ref List<BattleToken> selfTokens, ref List<int> selfTokensCount, 
        ref List<BattleToken> targetTokens, ref List<int> targetTokensCount)
    {
        int bulletCountUsed = FindBulletsUsed(activeAbility.abilityName);
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
            case "Sinful Shell":
                if (bulletsUsed.Any(t => t == BulletType.Critical)) {
                    // Force the attack to crit
                    critChance = 300;
                } else if (bulletsUsed.Any(t => t == BulletType.Incendiary)) {
                    // Force the attack to apply Burn
                    targetTokens.Add(battleSystem.GetTokenIdentity("Burn"));
                    targetTokensCount.Add(3);
                }  else if (bulletsUsed.Any(t => t == BulletType.Blank)) {
                    minDamage = 0;
                    maxDamage = 0;
                }
                break;
            case "Strafe Shooting": //TODO review this ability
                if (bulletsUsed.Any(t => t == BulletType.Critical)) {
                    // Count crit bullets
                    int critCount = bulletsUsed.Count(t => t == BulletType.Critical);
                    
                    // Force the attack to crit
                    critChance = 300;
                    selfYTravel *= (critCount + 1);
                } else if (bulletsUsed.Any(t => t == BulletType.Incendiary)) {
                    // Count incendiary bullets
                    int incendiaryCount = bulletsUsed.Count(t => t == BulletType.Critical);
                    
                    // Force the attack to apply Burn
                    targetTokens.Add(battleSystem.GetTokenIdentity("Burn"));
                    targetTokensCount.Add(2 * incendiaryCount);
                }  else if (bulletsUsed.Any(t => t == BulletType.Blank)) {
                    // Count misfire bullets
                    int misfireCount = bulletsUsed.Count(t => t == BulletType.Critical);

                    minDamage *= (1 - (1 / 3 * misfireCount));
                    maxDamage *= (1 - (1 / 3 * misfireCount));
                }
                break;
        }
    }
}
