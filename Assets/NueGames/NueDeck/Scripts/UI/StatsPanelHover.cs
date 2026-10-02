using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NueGames.NueDeck.Scripts.UI
{
    public class StatsPanelHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject statsPanel;
        [SerializeField] private float fadeDuration = 0.15f;
        [SerializeField] private float exitCheckDelay = 0.05f;

        private CanvasGroup statsPanelCanvasGroup;
        private Coroutine fadeRoutine;
        private Coroutine exitCheckRoutine;

        private void Awake()
        {
            if (statsPanel == null)
                return;

            statsPanelCanvasGroup = statsPanel.GetComponent<CanvasGroup>();
            if (statsPanelCanvasGroup == null)
                statsPanelCanvasGroup = statsPanel.AddComponent<CanvasGroup>();

            statsPanelCanvasGroup.interactable = false;
            statsPanelCanvasGroup.blocksRaycasts = false;
            statsPanelCanvasGroup.alpha = 0f;
            statsPanel.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (exitCheckRoutine != null)
            {
                StopCoroutine(exitCheckRoutine);
                exitCheckRoutine = null;
            }

            ShowPanel();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (exitCheckRoutine != null)
                StopCoroutine(exitCheckRoutine);

            exitCheckRoutine = StartCoroutine(CheckDelayedExit());
        }

        private IEnumerator CheckDelayedExit()
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, exitCheckDelay));

            if (transform is RectTransform triggerRect)
            {
                var canvas = GetComponentInParent<Canvas>();
                var eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? canvas.worldCamera
                    : null;

                if (!RectTransformUtility.RectangleContainsScreenPoint(triggerRect, Input.mousePosition, eventCamera))
                    HidePanel();
            }

            exitCheckRoutine = null;
        }

        private void ShowPanel()
        {
            if (statsPanel == null || statsPanelCanvasGroup == null)
                return;

            statsPanel.SetActive(true);
            StartFade(1f);
        }

        private void HidePanel()
        {
            if (statsPanel == null || statsPanelCanvasGroup == null)
                return;

            StartFade(0f);
        }

        private void StartFade(float targetAlpha)
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
        }

        private IEnumerator FadeRoutine(float targetAlpha)
        {
            float startAlpha = statsPanelCanvasGroup.alpha;
            float duration = Mathf.Max(0f, fadeDuration);

            if (duration <= 0f)
            {
                statsPanelCanvasGroup.alpha = targetAlpha;
            }
            else
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    statsPanelCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                    yield return null;
                }

                statsPanelCanvasGroup.alpha = targetAlpha;
            }

            if (Mathf.Approximately(targetAlpha, 0f))
                statsPanel.SetActive(false);

            fadeRoutine = null;
        }
    }
}
