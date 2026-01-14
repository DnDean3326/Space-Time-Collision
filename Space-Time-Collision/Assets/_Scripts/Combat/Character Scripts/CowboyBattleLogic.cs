using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

public class CowboyBattleLogic : MonoBehaviour
{
    private const float COWBOY_BASE_ACTOUT = 10f;
    private const float COWBOY_MAX_ACTOUT = 70f;
    private const float COWBOY_ACTOUT_INCREASE = 30f;
    
    private bool gainedVice;
    private float currentActoutChance;
    
    private BattleSystem battleSystem;
    
    private void Awake()
    {
        battleSystem = FindFirstObjectByType<BattleSystem>();
    }

    private void Start()
    {
        currentActoutChance = COWBOY_BASE_ACTOUT;
    }
    
    public void GainedVice(bool didGainVice)
    {
        gainedVice = didGainVice;
    }

    public void ResetCowboyActout()
    {
        currentActoutChance = COWBOY_BASE_ACTOUT;
    }
    
    public void CowboyGainVice(BattleEntities bune, BattleEntities attackTarget)
    {if (attackTarget.activeTokens.Any(t => t.tokenName == "AntiHeal") ||
            attackTarget.activeTokens.Any(t => t.tokenName == "Isolation") ||
            attackTarget.activeTokens.Any(t => t.tokenName == "OffGuard") ||
            attackTarget.activeTokens.Any(t => t.tokenName == "Stagger") ||
            attackTarget.activeTokens.Any(t => t.tokenName == "Stun") ||
            attackTarget.activeTokens.Any(t => t.tokenName == "Vulnerable")) {
            battleSystem.AddTokens(bune, bune, "Vice", 1, 0);
        }
    }
    
    public void CowboyRemoveVice(BattleEntities bune)
    {
        if (bune.activeTokens.Any(t => t.tokenName == "Vice")) {
            if (!bune.wasDamagedLastTurn && !gainedVice) {
                bune.activeTokens.RemoveAll(t => t.tokenName == "Vice");
            }
        }
    }

    public IEnumerator CowboyTurnStartLogic(BattleEntities cowboy)
    {
        if (cowboy.activeTokens.All(t => t.tokenName != "Vice") && !cowboy.myFirstTurn) {
                            
            float viceRoll = Random.Range(1, 101);
            if (viceRoll < currentActoutChance) {
                yield return StartCoroutine(battleSystem.CowboyViceActOut(cowboy));
                                
                currentActoutChance += COWBOY_ACTOUT_INCREASE;
                if (currentActoutChance > COWBOY_MAX_ACTOUT) {
                    currentActoutChance = COWBOY_MAX_ACTOUT;
                }
                
                yield break;
            }
        } else {
            currentActoutChance = COWBOY_BASE_ACTOUT;
        }

        gainedVice = false;
    }
    
    public bool CowboyUseLogic(BattleEntities cowboy, Ability ability)
    {
        bool blockAbility = false;
        switch (ability.abilityName) {
            case "Hedonist Headbutt":
                if (cowboy.activeTokens.All(t => t.tokenName != "Vice")) {
                    blockAbility = true;
                }
                break;
            case "Rampage":
                if (cowboy.activeTokens.All(t => t.tokenName != "Vice")) {
                    int viceIndex = cowboy.activeTokens.FindIndex(t => t.tokenName != "Vice");
                    if (cowboy.activeTokens[viceIndex].tokenCount < 2) {
                        blockAbility = true;
                    }
                }
                break;
        }

        return blockAbility;
    }
}
