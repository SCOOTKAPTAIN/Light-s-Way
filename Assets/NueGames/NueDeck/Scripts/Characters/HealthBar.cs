using UnityEngine;
using UnityEngine.UI;

namespace NueGames.NueDeck.Scripts.Characters
{
    [RequireComponent(typeof(Image))]
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        private Image _healthImage;

        private void Awake()
        {
            _healthImage = fillImage != null ? fillImage : GetComponent<Image>();
            _healthImage.type = Image.Type.Filled;
            _healthImage.fillMethod = Image.FillMethod.Horizontal;
            _healthImage.fillOrigin = 0;
            _healthImage.fillAmount = 1f;
        }

        private void OnValidate()
        {
            if (fillImage == null)
                fillImage = GetComponent<Image>();

            if (fillImage == null)
                return;

            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
        }

        public void SetHealth(int currentHealth, int maxHealth)
        {
            if (_healthImage == null)
                _healthImage = fillImage != null ? fillImage : GetComponent<Image>();

            _healthImage.fillAmount = maxHealth > 0
                ? Mathf.Clamp01(currentHealth / (float)maxHealth)
                : 0f;
        }
    }
}