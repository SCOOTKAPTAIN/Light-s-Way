using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Card;
using System.Linq;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class MegaFlare: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.MegaFlare;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
            var anchor = CombatManager.Instance.EnemiesFxAnchor;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

             FxManager.Instance.PlayFxAtPosition(anchor.position, FxType.MegaFlare, 0.4f);              

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);

            // Apply 2 Burning stacks
            targetCharacter.CharacterStats.ApplyStatus(
                StatusType.Burning,
                Mathf.RoundToInt(CardScaling.AddPotency(2f)),
                selfCharacter);


            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}