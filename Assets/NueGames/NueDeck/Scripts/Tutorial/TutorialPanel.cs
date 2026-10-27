using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NueGames.NueDeck.Scripts.Tutorial
{
    public sealed class TutorialPanel : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Image pageImage;
        [SerializeField] private TMP_Text pageNumberText;

        [Header("Controls")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button finishButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private CanvasGroup canvasGroup;

        private TutorialDefinition definition;
        private Action<TutorialDefinition, bool> closeCallback;
        private int pageIndex;
        private float previousTimeScale = 1f;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            AddListener(backButton, ShowPreviousPage);
            AddListener(nextButton, ShowNextPage);
            AddListener(finishButton, Finish);
            AddListener(closeButton, Close);
            gameObject.SetActive(false);
        }

        public void Open(TutorialDefinition tutorial, Action<TutorialDefinition, bool> onClosed)
        {
            if (tutorial == null || tutorial.Pages == null || tutorial.Pages.Count == 0)
            {
                Debug.LogError("Cannot open a tutorial panel without tutorial pages.", this);
                return;
            }

            definition = tutorial;
            closeCallback = onClosed;
            pageIndex = 0;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            ShowPage();
        }

        public void ShowNextPage()
        {
            if (definition == null || pageIndex >= definition.Pages.Count - 1)
                return;

            pageIndex++;
            ShowPage();
        }

        public void ShowPreviousPage()
        {
            if (definition == null || pageIndex <= 0)
                return;

            pageIndex--;
            ShowPage();
        }

        public void Finish()
        {
            CloseInternal(true);
        }

        public void Close()
        {
            CloseInternal(false);
        }

        private void ShowPage()
        {
            var page = definition.Pages[pageIndex];
            SetText(titleText, page.title);
            SetText(bodyText, page.body);

            if (pageImage != null)
            {
                pageImage.sprite = page.image;
                pageImage.enabled = page.image != null;
            }

            if (pageNumberText != null)
                pageNumberText.text = $"{pageIndex + 1}/{definition.Pages.Count}";

            SetButtonActive(backButton, pageIndex > 0);
            SetButtonActive(nextButton, pageIndex < definition.Pages.Count - 1);
            SetButtonActive(finishButton, pageIndex == definition.Pages.Count - 1);
        }

        private void CloseInternal(bool completed)
        {
            if (definition == null)
                return;

            var closedDefinition = definition;
            var callback = closeCallback;
            definition = null;
            closeCallback = null;

            Time.timeScale = previousTimeScale;
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
            callback?.Invoke(closedDefinition, completed);
        }

        private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
                button.onClick.AddListener(action);
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
                text.text = value ?? string.Empty;
        }

        private static void SetButtonActive(Button button, bool active)
        {
            if (button != null)
                button.gameObject.SetActive(active);
        }
    }
}
