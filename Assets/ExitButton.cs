using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButton : MonoBehaviour
{

    public GameObject confirmationDialog;

    public void Awake()
        {
            confirmationDialog.SetActive(false);

        }

         public void ShowExitConfirmationDialog()
    {
        confirmationDialog.SetActive(true);
    }

    public void CancelExit()
    {
        confirmationDialog.SetActive(false);
    }

    

}
