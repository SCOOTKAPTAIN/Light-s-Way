using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    public GameObject menuUI;    // The main menu UI to show after the animation
    public Image screenImage;    // The image to zoom and pan on
    private bool isAnimationSkipped = false;

    void Start()
    {
        // Initially hide the menu UI
        menuUI.SetActive(false);
        
        // Start the animation
        StartCoroutine(PlayIntroAnimation());
    }

    IEnumerator PlayIntroAnimation()
    {
        // Example of an animation loop: zoom in and pan on the image
        float duration = 5.0f;  // Adjust as necessary
        float elapsedTime = 0f;

        Vector3 initialScale = screenImage.transform.localScale;
        Vector3 targetScale = new Vector3(1.5f, 1.5f, 1.5f);  // Example zoom
        Vector3 initialPosition = screenImage.transform.localPosition;
        Vector3 targetPosition = new Vector3(0, 50, 0);  // Example panning position

        while (elapsedTime < duration && !isAnimationSkipped)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Update the scale and position (smooth transition)
            screenImage.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            screenImage.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, t);

            // Detect skip input (mouse click or key press)
            if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
            {
                isAnimationSkipped = true;
                break;  // Exit the loop immediately
            }

            yield return null;
        }

        // Flash effect (optional)
        if (!isAnimationSkipped)
        {
            yield return StartCoroutine(PlayFlashEffect());
        }

        // Show the menu
        ShowMainMenu();
    }

    IEnumerator PlayFlashEffect()
    {
        // Implement a simple flash effect
        float flashDuration = 0.5f; // Duration of the flash
        float elapsedTime = 0f;
        Color initialColor = screenImage.color;
        Color flashColor = Color.white;

        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flashDuration;

            // Example flash effect (fading to white and back)
            screenImage.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(t * 2, 1));

            yield return null;
        }

        // Return to normal color
        screenImage.color = initialColor;
    }

    void ShowMainMenu()
    {
        // Enable the main menu UI
        menuUI.SetActive(true);

       
    }
}
