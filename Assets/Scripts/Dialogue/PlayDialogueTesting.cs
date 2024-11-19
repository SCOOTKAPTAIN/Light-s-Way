using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
//DialogueManager.GetInstance().EnterDialogueMode(scene1, backgroundAnimator);
//StartCoroutine(Flow());
//yield return new WaitForSeconds(5);
//FadeBox.Play("FadeOut");
//FadeBox.Play("FadeIn");
public class PlayDialogueTesting : MonoBehaviour
{

    [Header("Background Animator")]
    [SerializeField] private Animator backgroundAnimator;

    [SerializeField] private Animator effectAnimator;

    [SerializeField] private Animator FadeBox;

    [SerializeField] private TextAsset scene1;

    private bool GotoGame;
    [SerializeField] private string scenename;
    

    private void Start()
    {
        DialogueAudioManager.instance.PlayMusic("clock");
        StartCoroutine(Flow());
    }


    private void Update()
    {
        if(GotoGame == true && !DialogueManager.GetInstance().dialogueIsPlaying)
        {
            StartCoroutine(GameGo());
        }

    }



    IEnumerator Flow()
   {
      FadeBox.Play("FadeIn");

       yield return new WaitForSeconds(4);
       DialogueManager.GetInstance().EnterDialogueMode(scene1, backgroundAnimator, effectAnimator);
       GotoGame = true;


       

    }

     IEnumerator GameGo()
    {
        yield return new WaitForSeconds(6);
        SceneManager.LoadScene(scenename);

    }



}






