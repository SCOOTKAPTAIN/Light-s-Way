using System;
using System.Collections.Generic;
using System.Text;
using NueGames.NueDeck.Scripts.Data.Collection;
using NueGames.NueDeck.Scripts.Data.Collection.RewardData;
using NueGames.NueDeck.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Markup;
using Yarn.Unity;

public sealed class YarnGameplayCommands : ReplacementMarkupHandler
{
    [Serializable]
    private struct NamedSprite
    {
        public string id;
        public Sprite sprite;
    }

    [Header("Screen Effects")]
    [SerializeField] private CanvasGroup screenEffectGroup;
    [SerializeField] private Image screenEffectImage;

    [Header("Screen Picture")]
    [SerializeField] private Image screenImage;
    [SerializeField] private NamedSprite[] pictures;

    [Header("Optional Background Animator")]
    [SerializeField] private Animator backgroundAnimator;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject speakerFrame;

    [Header("Bloom Text")]
    [SerializeField] private CanvasGroup bloomTextPanel;
    [SerializeField] private TMP_Text bloomText;
    [SerializeField, Min(0f)] private float bloomFadeInDuration = 0.2f;
    [SerializeField, Min(0f)] private float bloomHoldDuration = 1f;
    [SerializeField, Min(0f)] private float bloomFadeOutDuration = 0.4f;
    [SerializeField, Min(0.01f)] private float bloomStartScale = 0.85f;
    [SerializeField, Min(1f)] private float bloomPeakScale = 1.05f;

    [Header("Intro Rewards")]
    [SerializeField] private CardRewardData randomUncommonCardPool;

    [Header("Event Card Rewards")]
    [SerializeField] private CardRewardData commonPlusCardPool;
    [SerializeField] private CardRewardData uncommonPlusCardPool;
    [SerializeField] private CardRewardData rarePlusCardPool;
    [SerializeField] private CardRewardData mysticCardPool;

    private void Awake()
    {
        RegisterColorMarkup();

        if (screenEffectGroup != null)
        {
            screenEffectGroup.alpha = 0f;
            screenEffectGroup.interactable = false;
            screenEffectGroup.blocksRaycasts = false;
        }

        if (bloomTextPanel != null)
        {
            bloomTextPanel.alpha = 0f;
            bloomTextPanel.interactable = false;
            bloomTextPanel.blocksRaycasts = false;
            bloomTextPanel.gameObject.SetActive(false);
        }
    }

    private void RegisterColorMarkup()
    {
        var dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        var lineProvider = dialogueRunner?.LineProvider as LineProviderBehaviour;
        if (lineProvider == null)
        {
            Debug.LogWarning("Cannot register Yarn color markup because the line provider is missing.", this);
            return;
        }

        lineProvider.RegisterMarkerProcessor("gold", this);
        lineProvider.RegisterMarkerProcessor("green", this);
        lineProvider.RegisterMarkerProcessor("red", this);
    }

    public override ReplacementMarkerResult ProcessReplacementMarker(
        MarkupAttribute marker,
        StringBuilder childBuilder,
        List<MarkupAttribute> childAttributes,
        string localeCode)
    {
        string color = marker.Name.ToLowerInvariant() switch
        {
            "gold" => "#FFB430",
            "green" => "green",
            "red" => "red",
            _ => null
        };

        if (color != null)
        {
            childBuilder.Insert(0, $"<color={color}>");
            childBuilder.Append("</color>");
        }

        return new ReplacementMarkerResult
        {
            Diagnostics = new List<LineParser.MarkupDiagnostic>(),
            InvisibleCharacters = 0
        };
    }

    [YarnCommand("fade_in")]
    public YarnTask FadeIn(float duration)
    {
        return FadeToAsync(Color.black, 0f, duration);
    }

    [YarnCommand("fade_out")]
    public YarnTask FadeOut(float duration)
    {
        return FadeToAsync(Color.black, 1f, duration);
    }

    [YarnCommand("hide_information")]
    public void HideInformation()
    {
        UIManager.Instance?.InformationCanvas?.CloseCanvas();
    }

    [YarnCommand("show_information")]
    public void ShowInformation()
    {
        UIManager.Instance?.InformationCanvas?.OpenCanvas();
    }

