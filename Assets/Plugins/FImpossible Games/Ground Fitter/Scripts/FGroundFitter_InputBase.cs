using UnityEngine;

namespace FIMSpace.GroundFitter
{
    /// <summary>
    /// Basic input class for ground fitter purposes
    /// </summary>
    [RequireComponent( typeof(FGroundFitter_Movement))]
    public abstract class FGroundFitter_InputBase : MonoBehaviour
    {
        protected FGroundFitter fitter;
        protected FGroundFitter_Movement controller;

        public float RotationOffset { get; protected set; }
        public bool Sprint { get; protected set; }
        public bool MoveForward { get; protected set; }

        public virtual void Start()
        {
            fitter = GetComponent<FGroundFitter>();
            controller = GetComponent<FGroundFitter_Movement>();

            RotationOffset = 0f;
            Sprint = false;
            MoveForward = false;
        }

        protected virtual void TriggerJump()
        {
            controller.Jump();
        }
    }
}