using System.Linq;
using UnityEngine;

public class ProphetBattleLogic : MonoBehaviour
{
    private BattleSystem battleSystem;
    
    public void ProphetBattleSystemLink(BattleSystem bSystem)
    {
        battleSystem = bSystem;
    }

    public bool ProphetUseLogic(BattleEntity prophet, Ability ability)
    {
        bool blockAbility = false;
        switch (ability.abilityName) {
            case "Restorative Burst":
                int buffCount = 0;
                foreach (BattleToken token in prophet.activeTokens) {
                    if (token.tokenType == Token.TokenType.Buff) {
                        buffCount += token.tokenCount;
                    }
                }

                if (buffCount < 3) {
                    blockAbility = true;
                }
                break;
            default:
                break;
        }
        
        return blockAbility;
    }
    
    public void ProphetHealLogic(Ability activeAbility, ref bool overheal, ref int secondaryRestore)
    {
        switch (activeAbility.abilityName) {
            case "Restorative Burst":
                overheal = true;
                break;
            case "Reconvene With Madness":
                foreach (BattleEntity entity in battleSystem.GetCombatantsList()) {
                    if (entity.activeTokens.Any(t => t.tokenName == "ConfidenceProphecy")) {
                        BattleToken confidenceProphecy = entity.activeTokens.Find(t => t.tokenName == "ConfidenceProphecy");
                        entity.activeTokens.Remove(confidenceProphecy);
                        secondaryRestore += 2;
                    }
                    if (entity.activeTokens.Any(t => t.tokenName == "FriendshipProphecy")) {
                        BattleToken friendshipProphecy = entity.activeTokens.Find(t => t.tokenName == "FriendshipProphecy");
                        entity.activeTokens.Remove(friendshipProphecy);
                        secondaryRestore += 2;
                    }
                    if (entity.activeTokens.Any(t => t.tokenName == "DoomProphecy")) {
                        BattleToken doomProphecy = entity.activeTokens.Find(t => t.tokenName == "DoomProphecy");
                        entity.activeTokens.Remove(doomProphecy);
                        secondaryRestore += 2;
                    }
                }
                break;
        }
    }
}
