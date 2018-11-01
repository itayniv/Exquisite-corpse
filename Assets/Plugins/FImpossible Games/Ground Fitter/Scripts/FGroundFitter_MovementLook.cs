using UnityEngine;

namespace FIMSpace.GroundFitter
{
    /// <summary>
    /// FM: Example component to use groud fitter for movement controller
    /// </summary>
    public class FGroundFitter_MovementLook : FGroundFitter_Movement
    {
        public Transform targetOfLook;

        protected override void HandleTransforming()
        {
            base.HandleTransforming();
            if ( MoveForward ) SetLookAtPosition(transform.position + Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y + RotationOffset, 0f) * Vector3.forward * 10f);
        }

        private void SetLookAtPosition(Vector3 tPos)
        {
            if ( targetOfLook)
            targetOfLook.position = tPos + Vector3.up;
        }
    }
}