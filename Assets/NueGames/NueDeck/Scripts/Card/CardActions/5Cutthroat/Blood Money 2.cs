using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class BloodMoney2: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.BloodMoney2;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            

                       FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.BloodMoney2);
            FxManager.PlayFxAtPosition(targetCharacter.transform.position, FxType.Bleed);
                        FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.BloodMoney3);

                        var bleedValue = targetCharacter.CharacterStats.StatusDict[StatusType.Bleeding].StatusValue;
                        var damage = Mathf.RoundToInt(bleedValue * 3f);

                        // Grant gold immediately before dealing damage (in case damage ends combat)
                        GameManager.Instance.PersistentGameplayData.CurrentGold += damage;
                        UIManager.Instance.InformationCanvas.SetGoldText(GameManager.Instance.PersistentGameplayData.CurrentGold);

                        // Store the bleeding value for Blood Money 3 to use for gold calculation (backup)
                        CombatManager.SetActionContext("BloodMoneyBleedingDetonated", bleedValue);

                        // Detonate: deal damage equal to bleeding * 2. Use piercing damage to match bleeding ticks.
                        //targetCharacter.CharacterStats.Damage(damage, true);
                        targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(damage), false, "red", selfCharacter);
                        

                      

                        // Remove bleeding stacks from the target
                        targetCharacter.CharacterStats.ClearStatus(StatusType.Bleeding);

            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.BloodMoney2);
                AudioManager.PlayOneShot(AudioActionType.BloodMoney3);
              
        }
    }
}