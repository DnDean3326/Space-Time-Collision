using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RepentantBattleLogic : MonoBehaviour
{
    private const int MAX_ASCENSION = 2;
    private BattleSystem battleSystem;
    
    public void RepentantBattleSystemLink(BattleSystem battleSystem)
    {
        this.battleSystem = battleSystem;
    }

    public bool RepentantUseLogic(BattleEntity repentant, Ability ability)
    {
        bool blockAbility = false;
        switch (ability.abilityName) {
            case "Seraphic Ray":
                if (repentant.activeTokens.All(t => t.tokenName != "Ascension")) {
                    blockAbility = true;
                }
                break;
            case "Void Stares Back":
                if (repentant.activeTokens.All(t => t.tokenName != "Ascension")) {
                    blockAbility = true;
                }
                break;
            case "Winged Cocoon":
                if (repentant.activeTokens.All(t => t.tokenName != "Ascension")) {
                    blockAbility = true;
                }
                break;
        }

        return blockAbility;
    }
    
    public void RepentantAbilityLogic(BattleEntity repentant, BattleEntity target, Ability activeAbility, ref int minDamage,
        ref int maxDamage, ref int secondaryValue, ref int critChance, ref List<BattleToken> selfTokens, ref List<int> selfTokensCount,
        ref List<BattleToken> targetTokens, ref List<int> targetTokensCount)
    {
        BattleToken selfAscension;
        if (repentant.activeTokens.Any(t => t.tokenName == "Ascension")) {
            selfAscension = repentant.activeTokens.Find(t => t.tokenName == "Ascension");
            switch (activeAbility.abilityName) {
                case "Vide Sabrer":
                    if (selfAscension.tokenCount >= 2) {
                        secondaryValue += 40;
                    }
                    break;
                case "Ranging Thrust":
                    if (selfAscension.tokenCount >= 1) {
                        critChance += 50;
                    }
                    break;
                case "Lightspeed Strike":
                    if (selfAscension.tokenCount >= 2) {
                        selfAscension.tokenCount -= 2;
                        minDamage += 2 + repentant.speed;
                        maxDamage += 2 + repentant.speed;
                        selfTokens.Add(battleSystem.GetTokenIdentity("Haste"));
                        selfTokensCount.Add(2);
                    }
                    break;
                case "Weighted End":
                    if (selfAscension.tokenCount >= 1) {
                        int vulnIndex = selfTokens.FindIndex(t => t.tokenName == "Vulnerable");
                        selfTokensCount[vulnIndex] -= 1;
                        if (selfTokensCount[vulnIndex] <= 0) {
                            selfTokensCount.RemoveAt(vulnIndex);
                            selfTokens.RemoveAt(vulnIndex);
                        }
                    }
                    break;
                case "Trou Noir":
                    //
                    break;
                case "Pistolet":
                    //
                    break;
                case "Temporal Rend":
                    if (selfAscension.tokenCount >= 1) {
                        selfAscension.tokenCount -= 1;
                        minDamage += 3 + repentant.wit;
                        maxDamage += 3 + repentant.wit;
                        secondaryValue += 50;
                    }
                    break;
                case "Ravages of Time":
                    //
                    break;
                case "Explosion D'aile":
                    if (selfAscension.tokenCount >= 1) {
                        selfAscension.tokenCount -= 1;
                        int stunIndex = targetTokens.FindIndex(t => t.tokenName == "Stun");
                        targetTokens.RemoveAt(stunIndex);
                        targetTokens.Add(battleSystem.GetTokenIdentity("Stun"));
                        targetTokensCount.RemoveAt(stunIndex);
                        targetTokensCount.Add(1);
                    }
                    break;
                case "Seraphic Ray":
                    // 
                    break;
                case "Wing Shield": 
                    //
                    break;
                case "Time Heals":
                    if (selfAscension.tokenCount >= 1) {
                        selfAscension.tokenCount -= 1;
                        secondaryValue += 40;
                        minDamage += 5 + repentant.mind;
                        maxDamage += 5 + repentant.mind;
                    }
                    break;
                case "Forgotten By Time":
                    if (selfAscension.tokenCount >= 1) {
                        selfAscension.tokenCount -= 1;
                        selfTokens.Add(battleSystem.GetTokenIdentity("Critical"));
                        selfTokensCount.Add(1);
                    }
                    break;
                case "Nihilistic Rise": 
                    //
                    break;
                case "Guilty Guard": 
                    //
                    break;
                case "Backflip": 
                    //
                    break;
                case "Nihil":
                    if (target.activeTokens.Any(t => t.tokenName == "Ascension")) {
                        int targetAscendIndex = target.activeTokens.FindIndex(t => t.tokenName == "Ascension");
                        targetTokens[targetAscendIndex].tokenCount -= 1;
                        if (targetTokens[targetAscendIndex].tokenCount <= 0) {
                            targetTokens.RemoveAt(targetAscendIndex);
                        }

                        if (selfTokens.Any(t => t.tokenName == "Ascension")) {
                            int ascendIndex = selfTokens.FindIndex(t => t.tokenName == "Ascension");
                            selfTokensCount[ascendIndex] += 1;
                        } else {
                            selfTokens.Add(battleSystem.GetTokenIdentity("Ascension"));
                            selfTokensCount.Add(1);
                        }
                    }
                    break;
                case "Void Stares Back": 
                    //
                    break;
            }
        }
        AscensionMax(repentant);
    }

    public void RepentantLethalLogic(BattleEntity repentant, Ability activeAbility, ref List<BattleToken> selfTokens, 
        ref List<int> selfTokensCount)
    {
        switch (activeAbility.abilityName) {
            case "Trou Noir":
                if (repentant.activeTokens.Any(t => t.tokenName == "Ascension")) {
                    int ascendIndex = selfTokens.FindIndex(t => t.tokenName == "Ascension");
                    selfTokensCount[ascendIndex] += 1;
                } else {
                    selfTokens.Add(battleSystem.GetTokenIdentity("Ascension"));
                    selfTokensCount.Add(1);
                }
                break;
        }
    }

    public void AscensionMax(BattleEntity repentant)
    {
        if (repentant.activeTokens.Any(t => t.tokenName == "Ascension")) {
            int ascensionIndex = repentant.activeTokens.FindIndex(t => t.tokenName == "Ascension");
            if (repentant.activeTokens[ascensionIndex].tokenCount > MAX_ASCENSION) {
                repentant.activeTokens[ascensionIndex].tokenCount = MAX_ASCENSION;
            }
        }
    }
}
