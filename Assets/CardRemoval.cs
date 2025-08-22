using UnityEngine;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Data.Collection;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.UI;

public class CardRemovalManager : MonoBehaviour
{
    [SerializeField] private InventoryCanvas inventoryCanvas;

    public void OpenCardRemovalScreen()
    {
        // The player’s true deck (persists between combats)
        List<CardData> deck = GameManager.Instance.PersistentGameplayData.CurrentCardsList;

        inventoryCanvas.OpenCanvas();
        inventoryCanvas.ChangeTitle("Choose a card to remove");
        inventoryCanvas.SetCardsForRemoval(deck, OnCardChosen);
        
    }

    private void OnCardChosen(CardData chosenCard)
    {
        // Remove permanently from persistent deck
        GameManager.Instance.PersistentGameplayData.CurrentCardsList.Remove(chosenCard);

        // Close after one pick (one-time event)
        inventoryCanvas.CloseCanvas();
    }
}
