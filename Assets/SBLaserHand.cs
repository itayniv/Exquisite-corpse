using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SB.VR.Interactions
{

    public class SBLaserHand : MonoBehaviour {

        //turns trigger on for UI Menu Actions
        public bool TriggerOn;

        //link to the manager that effects different actions in the project
        public GameAction gameActions;

	
	    void Start () {
            RaycastToInteractables.instance.AddController(this);
        }

        public void ToggleTrigger(bool controllerState)
        {
            TriggerOn = controllerState;
        }

        public void OnPress()
        {
            if (gameActions != null)
            {
                gameActions.DoSomethingWithHandPosition(transform.position);
            }
        }

        public void OnPressDown()
        {
            if(gameActions != null)
            {
                gameActions.DoSomethingOnce(transform.position);
            }
        }
	
    }

}
