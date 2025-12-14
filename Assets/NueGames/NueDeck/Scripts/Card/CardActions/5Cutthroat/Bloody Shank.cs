using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class BloodyShank: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.BloodyShank;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue
             + targetCharacter.CharacterStats.StatusDict[StatusType.Bleeding].StatusValue;

            FxManager.PlayFx(actionParameters.TargetCharacter.transform, FxType.BloodyShank);
            FxManager.PlayFx(targetCharacter.transform, FxType.Bleed);
              

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);

            targetCharacter.CharacterStats.ApplyStatus(StatusType.Bleeding, 5);

            targetCharacter.CharacterStats.ApplyStatus(StatusType.Bleeding, targetCharacter.CharacterStats.StatusDict[StatusType.Bleeding].StatusValue);


           

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
              
        }
    }
}