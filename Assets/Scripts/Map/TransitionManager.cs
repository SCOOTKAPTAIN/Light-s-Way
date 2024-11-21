using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TransitionManager : MonoBehaviour
{

    
    public Animator transistionanimator;
    void Start()
    {
        StartCoroutine(Act1());
    }

    

    IEnumerator Act1()
    {
        DialogueAudioManager.instance.PlayMusic("Map_100_Light");
        yield return new WaitForSeconds(5);
        transistionanimator.Play("Act1");
       
        

    }
}
