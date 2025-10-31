using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using Unity.Mathematics;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class ShieldSiphon2: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.ShieldSiphon2;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            FxManager.PlayFx(selfCharacter.transform, FxType.ShieldSiphon2);
            int blockAmount = 0;
            if (CombatManager.Instance.TryConsumeActionContext<int>("SiphonedShield", out var consumedAmount))
            {
                blockAmount = consumedAmount;
            }
            else
            {
            
                blockAmount = Mathf.RoundToInt(actionParameters.Value);
            }

            if (blockAmount > 0)
            {
                selfCharacter.CharacterStats.ApplyStatus(StatusType.Block, blockAmount);
            }
            
            FxManager.PlayFx(selfCharacter.transform, FxType.ShieldSiphon2);
            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.ShieldSiphon2);
              
        }
    }
}