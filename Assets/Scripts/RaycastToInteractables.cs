using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace SB.VR.Interactions
{
    public class LaserPointerEventData : PointerEventData
    {
        public GameObject current;
        public SBLaserHand controller;
        public LaserPointerEventData(EventSystem e) : base(e) { }

        public override void Reset()
        {
            current = null;
            controller = null;
            base.Reset();
        }
    }


    public class RaycastToInteractables : BaseInputModule
    {

        public LineRenderer laserLineRenderer;
        public float laserMaxLength = 5f;

        public InteractableUI[] interactableUIs;


        public static RaycastToInteractables instance { get { return _instance; } }
        private static RaycastToInteractables _instance = null;

        // storage class for controller specific data
        private class ControllerData
        {
            public LaserPointerEventData pointerEvent;
            public SBLaserHand controllerLaser;
            public GameObject currentPoint;
            public GameObject currentPressed;
            public GameObject currentDragging;
        };


        private Camera UICamera;
        private PhysicsRaycaster raycaster;


        private Dictionary<SBLaserHand, ControllerData> _controllerData = new Dictionary<SBLaserHand, ControllerData>();


        protected override void Awake()
        {
            base.Awake();
           
            if (_instance != null)
            {
                Debug.LogWarning("Trying to instantiate multiple LaserPointerInputModule.");
                DestroyImmediate(this.gameObject);
            }

            _instance = this;
        }

        protected override void Start()
        {
            base.Start();

            //add controllers interacting with the UI to the list
           

            //find all interactables in the scene
            interactableUIs = FindObjectsOfType<InteractableUI>();

            //  laserLineRenderer.SetWidth(0.2f, 0.2f);
            laserLineRenderer.startWidth = 0.03f;
            laserLineRenderer.endWidth = 0.03f;

            // Create a new camera that will be used for raycasts
            UICamera = new GameObject("UI Camera").AddComponent<Camera>();
            // Added PhysicsRaycaster so that pointer events are sent to 3d objects
            raycaster = UICamera.gameObject.AddComponent<PhysicsRaycaster>();
            UICamera.clearFlags = CameraClearFlags.Nothing;
            UICamera.enabled = false;
            UICamera.fieldOfView = 5;
            UICamera.nearClipPlane = 0.01f;

            // Find canvases in the scene and assign our custom
            // UICamera to them
            Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                canvas.worldCamera = UICamera;
            }

        }

        public void AddController(SBLaserHand controller)
        {
            _controllerData.Add(controller, new ControllerData());
        }

        public void RemoveController(SBLaserHand controller)
        {
            _controllerData.Remove(controller);
        }

        // clear the current selection
        public void ClearSelection()
        {
            if (base.eventSystem.currentSelectedGameObject)
            {
                base.eventSystem.SetSelectedGameObject(null);
            }
        }

        // select a game object
        private void Select(GameObject go)
        {
            ClearSelection();

            if (ExecuteEvents.GetEventHandler<ISelectHandler>(go))
            {
                base.eventSystem.SetSelectedGameObject(go);
            }
        }


        protected void UpdateCameraPosition(SBLaserHand controller)
        {
            UICamera.transform.position = controller.transform.position;
            UICamera.transform.rotation = controller.transform.rotation;
        }


        public override void Process()
        {

            foreach (var pair in _controllerData)
            {
                SBLaserHand controller = pair.Key;
                ControllerData data = pair.Value;

                // Test if UICamera is looking at a GUI element
                UpdateCameraPosition(controller);

                if (data.pointerEvent == null)
                    data.pointerEvent = new LaserPointerEventData(EventSystem.current);
                else
                    data.pointerEvent.Reset();

                data.pointerEvent.controller = controller;
                data.pointerEvent.delta = Vector2.zero;
                data.pointerEvent.position = new Vector2(UICamera.pixelWidth * 0.5f, UICamera.pixelHeight * 0.5f);
                //data.pointerEvent.scrollDelta = Vector2.zero;

                int layer_mask = LayerMask.NameToLayer("WorldUI");
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(data.pointerEvent, results);

                data.pointerEvent.pointerCurrentRaycast = FindFirstRaycast(results);
               

                //change drawing of linerender according to raycast
                if (results.Count > 0)
                {
                    Vector3 targetPos = controller.transform.localPosition;
                    float dist = 0;
                    foreach (RaycastResult result in results)
                    {

                        if (result.gameObject.layer == layer_mask)
                        {
                            dist = Vector3.Distance(result.gameObject.transform.position, controller.transform.position);
                        }
                    }

                    laserLineRenderer.SetPosition(0, controller.transform.localPosition);
                    laserLineRenderer.SetPosition(1, (controller.transform.localPosition + (dist * Vector3.forward)));
                }
                else
                {
                    laserLineRenderer.SetPosition(0, controller.transform.localPosition);
                    laserLineRenderer.SetPosition(1, (controller.transform.localPosition + (laserMaxLength * Vector3.forward)));
                }

                //clear raycast results
                results.Clear();


                // Send control enter and exit events to our controller
                var hitControl = data.pointerEvent.pointerCurrentRaycast.gameObject;
                data.currentPoint = hitControl;

                // Handle enter and exit events on the GUI controlls that are hit
                base.HandlePointerExitAndEnter(data.pointerEvent, data.currentPoint);


                if (controller.TriggerOn)
                {
                    ClearSelection();

                    data.pointerEvent.pressPosition = data.pointerEvent.position;
                    data.pointerEvent.pointerPressRaycast = data.pointerEvent.pointerCurrentRaycast;
                    data.pointerEvent.pointerPress = null;

                    // update current pressed if the curser is over an element
                    if (data.currentPoint != null)
                    {
                        data.currentPressed = data.currentPoint;
                        data.pointerEvent.current = data.currentPressed;
                        GameObject newPressed = ExecuteEvents.ExecuteHierarchy(data.currentPressed, data.pointerEvent, ExecuteEvents.pointerDownHandler);
                        ExecuteEvents.Execute(controller.gameObject, data.pointerEvent, ExecuteEvents.pointerDownHandler);
                        if (newPressed == null)
                        {
                            // some UI elements might only have click handler and not pointer down handler
                            newPressed = ExecuteEvents.ExecuteHierarchy(data.currentPressed, data.pointerEvent, ExecuteEvents.pointerClickHandler);
                            ExecuteEvents.Execute(controller.gameObject, data.pointerEvent, ExecuteEvents.pointerClickHandler);
                            if (newPressed != null)
                            {
                                data.currentPressed = newPressed;
                            }
                        }
                        else
                        {
                            data.currentPressed = newPressed;
                            // we want to do click on button down at same time, unlike regular mouse processing
                            // which does click when mouse goes up over same object it went down on
                            // reason to do this is head tracking might be jittery and this makes it easier to click buttons
                            ExecuteEvents.Execute(newPressed, data.pointerEvent, ExecuteEvents.pointerClickHandler);
                            ExecuteEvents.Execute(controller.gameObject, data.pointerEvent, ExecuteEvents.pointerClickHandler);

                        }

                        if (newPressed != null)
                        {
                            data.pointerEvent.pointerPress = newPressed;
                            data.currentPressed = newPressed;
                            Select(data.currentPressed);
                        }

                        ExecuteEvents.Execute(data.currentPressed, data.pointerEvent, ExecuteEvents.beginDragHandler);
                        ExecuteEvents.Execute(controller.gameObject, data.pointerEvent, ExecuteEvents.beginDragHandler);

                        data.pointerEvent.pointerDrag = data.currentPressed;
                        data.currentDragging = data.currentPressed;
                    }
                }// button down end


                // update selected element for keyboard focus
                if (base.eventSystem.currentSelectedGameObject != null)
                {
                    data.pointerEvent.current = eventSystem.currentSelectedGameObject;
                    ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, GetBaseEventData(), ExecuteEvents.updateSelectedHandler);
                    //ExecuteEvents.Execute(controller.gameObject, GetBaseEventData(), ExecuteEvents.updateSelectedHandler);
                }


            }

        }

    }
}