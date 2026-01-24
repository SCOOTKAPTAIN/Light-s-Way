using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class ShieldSiphon3: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.ShieldSiphon3;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

            FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.ShieldSiphon3, new Vector3(-0.3f, 0, 0));

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);


           

            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.ShieldSiphon3);
              
        }
    }
}