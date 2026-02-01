using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class TalismanLogic : MonoBehaviour
{
    private List<Talisman> talismanList;
    private RunInfo runInfo;
    private BattleSystem battleSystem;
    
    private void Awake()
    {
        battleSystem = FindFirstObjectByType<BattleSystem>();
        runInfo = FindFirstObjectByType<RunInfo>();
        talismanList = runInfo.GetTalismans();
    }

    public void BattleStartTalisman()
    {
        if (talismanList.Any(t => t.GetName() == "Spirit Shield")) {
            List<BattleEntity> party = battleSystem.GetPartyList();
            foreach (BattleEntity member in party) {
                int spReduction = Mathf.RoundToInt(member.maxSpirit * 0.1f);
                member.currentSpirit -= spReduction;
                if (member.currentSpirit < 0) {
                    member.currentSpirit = 0;
                }
                battleSystem.AddTokens(member, member, "Block", 2, 0);
            }
        }
    }

    public void CritTalisman(ref List<BattleToken> selfTokens, ref List<int> selfTokensCount)
    {
        if (talismanList.Any(t => t.GetName() == "Spiky Snowball")) {
            selfTokens.Add(battleSystem.GetTokenIdentity("Boost"));
            selfTokensCount.Add(1);
        }
    }

    public void AttackTalisman(BattleEntity user, Ability activeAbility, int distance, ref List<BattleToken> targetTokens, ref List<int> targetTokensCount)
    {
        if (talismanList.Any(t => t.GetName() == "Sanguine Stone") && activeAbility.attackType == Ability.AttackType.Melee) {
            targetTokens.Add(battleSystem.GetTokenIdentity("Bleed"));
            targetTokensCount.Add(2);
        }
        if (talismanList.Any(t => t.GetName() == "Corrosive Payload") && activeAbility.attackType == Ability.AttackType.Ranged && 
            distance >= 4) {
            targetTokens.Add(battleSystem.GetTokenIdentity("Poison"));
            targetTokensCount.Add(2);
        }
        if (talismanList.Any(t => t.GetName() == "Hand-Painted Ember") && activeAbility.attackType == Ability.AttackType.Magic) {
            int baseSpCost = 0;
            if (activeAbility.costResource == Ability.CostResource.Spirit) {
                baseSpCost += activeAbility.costAmount;
            }
            if (user.currentSpirit >= baseSpCost + 1) {
                user.currentSpirit -= 1;
                targetTokens.Add(battleSystem.GetTokenIdentity("Burn"));
                targetTokensCount.Add(2);
            }
        }
    }

    public void DebuffTalisman(BattleEntity user, Ability activeAbility, ref List<BattleToken> targetTokens, ref List<int> targetTokensCount)
    {
        if (talismanList.Any(t => t.GetName() == "Hand-Painted Ember") && activeAbility.attackType == Ability.AttackType.Magic) {
            int baseSpCost = 0;
            if (activeAbility.costResource == Ability.CostResource.Spirit) {
                baseSpCost += activeAbility.costAmount;
            }
            if (user.currentSpirit >= baseSpCost + 1) {
                user.currentSpirit -= 1;
                targetTokens.Add(battleSystem.GetTokenIdentity("Burn"));
                targetTokensCount.Add(2);
            }
        }
    }

    public void DefenseTalisman(BattleEntity target, ref int critChance)
    {
        if (talismanList.Any(t => t.GetName() == "Adamantine Ore")) {
            int critReduction = target.currentArmor * 3;
            critChance -= critReduction;
            if (critChance < 0) {
                critChance = 0;
            }
        }
    }
}
