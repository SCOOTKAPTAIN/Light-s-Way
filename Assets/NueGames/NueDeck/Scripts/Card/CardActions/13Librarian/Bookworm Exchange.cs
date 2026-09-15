using NueGames.NueDeck.Scripts.Card;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class BookwormExchange : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.BookwormExchange;

        public override void DoAction(CardActionParameters actionParameters)
        {
            if (CollectionManager == null || CollectionManager.HandController == null)
                return;

            CollectionManager.ExhaustHandAndDraw();

            if (FxManager != null && actionParameters.SelfCharacter != null)
                FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.BookwormExchange);

            if (AudioManager != null && actionParameters.CardData != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}