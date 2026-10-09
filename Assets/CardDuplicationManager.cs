using System;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Data.Collection;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.UI;
using UnityEngine;

public sealed class CardDuplicationManager : MonoBehaviour
{
    [SerializeField] private InventoryCanvas inventoryCanvas;

    private void Awake()
    {
        if (inventoryCanvas == null)
            inventoryCanvas = FindFirstObjectByType<InventoryCanvas>(FindObjectsInactive.Include);
    }

    public bool HasDuplicableCard()
    {
        return GameManager.Instance != null
            && GameManager.Instance.PersistentGameplayData != null
            && GameManager.Instance.PersistentGameplayData.CurrentCardsList != null
            && GameManager.Instance.PersistentGameplayData.CurrentCardsList.Count > 0;
    }

    public void OpenCardDuplicationScreen(Action<bool> onCompleted)
    {
        if (inventoryCanvas == null)
            inventoryCanvas = FindFirstObjectByType<InventoryCanvas>(FindObjectsInactive.Include);

        if (inventoryCanvas == null)
        {
            Debug.LogWarning("Cannot open card duplication because InventoryCanvas is not available.", this);
            onCompleted?.Invoke(false);
            return;
        }

        if (!HasDuplicableCard())
        {
            Debug.LogWarning("Cannot duplicate a card because the persistent deck is empty.", this);
            onCompleted?.Invoke(false);
            return;
        }

        List<CardData> deck = GameManager.Instance.PersistentGameplayData.CurrentCardsList;
        inventoryCanvas.OpenCanvas();
        inventoryCanvas.ChangeTitle("Choose a card to duplicate");
        inventoryCanvas.SetCardsForSelection(deck, OnCardChosen);
        completedCallback = onCompleted;
    }

    private Action<bool> completedCallback;

    private void OnCardChosen(CardData chosenCard)
    {
        if (chosenCard == null || !HasDuplicableCard())
        {
            Complete(false);
            return;
        }

        GameManager.Instance.PersistentGameplayData.CurrentCardsList.Add(chosenCard);
        inventoryCanvas.CloseCanvas();
        Complete(true);
    }

    private void Complete(bool duplicated)
    {
        var callback = completedCallback;
        completedCallback = null;
        callback?.Invoke(duplicated);
    }
}
