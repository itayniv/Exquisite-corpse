using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class InteractableUI : MonoBehaviour {

	public UnityEvent UIHoverInEvent;
    public UnityEvent UIHoverOutEvent;
    public UnityEvent UIClickEvent;

    EventSystem m_EventSystem;

    void OnEnable()
    {
        //Fetch the current EventSystem. Make sure your Scene has one.
        m_EventSystem = EventSystem.current;
    }

    public virtual void OnHoverOff()
    {
        // Trigger the event!
        UIHoverOutEvent.Invoke();
    }

    public virtual void OnHoverOn()
    {
        // Trigger the event!
        UIHoverInEvent.Invoke();
    }

    public virtual void OnClick()
    {
        // Trigger the event!
        UIClickEvent.Invoke();
    }
}
