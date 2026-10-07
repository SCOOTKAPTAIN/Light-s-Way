using UnityEngine;
using Yarn.Unity;
using NueGames.NueDeck.Scripts.Managers;
using TMPro;
using UnityEngine.UI;
using System;
using NueGames.NueDeck.Scripts.Data.Collection.RewardData;
using NueGames.NueDeck.Scripts.UI.Reward;

public sealed class MerchantYarnCommands : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private CanvasGroup linePresenterCanvasGroup;
    [SerializeField] private CardRemovalManager cardRemovalManager;

    [Header("Merchant Procurement")]
    [SerializeField] private CardRewardData premiumProcurementRewardData;

    private const int CardRemovalCost = 300;
    private bool cardRemovalInProgress;

    private void Awake()
    {
        if (linePresenterCanvasGroup == null && dialogueRunner != null)
            linePresenterCanvasGroup = dialogueRunner.GetComponentInChildren<CanvasGroup>(true);

        if (cardRemovalManager == null)
            cardRemovalManager = GetComponent<CardRemovalManager>();

        WireCardRemovalButton();
        WireMerchantButton("Card Procurement", "merchant_card_procurement");
        WireMerchantButton("Premium Card Procurement", "merchant_premium_procurement");
        WireExitButton();
        SetShopOpen(false);
    }

    [YarnCommand("open_shop_panel")]
    public void OpenShopPanel()
    {
        if (shopPanel == null)
        {
            Debug.LogWarning("Cannot open the merchant shop because Shop Panel is not assigned.", this);
            return;
        }

        if (!cardRemovalInProgress)
            SetShopOpen(true);
    }
    
    [YarnCommand("close_shop_panel")]
    public void CloseShopPanel()
    {
        SetShopOpen(false);
    }

    [YarnCommand("merchant_purchase_success")]
    public static void PlayPurchaseSuccessSfx()
    {
        if (DialogueAudioManager.instance != null)
            DialogueAudioManager.instance.PlaySFX("buy");
    }

    [YarnCommand("merchant_purchase_broke")]
    public static void PlayPurchaseBrokeSfx()
    {
        if (DialogueAudioManager.instance != null)
            DialogueAudioManager.instance.PlaySFX("broke");
    }

    public async void StartMerchantNode(string nodeName)
    {
        if (dialogueRunner == null)
        {
            Debug.LogWarning("Cannot start merchant dialogue because DialogueRunner is not assigned.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(nodeName))
        {
            Debug.LogWarning("Cannot start merchant dialogue because the Yarn node name is empty.", this);
            return;
        }

        SetShopOpen(false);

        await dialogueRunner.StartDialogue(nodeName);

        SetShopOpen(true);
    }

    [YarnCommand("open_card_removal")]
    public void OpenCardRemoval()
    {
        if (cardRemovalManager == null)
        {
            Debug.LogWarning("Cannot open card removal because CardRemovalManager is not assigned.", this);
            return;
        }

        cardRemovalInProgress = true;
        cardRemovalManager.OpenCardRemovalScreen(OnCardRemovalCompleted);
    }

    private async void OnCardRemovalCompleted(bool removed)
    {
        cardRemovalInProgress = false;

        if (!removed)
        {
            SetShopOpen(true);
            return;
        }

        YarnGameplayCommands.ChangeGold(-CardRemovalCost);
        PlayPurchaseSuccessSfx();
        await dialogueRunner.StartDialogue("merchant_card_removal_complete");
        SetShopOpen(true);
    }

    [YarnFunction("has_removable_card")]
    public static bool HasRemovableCard()
    {
        CardRemovalManager manager = FindFirstObjectByType<CardRemovalManager>();
        return manager != null && manager.HasRemovableCard();
    }

    [YarnFunction("has_premium_procurement")]
    public static bool HasPremiumProcurement()
    {
        MerchantYarnCommands merchant = FindFirstObjectByType<MerchantYarnCommands>();
        return merchant != null && merchant.premiumProcurementRewardData != null;
    }

    [YarnCommand("open_default_card_procurement")]
    public async YarnTask OpenDefaultCardProcurement()
    {
        var rewardCanvas = UIManager.Instance?.RewardCanvas;
        if (rewardCanvas == null)
        {
            Debug.LogWarning("Cannot open normal procurement because RewardCanvas is missing.", this);
            return;
        }

        var completionSource = new YarnTaskCompletionSource<bool>();
        if (!rewardCanvas.OpenDefaultCardReward(() => completionSource.TrySetResult(true)))
            return;

        await completionSource.Task;
        YarnGameplayCommands.ChangeGold(-200);
        PlayPurchaseSuccessSfx();
    }

    [YarnCommand("open_premium_card_procurement")]
    public async YarnTask OpenPremiumCardProcurement()
    {
        var rewardCanvas = UIManager.Instance?.RewardCanvas;
        if (rewardCanvas == null || premiumProcurementRewardData == null)
        {
            Debug.LogWarning("Cannot open premium procurement because RewardCanvas or premium reward data is missing.", this);
            return;
        }

        var completionSource = new YarnTaskCompletionSource<bool>();
        if (!rewardCanvas.OpenCardReward(premiumProcurementRewardData, () => completionSource.TrySetResult(true)))
            return;

        await completionSource.Task;
        YarnGameplayCommands.ChangeGold(-600);
        PlayPurchaseSuccessSfx();
    }

    private void WireCardRemovalButton()
    {
        if (shopPanel == null)
            return;

        foreach (TMP_Text label in shopPanel.GetComponentsInChildren<TMP_Text>(true))
        {
            if (!string.Equals(label.text.Trim(), "Card Removal Service", System.StringComparison.OrdinalIgnoreCase))
                continue;

            Button button = label.GetComponentInParent<Button>();
            if (button == null)
                continue;

            button.onClick.AddListener(() => StartMerchantNode("merchant_card_removal"));
            return;
        }
    }

    private void WireMerchantButton(string labelText, string nodeName)
    {
        if (shopPanel == null)
            return;

        foreach (TMP_Text label in shopPanel.GetComponentsInChildren<TMP_Text>(true))
        {
            if (!string.Equals(label.text.Trim(), labelText, StringComparison.OrdinalIgnoreCase))
                continue;

            Button button = label.GetComponentInParent<Button>();
            if (button == null)
                continue;

            button.onClick.AddListener(() => StartMerchantNode(nodeName));
            return;
        }
    }

    private void WireExitButton()
    {
        if (shopPanel == null)
            return;

        foreach (TMP_Text label in shopPanel.GetComponentsInChildren<TMP_Text>(true))
        {
            if (!string.Equals(label.text.Trim(), "Exit", StringComparison.OrdinalIgnoreCase))
                continue;

            Button button = label.GetComponentInParent<Button>();
            if (button == null)
                continue;

            button.onClick.AddListener(StartExitDialogue);
            return;
        }
    }

    private async void StartExitDialogue()
    {
        SetShopOpen(false);

        if (dialogueRunner == null)
        {
            Debug.LogWarning("Cannot start merchant goodbye because DialogueRunner is not assigned.", this);
            return;
        }

        await dialogueRunner.StartDialogue("merchant_bye_bye");
    }

    private void SetShopOpen(bool isOpen)
    {
        if (shopPanel != null)
            shopPanel.SetActive(isOpen);

        if (linePresenterCanvasGroup != null)
            linePresenterCanvasGroup.blocksRaycasts = !isOpen;
    }
}
