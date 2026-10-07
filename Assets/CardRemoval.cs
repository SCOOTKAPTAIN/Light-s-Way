using UnityEngine;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Data.Collection;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.UI;

public class CardRemovalManager : MonoBehaviour
{
    [SerializeField] private InventoryCanvas inventoryCanvas;
    private System.Action<bool> onCompleted;

    private void Awake()
    {
        if (inventoryCanvas == null)
            inventoryCanvas = FindFirstObjectByType<InventoryCanvas>(FindObjectsInactive.Include);
    }

    public bool HasRemovableCard()
    {
        return GameManager.Instance != null
            && GameManager.Instance.PersistentGameplayData != null
            && GameManager.Instance.PersistentGameplayData.CurrentCardsList != null
            && GameManager.Instance.PersistentGameplayData.CurrentCardsList.Count > 0;
    }

    public void OpenCardRemovalScreen(System.Action<bool> onCompleted)
    {
        this.onCompleted = onCompleted;

        if (inventoryCanvas == null)
            inventoryCanvas = FindFirstObjectByType<InventoryCanvas>(FindObjectsInactive.Include);

        if (inventoryCanvas == null)
        {
            Debug.LogWarning("Cannot open card removal because InventoryCanvas is not available.", this);
            CompleteRemoval(false, this.onCompleted);
            return;
        }

        if (!HasRemovableCard())
        {
            Debug.LogWarning("Cannot open card removal because the persistent deck is empty.", this);
            CompleteRemoval(false, this.onCompleted);
            return;
        }

        List<CardData> deck = GameManager.Instance.PersistentGameplayData.CurrentCardsList;

        inventoryCanvas.OpenCanvas();
        inventoryCanvas.ChangeTitle("Choose a card to remove");
        inventoryCanvas.SetCardsForRemoval(deck, OnCardChosen);
    }

    private void OnCardChosen(CardData chosenCard)
    {
        if (chosenCard == null || !HasRemovableCard())
        {
            CompleteRemoval(false, onCompleted);
            return;
        }

        GameManager.Instance.PersistentGameplayData.CurrentCardsList.Remove(chosenCard);
        CompleteRemoval(true, onCompleted);
    }

    private void CompleteRemoval(bool removed, System.Action<bool> onCompleted)
    {
        if (removed)
            inventoryCanvas.CloseCanvas();

        this.onCompleted = null;
        onCompleted?.Invoke(removed);
    }
}
