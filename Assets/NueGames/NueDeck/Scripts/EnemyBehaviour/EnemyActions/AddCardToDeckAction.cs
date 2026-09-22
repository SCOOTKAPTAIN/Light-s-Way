using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class AddCardToDeckAction : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.AddCardToDeck;

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            var cardToAdd = actionParameters.ActionData?.CardToAdd;
            if (cardToAdd == null || CollectionManager == null)
                return;

            CollectionManager.AddCardToDrawPileWithAnimation(cardToAdd);
        }
    }
}