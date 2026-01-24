using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class GrandCross2: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.GrandCross2;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var fragileStacks = targetCharacter.CharacterStats.StatusDict[StatusType.Fragile].StatusValue;
            var damage = fragileStacks * 4;

            damage = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, damage));

            if (FxManager != null)
                FxManager.PlayFxAtPosition(targetCharacter.transform.position, FxType.GrandCross2);

            targetCharacter.CharacterStats.Damage(damage, false, "red", selfCharacter);

            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.GrandCross2);
        }
    }
}