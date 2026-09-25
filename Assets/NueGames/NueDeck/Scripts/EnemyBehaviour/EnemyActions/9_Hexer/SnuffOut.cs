using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class SnuffOut : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.SnuffOut;
        public override bool UsesDamageModifiersForPreview => true;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateDamageValue(baseValue, selfCharacter, targetCharacter, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (actionParameters?.SelfCharacter == null || actionParameters.TargetCharacter == null)
                return;

            var target = actionParameters.TargetCharacter;
            var value = CalculateValue(actionParameters.Value, actionParameters.SelfCharacter, target, actionParameters.ActionData);
            target.CharacterStats.Damage(value, false, "red", actionParameters.SelfCharacter);

            if (actionParameters.SelfCharacter.CharacterStats.StatusDict[StatusType.TotalAssimilation].IsActive)
            {
                GameManager.Instance.PersistentGameplayData.ChangeLight(-20);
                FxManager?.SpawnStaticText(target.transform, "Light drained!", 0, 1);
            }
            else
                target.CharacterStats.ApplyStatus(StatusType.Flickering, 2, actionParameters.SelfCharacter);

            actionParameters.SelfCharacter.CharacterStats.ApplyStatus(StatusType.Assimilation, 3);

            PlayActionFx(actionParameters, target.transform, FxType.Debuff);
            PlayActionAudio(actionParameters, AudioActionType.Power);
        }
    }
}