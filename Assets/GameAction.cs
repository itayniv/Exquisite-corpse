using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAction : MonoBehaviour {


    public Color[] brushColors;
    public Color currenColor;

    public Material[] skyBoxes;


    public controllerMode currentMode;

    public GameObject objectTOCreate;


    public enum controllerMode
    {
        DRAWING,
        CREATE
    }

    public void DoSomethingOnce(Vector3 position)
    {
       
        if (currentMode == controllerMode.DRAWING)
        {
           // GetComponentInChildren<DrawingEngine>().initializeNewLine(position);

        }
        else if (currentMode == controllerMode.CREATE)
        {
            Instantiate(objectTOCreate,position,Quaternion.identity);
        }
    }

    public void DoSomethingWithHandPosition(Vector3 position)
    {
       
        if (currentMode == controllerMode.DRAWING)
        {
           // GetComponentInChildren<DrawingEngine>().DrawPoint(position);

        }

    }


    public void ChandeMode(int gameMode)
    {
        currentMode = (controllerMode)gameMode;
    }

    public void ChangeColor(int colorNum)
    {

        if (colorNum >= brushColors.Length)
            Debug.LogError("your selection is higher than number of colors"); 

        switch(colorNum)
        {
            case 0:
                currenColor = brushColors[colorNum];
                break;
            case 1:
                currenColor = brushColors[colorNum];
                break;

            default:
                break;

        }
    }

    public void ChangeSkybox(int skynum)
    {

        if (skynum >= skyBoxes.Length)
            Debug.LogError("your selection is higher than number of skybox materials in the array");

        switch (skynum)
        {
            case 0:
                RenderSettings.skybox = skyBoxes[skynum];
                break;
            case 1:
                RenderSettings.skybox = skyBoxes[skynum];
                break;

            default:
                break;

        }
    }

  
}
