using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class AngelDance1: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.AngelDance;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
            var anchor = CombatManager.Instance.EnemiesFxAnchor;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

            FxManager.PlayFxAtPosition(anchor.position, FxType.AngelDance, 0.4f);
            FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.AngelDance3);

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);

            targetCharacter.CharacterStats.ApplyStatus(StatusType.Fragile, 2);


            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
              
        }
    }
}