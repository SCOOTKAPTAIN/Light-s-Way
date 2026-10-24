using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Card;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class IllusionofVictory2 : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.IllusionOfVictory2;
        public override void DoAction(CardActionParameters actionParameters)
        {
           var targetCharacter = actionParameters.TargetCharacter;
           var selfCharacter = actionParameters.SelfCharacter;
           var anchor = CombatManager.Instance.EnemiesFxAnchor;
            
            if (!targetCharacter) return;

            // Potency scales the Strength that was actually converted, but cannot
            // create debuffs when there was no Strength to convert.
            if (!CombatManager.Instance.TryGetActionContext<int>("LostStrength", out var strengthlostAmount) ||
                strengthlostAmount <= 0)
                return;

            var debuffvalue = Mathf.RoundToInt(CardScaling.AddPotency(strengthlostAmount));

            if (debuffvalue > 0)
            {
                targetCharacter.CharacterStats.ApplyStatus(StatusType.Weak, debuffvalue);
                targetCharacter.CharacterStats.ApplyStatus(StatusType.Poison, debuffvalue);
                targetCharacter.CharacterStats.ApplyStatus(StatusType.Sabotaged, debuffvalue);
            }
            

            if (FxManager != null)
                 FxManager.PlayFx(selfCharacter.transform, FxType.IllusionOfVictory2, new Vector3(0f, 0.4f, 0f));             
            if (AudioManager != null) 
                AudioManager.PlayOneShot(AudioActionType.IllusionOfVictory2);
        }
    }
}