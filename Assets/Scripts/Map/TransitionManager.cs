using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;


public class TransitionManager : MonoBehaviour
{

    //public int ActNumber = 1;
    public Animator transistionanimator;
    public GameObject IntroCover;
    void Start()
    {
        Debug.Log("initial value: " + GameManager.Instance.PersistentGameplayData.ActNumber);
        if(GameManager.Instance.PersistentGameplayData.ActNumber == 1)
        {
           StartCoroutine(Act1());
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
        switch(GameManager.Instance.PersistentGameplayData.ActNumber)
        {
            case 2:
            transistionanimator.Play("Act2");
            break;

            case 3:
            transistionanimator.Play("Act3");
            break;

            case 4:
            transistionanimator.Play("Act4");
            break;

            case 5:
            transistionanimator.Play("Act5");
            break;


        }

    }
}