    [YarnCommand("flash")]
    public async YarnTask Flash(string colorName, float duration)
    {
        if (!TryGetEffectColor(colorName, out var color))
        {
            Debug.LogWarning($"Unknown Yarn flash color '{colorName}'. Use white, red, black, or a hex color.", this);
            return;
        }

        await FadeToAsync(color, 1f, duration * 0.5f);
        await FadeToAsync(color, 0f, duration * 0.5f);
    }

    [YarnCommand("flash_to_black")]
    public async YarnTask FlashToBlack(float whiteFadeDuration, float blackFadeDuration)
    {
        if (screenEffectGroup == null || screenEffectImage == null)
        {
            Debug.LogWarning("Yarn screen effect references are not assigned.", this);
            return;
        }

        screenEffectGroup.gameObject.SetActive(true);
        await FadeToAsync(Color.white, 1f, whiteFadeDuration);
        await FadeColorToAsync(Color.black, blackFadeDuration);
    }

    [YarnCommand("sfx")]
    public void PlaySfx(string name)
    {
        if (DialogueAudioManager.instance == null)
        {
            Debug.LogWarning("Cannot play Yarn SFX because DialogueAudioManager.instance is missing.", this);
            return;
        }

        DialogueAudioManager.instance.PlaySFX(name);
    }

    [YarnCommand("playmusic")]
    public void PlayMusic(string name)
    {
        if (DialogueAudioManager.instance == null)
        {
            Debug.LogWarning("Cannot play Yarn music because DialogueAudioManager.instance is missing.", this);
            return;
        }

        DialogueAudioManager.instance.PlayMusic(name);
    }

    [YarnCommand("pausemusic")]
    public void PauseMusic()
    {
        if (DialogueAudioManager.instance == null)
        {
            Debug.LogWarning("Cannot pause Yarn music because DialogueAudioManager.instance is missing.", this);
            return;
        }

        DialogueAudioManager.instance.PauseMusic();
    }

    [YarnCommand("hide_speaker_frame")]
    public void HideSpeakerFrame()
    {
        if (speakerFrame == null)
        {
            Debug.LogWarning("Cannot hide the speaker frame because it is not assigned.", this);
            return;
        }

        speakerFrame.SetActive(false);
    }

    [YarnCommand("show_speaker_frame")]
    public void ShowSpeakerFrame()
    {
        if (speakerFrame != null)
        {
            speakerFrame.SetActive(true);
        }
    }

    [YarnCommand("bloom_text")]
    public YarnTask BloomText(string message)
    {
        return BloomTextAsync(message);
    }

    [YarnCommand("card_reward")]
    public async YarnTask CardReward(string poolName)
    {
        var rewardCanvas = UIManager.Instance?.RewardCanvas;
        if (rewardCanvas == null)
        {
            Debug.LogWarning("Cannot open a Yarn card reward because UIManager.RewardCanvas is missing.", this);
            return;
        }

        var completionSource = new YarnTaskCompletionSource<bool>();
        if (!rewardCanvas.OpenCardReward(poolName, () => completionSource.TrySetResult(true)))
            return;

        await completionSource.Task;
    }

    [YarnCommand("gain_common_cards")]
    public YarnTask GainCommonCards()
    {
        return OpenConfiguredCardReward(commonPlusCardPool, "common+");
    }

    [YarnCommand("gain_uncommon_cards")]
    public YarnTask GainUncommonCards()
    {
        return OpenConfiguredCardReward(uncommonPlusCardPool, "uncommon+");
    }

    [YarnCommand("gain_rare_cards")]
    public YarnTask GainRareCards()
    {
        return OpenConfiguredCardReward(rarePlusCardPool, "rare+");
    }

    [YarnCommand("gain_mystic_cards")]
    public YarnTask GainMysticCards()
    {
        return OpenConfiguredCardReward(mysticCardPool, "mystic");
    }

    [YarnCommand("remove_card")]
    public YarnTask RemoveCard()
    {
        var removalManager = FindFirstObjectByType<CardRemovalManager>(FindObjectsInactive.Include);
        if (removalManager == null)
            removalManager = gameObject.AddComponent<CardRemovalManager>();

        if (!removalManager.HasRemovableCard())
        {
            Debug.LogWarning("Cannot remove a card because the persistent deck is empty.", this);
            return YarnTask.CompletedTask;
        }

        var completionSource = new YarnTaskCompletionSource<bool>();
        removalManager.OpenCardRemovalScreen(removed => completionSource.TrySetResult(removed));
        return WaitForCardRemoval(completionSource);
    }

