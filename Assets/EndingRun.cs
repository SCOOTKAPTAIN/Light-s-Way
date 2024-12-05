using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingRun : MonoBehaviour
{
    [Header("Background Animator")]
    [SerializeField] private Animator backgroundAnimator;

    [SerializeField] private Animator effectAnimator;

    [SerializeField] private Animator FadeBox;

    [SerializeField] private List<TextAsset> DialogueList;

    private int DialogueId = 0;

    private bool EndDialogue = false;


    
 private void Start()
    {
        backgroundAnimator.Play("ending");
        EndDialogue = false;
       // DialogueProcessing();
        StartCoroutine(Flow());
    }

    private void Update()
    {
        if(EndDialogue == true && !DialogueManager.GetInstance().dialogueIsPlaying)
        {
            StartCoroutine(BacktoMenu());
        }
    }


     IEnumerator Flow()
   {
       yield return new WaitForSeconds(3);
       DialogueAudioManager.instance.PlayMusic("ending");
       FadeBox.Play("FadeIn");
       yield return new WaitForSeconds(3);
       DialogueManager.GetInstance().EnterDialogueMode(DialogueList[0], backgroundAnimator, effectAnimator);
       EndDialogue = true;
    }


     IEnumerator BacktoMenu()
    {
        
     yield return new WaitForSeconds(10);
     FadeBox.Play("FadeOut");
     yield return new WaitForSeconds(5);
     DialogueAudioManager.instance.music_source.Stop();
     yield return new WaitForSeconds(1);

     SceneManager.LoadScene("MainMenu");
     
     
    }




}
