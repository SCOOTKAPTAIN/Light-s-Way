using System.Collections;
using NueGames.NueDeck.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NueGames.NueDeck.Scripts.Utils
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private float duration = 2;
        [SerializeField] private AnimationCurve scaleCurve;
        [SerializeField] private AnimationCurve yForceCurve;
        [SerializeField] private AnimationCurve xForceCurve;
        [SerializeField] private TextMeshProUGUI textField;

        private Image _statusIcon;
        
        public void PlayText(string text,int xDir,int yDir = 1)
        {
            textField.text = text;
            StartCoroutine(TextRoutine(xDir,yDir));
        }

        public void SetTextColor(Color color)
        {
            if (textField != null)
                textField.color = color;
        }

        public void AttachStatusIcon(StatusIconBase statusIconPrefab, Sprite iconSprite, float iconScale = 0.3f)
        {
            if (statusIconPrefab == null || iconSprite == null)
                return;

            var statusIcon = Instantiate(statusIconPrefab, transform);
            var iconTransform = statusIcon.GetComponent<RectTransform>();
            if (iconTransform != null)
            {
                iconTransform.anchorMin = new Vector2(0.5f, 0.5f);
                iconTransform.anchorMax = new Vector2(0.5f, 0.5f);
                iconTransform.anchoredPosition = new Vector2(-0.45f, 0f);
                iconTransform.sizeDelta = new Vector2(1f, 1f);
                iconTransform.localScale = Vector3.one * iconScale;
            }

            statusIcon.StatusImage.sprite = iconSprite;
            statusIcon.StatusValueText.gameObject.SetActive(false);
            _statusIcon = statusIcon.StatusImage;
        }

        private IEnumerator TextRoutine(int xDir, int yDir)
        {
            var waitFrame = new WaitForEndOfFrame();
            var timer = 0f;

            var initalScale = transform.localScale;
            // How long (normalized 0-1) before fade begins. 0 = fade immediately, 1 = never fade
            var fadeStart = 0.5f;
            while (timer<=duration)
            {
                timer += Time.deltaTime;
                var t = Mathf.Clamp01(timer / duration);

                // Scale (pop) behavior driven by animation curve
                transform.localScale = scaleCurve.Evaluate(t)*initalScale;
                var pos =transform.position;
                pos.x += xForceCurve.Evaluate(t)*xDir*Time.deltaTime;
                pos.y += yForceCurve.Evaluate(t)*yDir*Time.deltaTime;
                transform.position = pos;
                
                // Fade out alpha after fadeStart portion of the lifetime
                if (textField != null)
                {
                    var col = textField.color;
                    if (t <= fadeStart)
                    {
                        col.a = 1f;
                    }
                    else
                    {
                        var fadeT = Mathf.InverseLerp(fadeStart, 1f, t);
                        col.a = Mathf.Lerp(1f, 0f, fadeT);
                    }
                    textField.color = col;
                }

                if (_statusIcon != null)
                {
                    var iconColor = _statusIcon.color;
                    iconColor.a = textField != null ? textField.color.a : 1f;
                    _statusIcon.color = iconColor;
                }
                yield return waitFrame;
            }
            Destroy(gameObject);
        }
    }
}