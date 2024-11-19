using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MainMenutest : MonoBehaviour
{

 
    [SerializeField] private Button playButton;

    public void PlayGame()
    {
        playButton.interactable = false;
        StartCoroutine(PlayPress());
        EventSystem.current.SetSelectedGameObject(null);

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator PlayPress()
   {

    yield return new WaitForSeconds(1);

    SceneManager.LoadScene("MainMenu");
        

    }

    
}