    [YarnCommand("duplicate_card")]
    public YarnTask DuplicateCard()
    {
        var duplicationManager = FindFirstObjectByType<CardDuplicationManager>(FindObjectsInactive.Include);
        if (duplicationManager == null)
            duplicationManager = gameObject.AddComponent<CardDuplicationManager>();

        if (!duplicationManager.HasDuplicableCard())
        {
            Debug.LogWarning("Cannot duplicate a card because the persistent deck is empty.", this);
            return YarnTask.CompletedTask;
        }

        var completionSource = new YarnTaskCompletionSource<bool>();
        duplicationManager.OpenCardDuplicationScreen(
            duplicated => completionSource.TrySetResult(duplicated));
        return WaitForCardDuplication(completionSource);
    }

    [YarnCommand("give_card")]
    public void GiveCard(string cardName)
    {
        var gameManager = GameManager.Instance;
        var gameplayData = gameManager?.PersistentGameplayData;
        var allCards = gameManager?.GameplayData?.AllCardsList;
        if (gameplayData == null || allCards == null)
        {
            Debug.LogWarning($"Cannot give card '{cardName}' because card data is unavailable.", this);
            return;
        }

        var cardData = allCards.Find(card =>
            card != null && string.Equals(card.CardName, cardName, StringComparison.OrdinalIgnoreCase));
        if (cardData == null)
        {
            Debug.LogWarning($"Cannot give card because no card named '{cardName}' was found.", this);
            return;
        }

        gameplayData.CurrentCardsList.Add(cardData);
        Debug.Log($"Added '{cardData.CardName}' to the player's deck.", this);
    }

