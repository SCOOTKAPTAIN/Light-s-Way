using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class EnemyHealAction : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Heal;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return Mathf.RoundToInt(ApplyLightMultiplier(baseValue, actionData));
        }
        public override void DoAction(EnemyActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;

            if (!newTarget) return;
            
            var value = CalculateValue(actionParameters.Value, actionParameters.SelfCharacter, newTarget, actionParameters.ActionData);
            var healthBefore = newTarget.CharacterStats.CurrentHealth;
            newTarget.CharacterStats.Heal(value);

            if (!ShouldSuppressPresentation(actionParameters) && FxManager != null)
            {
                var actualHealing = newTarget.CharacterStats.CurrentHealth - healthBefore;
                if (actualHealing > 0)
                {
                    var textRoot = newTarget.TextSpawnRoot != null
                        ? newTarget.TextSpawnRoot
                        : newTarget.transform;
                    FxManager.SpawnFloatingTextGreen(textRoot, actualHealing.ToString());
                }
            }

            PlayActionFx(actionParameters, newTarget.transform, FxType.Heal);
            PlayActionAudio(actionParameters, AudioActionType.Heal);
        }
    }
}