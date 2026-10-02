using System;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public sealed class YarnGameplayCommands : MonoBehaviour
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

    private void Awake()
    {
        if (screenEffectGroup != null)
        {
            screenEffectGroup.alpha = 0f;
            screenEffectGroup.interactable = false;
            screenEffectGroup.blocksRaycasts = false;
        }
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

        var previousVigor = gameplayData.Vigor;
        ChangePersistentStat("vigor", amount, value => gameplayData.Vigor = value);
        ApplyVigorDelta(gameplayData.Vigor - previousVigor);
    }

    private static void ApplyVigorDelta(int delta)
    {
        var gameplayData = GameManager.Instance?.PersistentGameplayData;
        if (gameplayData == null)
            return;

        var combatManager = CombatManager.Instance;
        var ally = combatManager != null ? combatManager.CurrentMainAlly : null;
        if (ally != null && ally.CharacterStats != null)
        {
            if (delta > 0)
                ally.CharacterStats.IncreaseMaxHealth(delta);
            else if (delta < 0)
                ally.CharacterStats.ApplyPermanentMaxHealthReduction(-delta);

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
            healthData.MaxHealth += delta;
            healthData.CurrentHealth = Mathf.Clamp(healthData.CurrentHealth + delta, 1, healthData.MaxHealth);
            UIManager.Instance?.InformationCanvas?.SetHealthText(healthData.CurrentHealth, healthData.MaxHealth);
            return;
        }

        if (gameplayData.AllyList.Count > 0)
        {
            var health = gameplayData.AllyList[0].AllyCharacterData.MaxHealth + gameplayData.Vigor;
            UIManager.Instance?.InformationCanvas?.SetHealthText(health, health);
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
        int maxHealth = currentAlly != null ? currentAlly.CharacterStats.MaxHealth : allyData.MaxHealth;
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
            : gameplayData.AllyList[0].AllyCharacterData.MaxHealth;
    }

    [YarnFunction("health")]
    public static int GetHealth()
    {
        var currentAlly = CombatManager.Instance?.CurrentMainAlly;
        return currentAlly != null ? currentAlly.CharacterStats.CurrentHealth : 0;
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