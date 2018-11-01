using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableUI_Button : InteractableUI
{
   
    public Button button;

    void Start()
    {
        button = GetComponent<Button>();
    }

    public override void OnHoverOff()
    {
       
        button.OnDeselect(null);
        // Trigger the event!
        base.OnHoverOff();
    }

    public override void OnHoverOn()
    {
        button.OnSelect(null);
        button.Select();
        base.OnHoverOn();
    }

    public override void OnClick()
    {
       
        base.OnClick();
    }

}
