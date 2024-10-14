using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private static AudioSource sharedAudioSource;

    private void Awake()
    {
        // Find the AudioSource only once for all buttons
        if (sharedAudioSource == null)
        {
            GameObject audioManager = GameObject.Find("UISound");
            if (audioManager != null)
            {
                sharedAudioSource = audioManager.GetComponent<AudioSource>();
            }

            if (sharedAudioSource == null)
            {
                Debug.LogWarning("UI Audio Manager with AudioSource not found in the scene.");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySound(hoverSound);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlaySound(clickSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (sharedAudioSource != null && clip != null)
        {
            sharedAudioSource.PlayOneShot(clip);
        }
    }
}
