using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObliviwyrmLogic : MonoBehaviour
{
    [SerializeField] private EnemyInfo baseMe;
    private BattleEntity me;

    private const int WYRM_ANNIHILATION_ASC_REQ = 2;

    public void SetMe(BattleEntity entity)
    {
        me = entity;
    }

    public void ObliviwyrmAbilityBlock(List<EnemyAbility> validAbilities)
    {
        if (validAbilities.Any(t => t.ability.abilityName == "Wyrm Annihilation")) {
            int abilityIndex = validAbilities.FindIndex(t => t.ability.abilityName == "Wyrm Annihilation");
            if (me.activeTokens.Any(t => t.tokenName == "Ascension")) {
                int ascCount = me.activeTokens.Count(t => t.tokenName == "Ascension");
                if (ascCount > WYRM_ANNIHILATION_ASC_REQ) {
                    return;
                }
            }
            validAbilities.RemoveAt(abilityIndex);
        }
    }

    public void ObliviwyrmTurnEnd()
    {
        if (me.activeTokens.Any(t => t.tokenName == "Ascension")) {
            int ascCount = me.activeTokens.Count(t => t.tokenName == "Ascension");

            me.power = baseMe.basePower + ascCount;
            me.skill = baseMe.baseSkill + ascCount;
            me.wit =  baseMe.baseWit + ascCount;
            me.mind = baseMe.baseMind + ascCount;
            me.speed = baseMe.baseSpeed + ascCount;
            me.luck = baseMe.baseLuck + ascCount;
        }
    }
}
