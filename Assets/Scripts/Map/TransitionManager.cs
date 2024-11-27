using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;


public class TransitionManager : MonoBehaviour
{

    //public int ActNumber = 1;
    public Animator transistionanimator;
    
    void Start()
    {
        Debug.Log("initial value: " + GameManager.Instance.PersistentGameplayData.ActNumber);
        if(GameManager.Instance.PersistentGameplayData.ActNumber == 0)
        {
           StartCoroutine(Act1());
        }else
        {
            PlayAct();
        }

        
    }


    void Update()
    {
        

    }

    

    IEnumerator Act1()
    {
        transistionanimator.Play("Act1");
        yield return new WaitForSeconds(1);
        DialogueAudioManager.instance.PlayMusic("Map_100_Light");
        UIManager.Instance.InformationCanvas.gameObject.SetActive(true);
        GameManager.Instance.PersistentGameplayData.ActNumber++;
        Debug.Log("before intro" + GameManager.Instance.PersistentGameplayData.ActNumber);
    }

    public void PlayAct()
    {

        // 0 - start
        // 1 - Act 1
        // 2 - Act 1 Boss
        // 3 - Act 2
        // 4 - Act 2 Boss
        // 5 - Act 3
        // 6 - Act 3 Boss
        // 7 - Act 4 
        // 8 - Act 4 Boss
        // 9 - Final Act
        // 10 - Final Boss
        switch(GameManager.Instance.PersistentGameplayData.ActNumber)
        {
            case 3:
            transistionanimator.Play("Act2");
            break;

            case 5:
            transistionanimator.Play("Act3");
            break;

            case 7:
            transistionanimator.Play("Act4");
            break;

            case 9:
            transistionanimator.Play("Act5");
            break;

            default:
            break;
        }

    }
}
