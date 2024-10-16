using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{

    [SerializeField] private Animator FlashScreen;
    [SerializeField] private Button playButton;
    [SerializeField] private Button Options;
    [SerializeField] private Button Quit;
    public void PlayGame()
    {
        playButton.interactable = false;
        Options.interactable = false;
        Quit.interactable = false;
        StartCoroutine(PlayPress());
        EventSystem.current.SetSelectedGameObject(null);

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator PlayPress()
   {
    FlashScreen.Play("White");
    yield return new WaitForSeconds(3);
    FlashScreen.Play("Black");
    yield return new WaitForSeconds(4);
    SceneManager.LoadScene("1- Map");
        

    }

    
}
