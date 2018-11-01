using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenuHandler : MonoBehaviour {


    public GameObject menu;
    public GameObject handRay;

	
    public void enableHandMenu()
    {
        Debug.Log("enable menu");

        menu.SetActive(true);
        handRay.SetActive(true);
    }

    public void disableHandMenu()
    {
        Debug.Log("disable menu");

        menu.SetActive(false);
        handRay.SetActive(false);
    }
}
