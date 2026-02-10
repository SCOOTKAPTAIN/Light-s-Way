using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NueGames.NueDeck.Scripts.UI
{
    public class PlayerDeathAnimation : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private Image blackScreen;
        [SerializeField] private Image playerSilhouette;
        [SerializeField] private GameObject losePanel;
        
        [Header("Animation Parameters")]
        [SerializeField] private float delayBeforeDecay = 1f;
        [SerializeField] private float redToWhiteDecayDuration = 2f;
        [SerializeField] private float delayAfterDecay = 0.5f;
        [SerializeField] private float blackScreenDelay = 1f;
        [SerializeField] private float losePanelFadeDuration = 1f;
        
        [Header("Audio")]
        [SerializeField] private string deathSFX = "death";
        [SerializeField] private string decayCompleteSFX = "decaycomplete";
        [SerializeField] private string deathMusic = "gameover";
        
        private void Awake()
        {
            ResetAnimation();
        }
        
        private void ResetAnimation()
        {
            // Initialize all images to be invisible
            if (blackScreen != null)
            {
                blackScreen.color = new Color(0, 0, 0, 0);
                blackScreen.gameObject.SetActive(false);
            }
            
            if (playerSilhouette != null)
            {
                playerSilhouette.color = new Color(0.545f, 0.149f, 0.706f, 0);
                playerSilhouette.gameObject.SetActive(false);
            }
            
            if (losePanel != null)
            {
                var canvasGroup = losePanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = losePanel.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0;
                losePanel.SetActive(false);
            }
        }
        
        public void PlayDeathAnimation()
        {
            ResetAnimation();
            StartCoroutine(DeathSequence());
        }
        
        private IEnumerator DeathSequence()
        {
            // Stop all ongoing sounds immediately
            StopAllSounds();
            
            // Step 1: Play death SFX and immediately show dark screen with red silhouette
            PlayDeathSFX();
            yield return FadeToBlack();
            yield return ShowRedSilhouette();
            
            // Step 2: Hold purple silhouette for a moment
            yield return new WaitForSeconds(delayBeforeDecay);
            
            // Step 3: Red silhouette slowly decays to white
            yield return DecayRedToWhite();
            
            // Step 3: Wait a bit
            yield return new WaitForSeconds(delayAfterDecay);
            
            // Step 4: Play decay complete SFX
            PlayDecayCompleteSFX();
            
            // Step 5: Remove silhouette instantly
            yield return RemoveSilhouette();
            
            // Step 6: Keep black screen for a moment
            yield return new WaitForSeconds(blackScreenDelay);
            
            // Step 7: Slowly fade in game over panel
            yield return ShowLosePanel();
            
            // Step 8: Play death music
            PlayDeathMusic();
            
            // Step 9: Hide black screen so it doesn't persist
            if (blackScreen != null)
                blackScreen.gameObject.SetActive(false);
        }
        
        private void StopAllSounds()
        {
            // Stop AudioManager sounds (combat music and effects)
            if (NueGames.NueDeck.Scripts.Managers.AudioManager.Instance != null)
            {
                var audioManager = NueGames.NueDeck.Scripts.Managers.AudioManager.Instance;
                var musicSource = audioManager.GetType().GetField("musicSource", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(audioManager) as AudioSource;
                var sfxSource = audioManager.GetType().GetField("sfxSource", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(audioManager) as AudioSource;
                var buttonSource = audioManager.GetType().GetField("buttonSource", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(audioManager) as AudioSource;
                
                if (musicSource != null) musicSource.Stop();
                if (sfxSource != null) sfxSource.Stop();
                if (buttonSource != null) buttonSource.Stop();
            }
            
            // Stop DialogueAudioManager sounds (map/dialogue music and effects)
            if (DialogueAudioManager.instance != null)
            {
                if (DialogueAudioManager.instance.music_source != null)
                    DialogueAudioManager.instance.music_source.Stop();
                if (DialogueAudioManager.instance.sfx_source != null)
                    DialogueAudioManager.instance.sfx_source.Stop();
            }
        }
        
        private IEnumerator FadeToBlack()
        {
            if (blackScreen == null) yield break;
            
            // Ensure the GameObject is active and image is enabled
            blackScreen.gameObject.SetActive(true);
            blackScreen.enabled = true;
            
            // Instant fade to black
            blackScreen.color = new Color(0, 0, 0, 1);
            yield break;
        }
        
        private IEnumerator ShowRedSilhouette()
        {
            if (playerSilhouette == null) yield break;
            
            // Ensure the GameObject is active and image is enabled
            playerSilhouette.gameObject.SetActive(true);
            playerSilhouette.enabled = true;
            
            // Instant purple silhouette
            playerSilhouette.color = new Color(0.545f, 0.149f, 0.706f, 1);
            yield break;
        }
        
        private IEnumerator DecayRedToWhite()
        {
            if (playerSilhouette == null) yield break;
            
            float elapsed = 0f;
            Color startColor = new Color(0.545f, 0.149f, 0.706f, 1); // Purple
            Color endColor = new Color(1, 1, 1, 1);   // White
            
            while (elapsed < redToWhiteDecayDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / redToWhiteDecayDuration;
                playerSilhouette.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }
            
            playerSilhouette.color = endColor;
        }
        
        private void PlayDeathSFX()
        {
            if (DialogueAudioManager.instance != null)
            {
                DialogueAudioManager.instance.PlaySFX(deathSFX);
            }
        }
        
        private void PlayDecayCompleteSFX()
        {
            if (DialogueAudioManager.instance != null)
            {
                DialogueAudioManager.instance.PlaySFX(decayCompleteSFX);
            }
        }
        
        private void PlayDeathMusic()
        {
            if (DialogueAudioManager.instance != null)
            {
                DialogueAudioManager.instance.PlayMusic(deathMusic);
            }
        }
        
        private IEnumerator RemoveSilhouette()
        {
            if (playerSilhouette == null) yield break;
            
            // Instant removal
            playerSilhouette.color = new Color(1, 1, 1, 0);
            playerSilhouette.gameObject.SetActive(false);
            yield break;
        }
        
        private IEnumerator ShowLosePanel()
        {
            if (losePanel == null) yield break;
            
            losePanel.SetActive(true);
            CanvasGroup canvasGroup = losePanel.GetComponent<CanvasGroup>();
            
            if (canvasGroup == null) yield break;
            
            float elapsed = 0f;
            
            while (elapsed < losePanelFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / losePanelFadeDuration;
                canvasGroup.alpha = t;
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
    }
}
