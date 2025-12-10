using System;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class ReapersHarvest2: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.ReapersHarvest2;
        public override void DoAction(CardActionParameters actionParameters)
        {
            
            var anchor = CombatManager.Instance.EnemiesFxAnchor;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue
             + targetCharacter.CharacterStats.StatusDict[StatusType.Bleeding].StatusValue;

            FxManager.PlayFxAtPosition(anchor.position, FxType.ReapersHarvest2, 0.4f);
            FxManager.PlayFx(targetCharacter.transform, FxType.Bleed);

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value));

            targetCharacter.CharacterStats.ApplyStatus(StatusType.Bleeding, 2);


            if (FxManager != null)
            {
                FxManager.SpawnFloatingText(actionParameters.TargetCharacter.TextSpawnRoot, value.ToString());
            }

            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.ReapersHarvest2);
              
        }
    }
}