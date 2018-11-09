using UnityEngine;

namespace FIMSpace.GroundFitter
{
    public class FGroundFitter_Input : FGroundFitter_InputBase
    {
        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) TriggerJump();

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                if (Input.GetKey(KeyCode.LeftShift)) Sprint = true; else Sprint = false;

                RotationOffset = 0f;
                if (Input.GetKey(KeyCode.A)) RotationOffset = -90;
                if (Input.GetKey(KeyCode.D)) RotationOffset = 90;
                if (Input.GetKey(KeyCode.S)) RotationOffset = 180;

                MoveForward = true;
            }
            else
            {
                Sprint = false;
                MoveForward = false;
            }

            controller.Sprint = Sprint;
            controller.MoveForward = MoveForward;
            controller.RotationOffset = RotationOffset;
        }
    }
}