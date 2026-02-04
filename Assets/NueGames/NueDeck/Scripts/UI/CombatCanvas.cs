using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NueGames.NueDeck.Scripts.UI
{
    public class CombatCanvas : CanvasBase
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI drawPileTextField;
        [SerializeField] private TextMeshProUGUI discardPileTextField;
        [SerializeField] private TextMeshProUGUI exhaustPileTextField;
        [SerializeField] private TextMeshProUGUI manaTextTextField;
        
        [Header("Panels")]
        [SerializeField] private GameObject combatWinPanel;
        [SerializeField] private GameObject combatLosePanel;
        [SerializeField] private LightCardSelectionPanel lightCardSelectionPanel;
        
        [Header("Light Card Button")]
        [SerializeField] private Button lightCardButton;
        [SerializeField] private Image lightCardButtonImage;
        [SerializeField] private Color glowColor = Color.yellow;
        [SerializeField] private Color normalColor = Color.white;

        public TextMeshProUGUI DrawPileTextField => drawPileTextField;
        public TextMeshProUGUI DiscardPileTextField => discardPileTextField;
        public TextMeshProUGUI ManaTextTextField => manaTextTextField;
        public GameObject CombatWinPanel => combatWinPanel;
        public GameObject CombatLosePanel => combatLosePanel;

        public TextMeshProUGUI ExhaustPileTextField => exhaustPileTextField;
        public LightCardSelectionPanel LightCardSelectionPanel => lightCardSelectionPanel;

        #region Setup
        private void Awake()
        {
            CombatWinPanel.SetActive(false);
            CombatLosePanel.SetActive(false);
        }
        #endregion

        #region Public Methods
        public void SetPileTexts()
        {
            DrawPileTextField.text = $"{CollectionManager.DrawPile.Count.ToString()}";
            DiscardPileTextField.text = $"{CollectionManager.DiscardPile.Count.ToString()}";
            ExhaustPileTextField.text =  $"{CollectionManager.ExhaustPile.Count.ToString()}";
            ManaTextTextField.text = $"{GameManager.PersistentGameplayData.CurrentMana.ToString()}/{GameManager.PersistentGameplayData.MaxMana}";
            
            // Update button glow based on Light
            UpdateLightCardButtonGlow();
        }
        
        public void UpdateLightCardButtonGlow()
        {
            if (lightCardButtonImage != null && GameManager != null && GameManager.PersistentGameplayData != null)
            {
                bool hasEnoughLight = GameManager.PersistentGameplayData.light >= 10;
                lightCardButtonImage.color = hasEnoughLight ? glowColor : normalColor;
            }
        }

        public override void ResetCanvas()
        {
            base.ResetCanvas();
            CombatWinPanel.SetActive(false);
            CombatLosePanel.SetActive(false);
        }

        public void EndTurn()
        {
            if (CombatManager.CurrentCombatStateType == CombatStateType.AllyTurn)
                CombatManager.EndTurn();
        }
        
        public void OpenLightCardSelection()
        {
            if (GameManager == null || GameManager.PersistentGameplayData == null) return;
            
            // Check if player has enough Light
            if (GameManager.PersistentGameplayData.light < 10)
            {
                // Play "not enough Light" effects on the ally character
                if (CombatManager != null && CombatManager.CurrentMainAlly != null)
                {
                    if (FxManager.Instance != null)
                        FxManager.Instance.PlayFx(CombatManager.CurrentMainAlly.transform, FxType.NoLight);
                }
                
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayOneShot(AudioActionType.NoLight);
                
                return; // Don't open panel
            }
            
            // Has enough Light - open the panel
            if (lightCardSelectionPanel != null)
            {
                lightCardSelectionPanel.OpenCanvas();
            }
        }
        #endregion
    }
}