    [YarnCommand("change_gold")]
    public static void ChangeGold(int amount)
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData == null)
        {
            Debug.LogWarning("Cannot change Yarn gold because PersistentGameplayData is missing.");
            return;
        }

        gameplayData.CurrentGold = Mathf.Max(0, gameplayData.CurrentGold + amount);
        UIManager.Instance?.InformationCanvas?.SetGoldText(gameplayData.CurrentGold);
    }

    [YarnCommand("remove_initial_card")]
    public static void RemoveInitialCard(string cardName)
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData == null)
            return;

        gameplayData.QueueInitialCardRemoval(cardName);
    }

    [YarnCommand("add_random_uncommon_card")]
    public static async YarnTask AddRandomUncommonCard()
    {
        var rewardCanvas = UIManager.Instance?.RewardCanvas;
        var commandTarget = FindFirstObjectByType<YarnGameplayCommands>();
        if (rewardCanvas == null || commandTarget == null || commandTarget.randomUncommonCardPool == null)
        {
            Debug.LogWarning("Cannot open the intro card reward because the reward canvas or dedicated CardRewardData pool is missing.");
            return;
        }

        var completionSource = new YarnTaskCompletionSource<bool>();
        if (!rewardCanvas.OpenCardReward(commandTarget.randomUncommonCardPool, 3, 3, () => completionSource.TrySetResult(true)))
        {
            return;
        }

        await completionSource.Task;
    }

    [YarnCommand("reset_intro_bonuses")]
    public static void ResetIntroBonuses()
    {
        var gameManager = GameManager.Instance;
        var gameplayData = gameManager?.PersistentGameplayData;
        if (gameManager == null || gameplayData == null)
            return;

        gameplayData.ResetIntroBonuses();
        gameManager.SetInitalHand();
        UIManager.Instance?.InformationCanvas?.RefreshStatsText();
        UIManager.Instance?.InformationCanvas?.SetGoldText(gameplayData.CurrentGold);
        UIManager.Instance?.InformationCanvas?.SetLightText(gameplayData.Light);
        UIManager.Instance?.InformationCanvas?.SetProficiencyText(gameplayData.Proficiency);
    }

    [YarnCommand("change_proficiency")]
    public static void ChangeProficiency(int amount)
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData == null)
        {
            Debug.LogWarning("Cannot change Yarn proficiency because PersistentGameplayData is missing.");
            return;
        }

        gameplayData.Proficiency = Mathf.Max(0, gameplayData.Proficiency + amount);
        UIManager.Instance?.InformationCanvas?.SetProficiencyText(gameplayData.Proficiency);
    }

    [YarnCommand("change_wisdom")]
    public static void ChangeWisdom(int amount)
    {
        ChangePersistentStat("wisdom", amount, value => GameManager.Instance.PersistentGameplayData.Wisdom = value);
    }

    [YarnCommand("change_potency")]
    public static void ChangePotency(int amount)
    {
        ChangePersistentStat("potency", amount, value => GameManager.Instance.PersistentGameplayData.Potency = value);
    }

    [YarnCommand("change_affinity")]
    public static void ChangeAffinity(int amount)
    {
        ChangePersistentStat("affinity", amount, value => GameManager.Instance.PersistentGameplayData.Affinity = value);
    }

    [YarnCommand("change_arcana")]
    public static void ChangeArcana(int amount)
    {
        ChangePersistentStat("arcana", amount, value => GameManager.Instance.PersistentGameplayData.Arcana = value);
    }

    [YarnCommand("enable_elite_boss_proficiency")]
    public static void EnableEliteBossProficiency()
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData == null)
        {
            Debug.LogWarning("Cannot enable Elite/Boss proficiency reward because PersistentGameplayData is missing.");
            return;
        }

        gameplayData.GainProficiencyAfterEliteOrBoss = true;
    }

    [YarnCommand("enable_insight_at_act_start")]
    public static void EnableInsightAtActStart()
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData == null)
            return;

        gameplayData.GainInsightAtActStart = true;
    }

    [YarnCommand("enable_light_at_act_start")]
    public static void EnableLightAtActStart()
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData == null)
            return;

        gameplayData.GainLightAtActStart = true;
    }

    [YarnCommand("change_metabolism")]
    public static void ChangeMetabolism(int amount)
    {
        ChangePersistentStat("metabolism", amount, value => GameManager.Instance.PersistentGameplayData.Metabolism = value);
    }

    [YarnCommand("change_vigor")]
    public static void ChangeVigor(int amount)
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData == null)
            return;

        int previousVigor = gameplayData.Vigor;
        gameplayData.Vigor = Mathf.Max(1, gameplayData.Vigor + amount);
        ApplyVigorToMaxHealth(gameplayData, gameplayData.Vigor - previousVigor);
        UIManager.Instance?.InformationCanvas?.RefreshStatsText();
        UIManager.Instance?.CombatCanvas?.LightCardSelectionPanel?.RefreshCostText();
    }

    private static void ApplyVigorToMaxHealth(
        NueGames.NueDeck.Scripts.Data.Settings.PersistentGameplayData gameplayData,
        int vigorDelta)
    {
        int newMaxHealth = Mathf.Max(1, gameplayData.Vigor);

        var combatManager = CombatManager.Instance;
        var ally = combatManager != null ? combatManager.CurrentMainAlly : null;
        if (ally != null && ally.CharacterStats != null)
        {
            ally.CharacterStats.MaxHealth = newMaxHealth;
            ally.CharacterStats.CurrentHealth = Mathf.Clamp(
                ally.CharacterStats.CurrentHealth + Mathf.Max(0, vigorDelta),
                1,
                newMaxHealth);
            ally.CharacterStats.OnHealthChanged?.Invoke(
                ally.CharacterStats.CurrentHealth,
                ally.CharacterStats.MaxHealth);

            gameplayData.SetAllyHealthData(
                ally.AllyCharacterData.CharacterID,
                ally.CharacterStats.CurrentHealth,
                ally.CharacterStats.MaxHealth);

            UIManager.Instance?.InformationCanvas?.SetHealthText(
                ally.CharacterStats.CurrentHealth,
                ally.CharacterStats.MaxHealth);
            return;
        }

        if (gameplayData.AllyHealthDataList.Count > 0)
        {
            var healthData = gameplayData.AllyHealthDataList[0];
            healthData.MaxHealth = newMaxHealth;
            healthData.CurrentHealth = Mathf.Clamp(
                healthData.CurrentHealth + Mathf.Max(0, vigorDelta),
                1,
                newMaxHealth);
            UIManager.Instance?.InformationCanvas?.SetHealthText(healthData.CurrentHealth, healthData.MaxHealth);
            return;
        }

        if (gameplayData.AllyList.Count > 0)
        {
            UIManager.Instance?.InformationCanvas?.SetHealthText(newMaxHealth, newMaxHealth);
        }
    }

    [YarnCommand("change_insight")]
    public static void ChangeInsight(int amount)
    {
        ChangePersistentStat("insight", amount, value => GameManager.Instance.PersistentGameplayData.Insight = value);
    }

    [YarnCommand("spend_insight")]
    public static void SpendInsight(int amount)
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData == null || amount <= 0)
            return;

        if (gameplayData.Insight < amount)
        {
            Debug.LogWarning($"Cannot spend {amount} Insight; only {gameplayData.Insight} is available.");
            return;
        }

        gameplayData.Insight -= amount;
        UIManager.Instance?.InformationCanvas?.RefreshStatsText();
    }

    [YarnCommand("change_abundance")]
    public static void ChangeAbundance(int amount)
    {
        ChangePersistentStat("abundance", amount, value => GameManager.Instance.PersistentGameplayData.Abundance = value);
    }

    [YarnCommand("change_capacity")]
    public static void ChangeCapacity(int amount)
    {
        ChangePersistentStat("capacity", amount, value => GameManager.Instance.PersistentGameplayData.Capacity = value);
    }

    [YarnCommand("change_radiance")]
    public static void ChangeRadiance(int amount)
    {
        ChangePersistentStat("radiance", amount, value => GameManager.Instance.PersistentGameplayData.Radiance = value);
    }

    private static void ChangePersistentStat(string statName, int amount, Action<int> setValue)
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData == null)
        {
            Debug.LogWarning($"Cannot change Yarn {statName} because PersistentGameplayData is missing.");
            return;
        }

        var currentValue = statName switch
        {
            "wisdom" => gameplayData.Wisdom,
            "potency" => gameplayData.Potency,
            "affinity" => gameplayData.Affinity,
            "arcana" => gameplayData.Arcana,
            "metabolism" => gameplayData.Metabolism,
            "vigor" => gameplayData.Vigor,
            "insight" => gameplayData.Insight,
            "abundance" => gameplayData.Abundance,
            "capacity" => gameplayData.Capacity,
            "radiance" => gameplayData.Radiance,
            _ => 0
        };

        setValue(Mathf.Max(0, currentValue + amount));
        UIManager.Instance?.InformationCanvas?.RefreshStatsText();
        UIManager.Instance?.CombatCanvas?.LightCardSelectionPanel?.RefreshCostText();
    }

    [YarnCommand("change_health")]
    public static void ChangeHealth(int amount)
    {
        ApplyHealthDelta(amount);
    }

    [YarnCommand("change_health_percent")]
    public static void ChangeHealthPercent(float percent)
    {
        ApplyHealthDelta(Mathf.RoundToInt(GetMaxHealth() * percent / 100f));
    }

    [YarnCommand("change_light")]
    public static void ChangeLight(int amount)
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData == null)
        {
            Debug.LogWarning("Cannot change Yarn light because PersistentGameplayData is missing.");
            return;
        }

        gameplayData.ChangeLight(amount);
    }

    [YarnCommand("change_light_percent")]
    public static void ChangeLightPercent(float percent)
    {
        ChangeLight(Mathf.RoundToInt(100f * percent / 100f));
    }

    [YarnCommand("set_light_loss")]
    public static void SetLightLoss(int value)
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData == null)
        {
            Debug.LogWarning("Cannot set light loss because PersistentGameplayData is missing.");
            return;
        }

        gameplayData.LightLoss = Mathf.Max(0, value);
    }

    [YarnCommand("load_map_scene")]
    public static void LoadMapScene()
    {
        var gameManager = GameManager.Instance;
        var uiManager = UIManager.Instance;
        if (gameManager == null || uiManager == null)
        {
            Debug.LogError("Cannot load the map because GameManager or UIManager is missing.");
            return;
        }

        gameManager.ApplyInitialCardRemovals();
        if (DialogueAudioManager.instance != null)
        {
            DialogueAudioManager.instance.DynamicMusic("map");
        }
        else
        {
            Debug.LogWarning("Cannot update map music because DialogueAudioManager.instance is missing.", gameManager);
        }

        uiManager.SetCanvas(uiManager.CombatCanvas, false, true);
        uiManager.SetCanvas(uiManager.InformationCanvas, true, false);
        uiManager.SetCanvas(uiManager.RewardCanvas, false, true);
        uiManager.ChangeScene(gameManager.SceneData.mapSceneIndex);
    }

    [YarnCommand("change_max_health")]
    public static void ChangeMaxHealth(int amount)
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData == null || gameplayData.AllyList == null || gameplayData.AllyList.Count == 0)
        {
            Debug.LogWarning("Cannot change Yarn max health because player data is missing.");
            return;
        }

        var allyData = gameplayData.AllyList[0].AllyCharacterData;
        allyData.MaxHealth = Mathf.Max(1, allyData.MaxHealth + amount);
        gameplayData.Vigor = Mathf.Max(1, gameplayData.Vigor + amount);

        var currentAlly = CombatManager.Instance?.CurrentMainAlly;
        if (currentAlly != null)
        {
            currentAlly.CharacterStats.MaxHealth = Mathf.Max(1, currentAlly.CharacterStats.MaxHealth + amount);
            currentAlly.CharacterStats.CurrentHealth = Mathf.Clamp(
                currentAlly.CharacterStats.CurrentHealth + Mathf.Max(0, amount),
                1,
                currentAlly.CharacterStats.MaxHealth);

            gameplayData.SetAllyHealthData(
                allyData.CharacterID,
                currentAlly.CharacterStats.CurrentHealth,
                currentAlly.CharacterStats.MaxHealth);

            currentAlly.CharacterStats.OnHealthChanged?.Invoke(
                currentAlly.CharacterStats.CurrentHealth,
                currentAlly.CharacterStats.MaxHealth);
        }

        UIManager.Instance?.InformationCanvas?.SetHealthText(
            currentAlly != null ? currentAlly.CharacterStats.CurrentHealth : allyData.MaxHealth,
            currentAlly != null ? currentAlly.CharacterStats.MaxHealth : allyData.MaxHealth);
    }

    private static void ApplyHealthDelta(int amount)
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData == null || gameplayData.AllyList == null || gameplayData.AllyList.Count == 0)
        {
            Debug.LogWarning("Cannot change Yarn health because player data is missing.");
            return;
        }

        var allyData = gameplayData.AllyList[0].AllyCharacterData;
        var currentAlly = CombatManager.Instance?.CurrentMainAlly;
        int maxHealth = currentAlly != null
            ? currentAlly.CharacterStats.MaxHealth
            : Mathf.Max(1, gameplayData.Vigor);
        int currentHealth = currentAlly != null
            ? currentAlly.CharacterStats.CurrentHealth
            : GetPersistentCurrentHealth(gameplayData, allyData.CharacterID, maxHealth);
        int newHealth = Mathf.Clamp(currentHealth + amount, 1, Mathf.Max(1, maxHealth));

        if (currentAlly != null)
        {
            currentAlly.CharacterStats.SetCurrentHealth(newHealth);
        }

        gameplayData.SetAllyHealthData(allyData.CharacterID, newHealth, maxHealth);
        UIManager.Instance?.InformationCanvas?.SetHealthText(newHealth, maxHealth);
    }

    private static int GetPersistentCurrentHealth(
        NueGames.NueDeck.Scripts.Data.Settings.PersistentGameplayData gameplayData,
        string characterId,
        int fallbackMaxHealth)
    {
        var healthData = gameplayData.AllyHealthDataList?.Find(data => data.CharacterId == characterId);
        return healthData != null ? healthData.CurrentHealth : fallbackMaxHealth;
    }

    [YarnFunction("gold")]
    public static int GetGold()
    {
        return GameManager.Instance?.PersistentGameplayData?.CurrentGold ?? 0;
    }

    [YarnFunction("proficiency")]
    public static int GetProficiency()
    {
        return GameManager.Instance?.PersistentGameplayData?.Proficiency ?? 0;
    }

    [YarnFunction("max_health")]
    public static int GetMaxHealth()
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData?.AllyList == null || gameplayData.AllyList.Count == 0)
        {
            return 0;
        }

        var currentAlly = CombatManager.Instance?.CurrentMainAlly;
        return currentAlly != null
            ? currentAlly.CharacterStats.MaxHealth
            : Mathf.Max(1, gameplayData.Vigor);
    }

    [YarnFunction("health")]
    public static int GetHealth()
    {
        var currentAlly = CombatManager.Instance?.CurrentMainAlly;
        if (currentAlly != null)
            return currentAlly.CharacterStats.CurrentHealth;

        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        var allyData = gameplayData?.AllyList != null && gameplayData.AllyList.Count > 0
            ? gameplayData.AllyList[0].AllyCharacterData
            : null;
        return gameplayData != null && allyData != null
            ? GetPersistentCurrentHealth(gameplayData, allyData.CharacterID, Mathf.Max(1, gameplayData.Vigor))
            : 0;
    }

    [YarnFunction("is_full_health")]
    public static bool IsFullHealth()
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData?.AllyList == null || gameplayData.AllyList.Count == 0)
            return false;

        var allyData = gameplayData.AllyList[0].AllyCharacterData;
        var currentAlly = CombatManager.Instance?.CurrentMainAlly;
        int maxHealth = currentAlly != null
            ? currentAlly.CharacterStats.MaxHealth
            : Mathf.Max(1, gameplayData.Vigor);
        int currentHealth = currentAlly != null
            ? currentAlly.CharacterStats.CurrentHealth
            : GetPersistentCurrentHealth(gameplayData, allyData.CharacterID, maxHealth);

        return maxHealth > 0 && currentHealth >= maxHealth;
    }

    [YarnFunction("light")]
    public static int GetLight()
    {
        return GameManager.Instance?.PersistentGameplayData?.Light ?? 0;
    }

    [YarnFunction("insight")]
    public static int GetInsight()
    {
        return GameManager.Instance?.PersistentGameplayData?.Insight ?? 0;
    }

    [YarnFunction("can_afford_insight")]
    public static bool CanAffordInsight(int amount)
    {
        return amount >= 0 && GetInsight() >= amount;
    }

    [YarnCommand("picture")]
    public void SetPicture(string id)
    {
        if (screenImage == null)
        {
            Debug.LogWarning("Cannot set a Yarn picture because Screen Image is missing.", this);
            return;
        }

        foreach (var picture in pictures)
        {
            if (!string.Equals(picture.id, id, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            screenImage.sprite = picture.sprite;
            screenImage.color = Color.white;
            screenImage.enabled = picture.sprite != null;
            return;
        }

        Debug.LogWarning($"No Yarn picture named '{id}' was found.", this);
    }

    [YarnCommand("clear_picture")]
    public void ClearPicture()
    {
        if (screenImage == null)
        {
            return;
        }

        screenImage.sprite = null;
        screenImage.enabled = false;
    }

    [YarnCommand("picture_color")]
    public void SetPictureColor(string colorName)
    {
        if (screenImage == null)
        {
            Debug.LogWarning("Cannot tint the Yarn picture because Screen Image is missing.", this);
            return;
        }

        if (!TryGetEffectColor(colorName, out var color))
        {
            Debug.LogWarning($"Unknown Yarn picture color '{colorName}'. Use white, red, black, or a hex color.", this);
            return;
        }

        screenImage.color = color;
        screenImage.enabled = screenImage.sprite != null;
    }

    [YarnCommand("background")]
    public void PlayBackground(string state)
    {
        if (backgroundAnimator == null)
        {
            Debug.LogWarning("Cannot play a Yarn background because Background Animator is missing.", this);
            return;
        }

        backgroundAnimator.Play(state);
    }

    private async YarnTask FadeToAsync(Color color, float targetAlpha, float duration)
    {
        if (screenEffectGroup == null || screenEffectImage == null)
        {
            Debug.LogWarning("Yarn screen effect references are not assigned.", this);
            return;
        }

        screenEffectGroup.gameObject.SetActive(true);
        screenEffectImage.color = color;

        float startAlpha = screenEffectGroup.alpha;
        float elapsed = 0f;
        float safeDuration = Mathf.Max(0f, duration);

        while (elapsed < safeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            screenEffectGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / safeDuration);
            await YarnTask.Yield();
        }

        screenEffectGroup.alpha = targetAlpha;
    }

    private async YarnTask OpenConfiguredCardReward(CardRewardData cardRewardData, string rewardName)
    {
        var rewardCanvas = UIManager.Instance?.RewardCanvas;
        if (rewardCanvas == null)
        {
            Debug.LogWarning($"Cannot open the {rewardName} card reward because RewardCanvas is missing.", this);
            return;
        }

        if (cardRewardData == null)
        {
            Debug.LogWarning($"Cannot open the {rewardName} card reward because its CardRewardData is not assigned.", this);
            return;
        }

        var completionSource = new YarnTaskCompletionSource<bool>();
        if (!rewardCanvas.OpenCardReward(
                cardRewardData,
                () => completionSource.TrySetResult(true)))
        {
            return;
        }

        await completionSource.Task;
    }

    private static async YarnTask WaitForCardRemoval(YarnTaskCompletionSource<bool> completionSource)
    {
        await completionSource.Task;
    }

    private static async YarnTask WaitForCardDuplication(YarnTaskCompletionSource<bool> completionSource)
    {
        await completionSource.Task;
    }

    private async YarnTask FadeColorToAsync(Color targetColor, float duration)
    {
        float safeDuration = Mathf.Max(0f, duration);
        Color startColor = screenEffectImage.color;
        float elapsed = 0f;

        while (elapsed < safeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            screenEffectImage.color = Color.Lerp(startColor, targetColor, elapsed / safeDuration);
            await YarnTask.Yield();
        }

        screenEffectImage.color = targetColor;
        screenEffectGroup.alpha = 1f;
    }

    private async YarnTask BloomTextAsync(string message)
    {
        if (bloomTextPanel == null || bloomText == null)
        {
            Debug.LogWarning("Cannot show Yarn bloom text because the panel or text reference is missing.", this);
            return;
        }

        var panelTransform = bloomTextPanel.transform as RectTransform;
        if (panelTransform == null)
        {
            Debug.LogWarning("Cannot show Yarn bloom text because the panel is not a RectTransform.", this);
            return;
        }

        bloomText.text = message ?? string.Empty;
        bloomTextPanel.gameObject.SetActive(true);
        bloomTextPanel.alpha = 0f;
        panelTransform.localScale = Vector3.one * bloomStartScale;

        await AnimateBloomTextAsync(panelTransform, 0f, 1f, bloomStartScale, bloomPeakScale, bloomFadeInDuration);

        if (bloomHoldDuration > 0f)
            await YarnTask.Delay(TimeSpan.FromSeconds(bloomHoldDuration));

        await AnimateBloomTextAsync(panelTransform, 1f, 0f, bloomPeakScale, 1f, bloomFadeOutDuration);

        bloomTextPanel.gameObject.SetActive(false);
    }

    private async YarnTask AnimateBloomTextAsync(
        RectTransform panelTransform,
        float startAlpha,
        float targetAlpha,
        float startScale,
        float targetScale,
        float duration)
    {
        float safeDuration = Mathf.Max(0f, duration);
        if (safeDuration <= 0f)
        {
            bloomTextPanel.alpha = targetAlpha;
            panelTransform.localScale = Vector3.one * targetScale;
            return;
        }

        float elapsed = 0f;
        while (elapsed < safeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / safeDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            bloomTextPanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, easedProgress);
            float scale = Mathf.Lerp(startScale, targetScale, easedProgress);
            panelTransform.localScale = Vector3.one * scale;
            await YarnTask.Yield();
        }

        bloomTextPanel.alpha = targetAlpha;
        panelTransform.localScale = Vector3.one * targetScale;
    }

    private static bool TryGetEffectColor(string colorName, out Color color)
    {
        switch (colorName.ToLowerInvariant())
        {
            case "white":
                color = Color.white;
                return true;
            case "red":
                color = Color.red;
                return true;
            case "black":
                color = Color.black;
                return true;
            default:
                return ColorUtility.TryParseHtmlString(colorName, out color);
        }
    }
}