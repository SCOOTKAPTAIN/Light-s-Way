using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Card;
using NueGames.NueDeck.Scripts.Data.Collection;
using NueGames.NueDeck.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NueGames.NueDeck.Scripts.UI
{
    public class InventoryCanvas : CanvasBase
    {
        [SerializeField] private TextMeshProUGUI titleTextField;
        [SerializeField] private LayoutGroup cardSpawnRoot;
        [SerializeField] private CardBase cardUIPrefab;

        public TextMeshProUGUI TitleTextField => titleTextField;
        public LayoutGroup CardSpawnRoot => cardSpawnRoot;

        private List<CardBase> _spawnedCardList = new List<CardBase>();

        public void ChangeTitle(string newTitle) => TitleTextField.text = newTitle;

        public void SetCards(List<CardData> cardDataList)
        {
            var count = 0;
            for (int i = 0; i < _spawnedCardList.Count; i++)
            {
                count++;
                if (i >= cardDataList.Count)
                {
                    _spawnedCardList[i].gameObject.SetActive(false);
                }
                else
                {
                    _spawnedCardList[i].SetCard(cardDataList[i], false);
                    _spawnedCardList[i].gameObject.SetActive(true);
                }

            }

            var cal = cardDataList.Count - count;
            if (cal > 0)
            {
                for (var i = 0; i < cal; i++)
                {
                    var cardData = cardDataList[count + i];
                    var cardBase = Instantiate(cardUIPrefab, CardSpawnRoot.transform);
                    cardBase.SetCard(cardData, false);
                    _spawnedCardList.Add(cardBase);
                }
            }

            ResetScrollToTop();


        }

        public override void OpenCanvas()
        {
            base.OpenCanvas();
            if (CollectionManager)
                CollectionManager.HandController.DisableDragging();
        }

        public override void CloseCanvas()
        {
            base.CloseCanvas();
            if (CollectionManager)
                CollectionManager.HandController.EnableDragging();

            // 🔑 Cleanup removal listeners so cards go back to neutral
            foreach (var card in _spawnedCardList)
            {
                var button = card.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    Destroy(button); // safe, removal adds fresh buttons again
                }

                var img = card.GetComponent<Image>();
                if (img != null)
                    img.raycastTarget = false;
            }
        }


        public override void ResetCanvas()
        {
            base.ResetCanvas();
        }







        public void SetCardsForRemoval(List<CardData> cardDataList, System.Action<CardData> onCardChosen)
        {
            // Clear existing UI
            foreach (var card in _spawnedCardList)
                Destroy(card.gameObject);
            _spawnedCardList.Clear();

            foreach (var cardData in cardDataList)
            {
                var cardBase = Instantiate(cardUIPrefab, CardSpawnRoot.transform);
                cardBase.SetCard(cardData, false);

                // Ensure there's an Image + Button to catch clicks
                var img = cardBase.GetComponent<Image>();
                if (img == null)
                    img = cardBase.gameObject.AddComponent<Image>(); // needed for button target
                img.raycastTarget = true;

                var button = cardBase.GetComponent<Button>();
                if (button == null)
                    button = cardBase.gameObject.AddComponent<Button>();

                button.transition = Selectable.Transition.None;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onCardChosen?.Invoke(cardData));

                _spawnedCardList.Add(cardBase);
            }
            ResetScrollToTop();
        }
        
        
        public void ResetScrollToTop()
        {
            var sr = CardSpawnRoot.GetComponentInParent<ScrollRect>();
            if (sr != null)
            {
                Canvas.ForceUpdateCanvases();        // force layout update
                sr.verticalNormalizedPosition = 1f;  // snap to top
            }
        }


    }
}
