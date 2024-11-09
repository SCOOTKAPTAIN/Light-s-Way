using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenSequence : MonoBehaviour
{
    public Image blackScreen;          
    public Image whiteFlashScreen;     
    public Image girlImage;            
    public Image gameTitleImage;       
    public Image backgroundImage;      
    public GameObject menu;            

    public Animator blackscreen;

    public float fadeDuration = 1.5f;  
    public float delayBetweenFades = 1f;
    public float shineDuration = 0.5f;

    private bool isSequenceSkipped = false;  // Variable to track if the sequence is skipped

    private void Start()
    {
        SetInitialVisibility();
        StartCoroutine(PlayTitleScreenSequence());
    }

    private void Update()
    {
        // Check for mouse click (left button or touch)
        if (Input.GetMouseButtonDown(0) && !isSequenceSkipped)
        {
            SkipSequence();
        }
    }

    private void SetInitialVisibility()
    {
        blackScreen.color = new Color(0, 0, 0, 1);
        girlImage.color = new Color(1, 1, 1, 0);
        gameTitleImage.color = new Color(1, 1, 1, 0);
        backgroundImage.color = new Color(1, 1, 1, 0);
        menu.SetActive(false);
        whiteFlashScreen.color = new Color(1, 1, 1, 0);
    }

    private IEnumerator PlayTitleScreenSequence()
    {
        blackscreen.Play("startsequenceblack");

        yield return new WaitForSeconds(delayBetweenFades);
        yield return FadeImage(girlImage, fadeDuration, true);
        yield return new WaitForSeconds(delayBetweenFades);
        yield return FadeImage(gameTitleImage, fadeDuration, true);
        yield return new WaitForSeconds(delayBetweenFades);
        yield return FadeImage(backgroundImage, fadeDuration, true);

        menu.SetActive(true); // Show the menu
    }

    private IEnumerator FadeImage(Image img, float duration, bool fadeIn)
    {
        float timer = 0f;
        Color startColor = img.color;
        Color endColor = fadeIn ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0);

        while (timer < duration)
        {
            if (isSequenceSkipped) yield break;  // Stop fading if sequence is skipped
            timer += Time.deltaTime;
            img.color = Color.Lerp(startColor, endColor, timer / duration);
            yield return null;
        }

        img.color = endColor;
    }

    private void SkipSequence()
    {
        isSequenceSkipped = true;  // Mark that the sequence was skipped

        // Stop all ongoing coroutines
        StopAllCoroutines();

        // Instantly show all the elements
        blackScreen.color = new Color(0, 0, 0, 0);  // Make black screen fully transparent
        girlImage.color = new Color(1, 1, 1, 1);    // Make girl image fully visible
        gameTitleImage.color = new Color(1, 1, 1, 1); // Make game title fully visible
        backgroundImage.color = new Color(1, 1, 1, 1); // Make background fully visible

        menu.SetActive(true); // Show the menu
    }
}
