using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace NueGames.NueDeck.Scripts.UI
{
    public class SacrificeKnowledgePanel : CanvasBase
    {
        protected override bool BlocksBackgroundInput => true;

        [Header("Options")]
        [SerializeField] private Button lightButton;
        [SerializeField] private Button proficiencyButton;
        [SerializeField] private Button maxManaButton;
        [SerializeField] private Button maxHealthButton;
        [SerializeField] private Button closeButton;

        private EnemyBase _targetEnemy;
        private bool _previousCanSelectCards;

        public bool IsOpen => gameObject.activeInHierarchy;

        private void Awake()
        {
            lightButton?.onClick.AddListener(SacrificeLight);
            proficiencyButton?.onClick.AddListener(SacrificeProficiency);
            maxManaButton?.onClick.AddListener(SacrificeMaxMana);
            maxHealthButton?.onClick.AddListener(SacrificeMaxHealth);
            closeButton?.onClick.AddListener(CloseCanvas);
        }

        public void OpenForTarget(EnemyBase targetEnemy)
        {
            if (targetEnemy == null || targetEnemy.CharacterStats == null ||
                targetEnemy.CharacterStats.StatusDict[StatusType.TotalAssimilation].IsActive)
                return;

            _targetEnemy = targetEnemy;
            _previousCanSelectCards = GameManager != null && GameManager.PersistentGameplayData != null &&
                                      GameManager.PersistentGameplayData.CanSelectCards;
            base.OpenCanvas();

            if (GameManager != null && GameManager.PersistentGameplayData != null)
            {
                if (lightButton != null)
                    lightButton.interactable = GameManager.PersistentGameplayData.light >= 20;
                GameManager.PersistentGameplayData.CanSelectCards = false;
            }
            else if (lightButton != null)
            {
                lightButton.interactable = false;
            }
        }

        public override void CloseCanvas()
        {
            _targetEnemy = null;

            if (CombatManager != null && CombatManager.CurrentCombatStateType == CombatStateType.AllyTurn)
            {
                if (CollectionManager != null && CollectionManager.HandController != null)
                    CollectionManager.HandController.EnableDragging();

                if (GameManager != null && GameManager.PersistentGameplayData != null)
                    GameManager.PersistentGameplayData.CanSelectCards = true;
            }
            else if (GameManager != null && GameManager.PersistentGameplayData != null)
            {
                GameManager.PersistentGameplayData.CanSelectCards = _previousCanSelectCards;
            }

            base.CloseCanvas();
        }

        public void SacrificeLight()
        {
            if (GameManager == null || GameManager.PersistentGameplayData == null ||
                GameManager.PersistentGameplayData.light < 20)
                return;

            CompleteSacrifice(() => GameManager.PersistentGameplayData.ChangeLight(-20));
        }

        public void SacrificeProficiency()
        {
            CompleteSacrifice(() => CombatManager.CurrentMainAlly.CharacterStats.ApplyPermanentProficiencyReduction(3));
        }

        public void SacrificeMaxMana()
        {
            CompleteSacrifice(() => CombatManager.CurrentMainAlly.CharacterStats.ApplyPermanentMaxManaReduction(1));
        }

        public void SacrificeMaxHealth()
        {
            CompleteSacrifice(() => CombatManager.CurrentMainAlly.CharacterStats.ApplyPermanentMaxHealthReduction(25));
        }

        private void CompleteSacrifice(System.Action sacrifice)
        {
            if (_targetEnemy == null || CombatManager == null || CombatManager.CurrentMainAlly == null ||
                GameManager == null || GameManager.PersistentGameplayData == null)
                return;

            if (_targetEnemy.CharacterStats.StatusDict[StatusType.TotalAssimilation].IsActive)
            {
                CloseCanvas();
                return;
            }

            sacrifice?.Invoke();
            _targetEnemy.CharacterStats.ResetAssimilationForSacrifice();
            CloseCanvas();
        }

        private void OnDestroy()
        {
            lightButton?.onClick.RemoveListener(SacrificeLight);
            proficiencyButton?.onClick.RemoveListener(SacrificeProficiency);
            maxManaButton?.onClick.RemoveListener(SacrificeMaxMana);
            maxHealthButton?.onClick.RemoveListener(SacrificeMaxHealth);
            closeButton?.onClick.RemoveListener(CloseCanvas);
        }
    }
}
