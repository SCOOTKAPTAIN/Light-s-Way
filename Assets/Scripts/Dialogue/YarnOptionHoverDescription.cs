using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity;

public sealed class YarnOptionHoverDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CanvasGroup descriptionPanel;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private float fadeDuration = 0.15f;

    private OptionItem optionItem;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        optionItem = GetComponent<OptionItem>();

        if (descriptionPanel != null)
        {
            descriptionPanel.alpha = 0f;
            descriptionPanel.interactable = false;
            descriptionPanel.blocksRaycasts = false;
            descriptionPanel.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (optionItem == null || descriptionPanel == null || descriptionText == null)
            return;

        var description = GetDescription();
        if (string.IsNullOrWhiteSpace(description))
        {
            HideDescription();
            return;
        }

        descriptionText.text = description;
        descriptionPanel.gameObject.SetActive(true);
        StartFade(1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideDescription();
    }

    private string GetDescription()
    {
        foreach (var metadata in optionItem.Option.Line.Metadata)
        {
            const string prefix = "paneldetails=";
            if (metadata.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return metadata.Substring(prefix.Length).Replace('_', ' ');
        }

        return string.Empty;
    }

    private void HideDescription()
    {
        if (descriptionPanel == null)
            return;

        StartFade(0f);
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeDescription(targetAlpha));
    }

    private IEnumerator FadeDescription(float targetAlpha)
    {
        var startAlpha = descriptionPanel.alpha;
        var safeDuration = Mathf.Max(0f, fadeDuration);
        var elapsed = 0f;

        while (elapsed < safeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            descriptionPanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / safeDuration);
            yield return null;
        }

        descriptionPanel.alpha = targetAlpha;
        if (Mathf.Approximately(targetAlpha, 0f))
            descriptionPanel.gameObject.SetActive(false);

        fadeCoroutine = null;
    }
}
