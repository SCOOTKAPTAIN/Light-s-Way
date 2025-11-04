using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class BloodMoney: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.BloodMoney;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            

                        FxManager.PlayFx(actionParameters.TargetCharacter.transform, FxType.BloodMoney);
                        FxManager.PlayFx(targetCharacter.transform, FxType.Bleed);

                        var bleedValue = targetCharacter.CharacterStats.StatusDict[StatusType.Bleeding].StatusValue;
                        var damage = Mathf.RoundToInt(bleedValue * 2f);

                        // Detonate: deal damage equal to bleeding * 2. Use piercing damage to match bleeding ticks.
                        targetCharacter.CharacterStats.Damage(damage, true);

                        // Reward gold equal to the amount of bleeding detonated (not the damage)
                        GameManager.Instance.PersistentGameplayData.CurrentGold += bleedValue;
                        UIManager.Instance.InformationCanvas.SetGoldText(GameManager.Instance.PersistentGameplayData.CurrentGold);

                        if (FxManager != null)
                        {
                                FxManager.SpawnFloatingText(actionParameters.TargetCharacter.TextSpawnRoot, damage.ToString());
                        }

                        // Remove bleeding stacks from the target
                        targetCharacter.CharacterStats.ClearStatus(StatusType.Bleeding);

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
              
        }
    }
}