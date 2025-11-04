using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class MurderGoRound: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.MurderGoRound;
        public override void DoAction(CardActionParameters actionParameters)
        {

            var anchor = CombatManager.Instance.EnemiesFxAnchor;
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue
             + targetCharacter.CharacterStats.StatusDict[StatusType.Bleeding].StatusValue;

            FxManager.PlayFxAtPosition(anchor.position, FxType.MurderGoRound, 0.2f);
            FxManager.PlayFx(targetCharacter.transform, FxType.Bleed);
              

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value));

            targetCharacter.CharacterStats.ApplyStatus(StatusType.Bleeding, 5);


            if (FxManager != null)
            {
                FxManager.SpawnFloatingText(actionParameters.TargetCharacter.TextSpawnRoot, value.ToString());
            }

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
              
        }
    }
}