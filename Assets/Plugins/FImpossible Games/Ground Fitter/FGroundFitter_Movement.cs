using UnityEngine;

namespace FIMSpace.GroundFitter
{
    /// <summary>
    /// FM: Example component to use groud fitter for movement controller
    /// </summary>
    [RequireComponent(typeof(FGroundFitter))]
    public class FGroundFitter_Movement : MonoBehaviour
    {
        [Header("> Main Tweak Variables <")]
        public float BaseSpeed = 3f;
        public float RotateToTargetSpeed = 6f;
        public float SprintingSpeed = 10f;

        protected float ActiveSpeed = 0f;
        public float AccelerationSpeed = 10f;
        public float DecelerationSpeed = 10f;

        [Header("> Additional Options <")]
        // Physics variables
        public float JumpPower = 7f; // variable used in TriggerJump() in script FGroundFitter_InputBase
        public float gravity = 15f;
        public bool MultiplySprintAnimation = false;

        [Tooltip("You need collider and rigidbody on object to make it work right - ALSO CHANGE YOUR CAMERA UPDATE CLOCK TO FIXED UPDATE AND USE TIME.fixedDeltaTime - ! For now it can cause errors when jumping, character can go through floor sometimes ! - Will be upgraded in future versions")]
        [Header("(experimental)")]
        public bool UsePhysics = false;

        internal float YVelocity;
        protected bool inAir = false;
        protected float gravityOffset = 0f;

        // Motion Variables changed from FGroundFitter_Input script
        internal bool MoveForward = false;
        internal bool Sprint = false;
        internal float RotationOffset = 0f;

        // Other variables
        protected string lastAnim = "";

        // Important references
        protected Animator animator;
        protected FGroundFitter fitter; // Using fitter for some transforming stuff
        protected Rigidbody rigb; // Using fitter for some transforming stuff

        // Using aniation speed property in animator to change speed of animations from code
        protected bool animatorHaveAnimationSpeedProp = false;
        protected float initialYOffset;

        // Additional motion calculations
        protected Vector3 holdJumpPosition;
        protected float freezeJumpYPosition;
        protected float delta;

        /// <summary>
        /// Preparin initial stuff
        /// </summary>
        protected virtual void Start()
        {
            fitter = GetComponent<FGroundFitter>();
            animator = GetComponentInChildren<Animator>();
            rigb = GetComponent<Rigidbody>();

            if (animator) if (HasParameter(animator, "AnimationSpeed")) animatorHaveAnimationSpeedProp = true;

            fitter.RotationYAxis = transform.rotation.eulerAngles.y;
            initialYOffset = fitter.YOffset;

            fitter.RefreshLastRaycast();
        }


        protected virtual void Update()
        {
            if (UsePhysics) return;

            delta = Time.deltaTime;
            HandleGravity();
            HandleAnimations();
            HandleTransforming();
        }

        protected virtual void FixedUpdate()
        {
            if (rigb)
            {
                if (UsePhysics)
                {
                    rigb.useGravity = false;
                    rigb.isKinematic = false;
                }
                else
                {
                    rigb.isKinematic = true;
                }
            }

            if (!UsePhysics) return;

            delta = Time.fixedDeltaTime;

            HandleGravity();
            HandleAnimations();
            HandleTransforming();

            rigb.velocity = Vector3.zero;
            rigb.angularVelocity = Vector3.zero;
            rigb.freezeRotation = true;
        }

        /// <summary>
        /// Calculating gravity stuff
        /// </summary>
        protected virtual void HandleGravity()
        {
            if (fitter.enabled)
            {
                if (fitter.YOffset > initialYOffset)
                    fitter.YOffset += YVelocity * delta;
                else
                    fitter.YOffset = initialYOffset;
            }
            else
                fitter.YOffset += YVelocity * delta;

            if (inAir) YVelocity -= gravity * delta;

            if (fitter.enabled)
            {
                if (!fitter.LastRaycast.transform)
                {
                    if (!inAir)
                    {
                        inAir = true;
                        holdJumpPosition = transform.position;
                        freezeJumpYPosition = holdJumpPosition.y;
                        YVelocity = -1f;
                        fitter.enabled = false;
                    }
                }
                else
                    if (YVelocity > 0f)
                {
                    inAir = true;
                }
            }

            if (inAir)
            {
                if (fitter.enabled) fitter.enabled = false;

                if (YVelocity < 0f)
                {
                    RaycastHit hit = fitter.CastRay();

                    if (hit.transform)
                    {
                        //if (transform.position.y + (YVelocity * delta) <= hit.point.y + initialYOffset + 0.05f)
                        if (transform.position.y + (YVelocity * delta) <= hit.point.y + initialYOffset + 0.05f)
                        {
                            fitter.YOffset -= (hit.point.y - freezeJumpYPosition);
                            HitGround();
                        }
                    }
                }
                else
                {
                    RaycastHit hit = fitter.CastRay();

                    if (hit.transform)
                    {
                        if (hit.point.y - 0.1f > transform.position.y )
                        {
                            fitter.YOffset = initialYOffset;
                            YVelocity = -1f;
                            HitGround();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handling switching animation clips of Animator
        /// </summary>
        protected virtual void HandleAnimations()
        {
            if (ActiveSpeed > 0.15f)
            {
                if (Sprint)
                    CrossfadeTo("Run", 0.25f);
                else
                    CrossfadeTo("Walk", 0.25f);
            }
            else
            {
                CrossfadeTo("Idle", 0.25f);
            }

            // If object is in air we just slowing animation speed to zero
            if (animatorHaveAnimationSpeedProp)
                if (inAir) FAnimatorMethods.LerpFloatValue(animator, "AnimationSpeed", 0f);
                else
                    FAnimatorMethods.LerpFloatValue(animator, "AnimationSpeed", MultiplySprintAnimation ? (ActiveSpeed / BaseSpeed) : Mathf.Min(1f, (ActiveSpeed / BaseSpeed)));
        }


        /// <summary>
        /// Refreshing some switching to new landing position varibles, useful in custom coding
        /// </summary>
        protected void RefreshHitGroundVars(RaycastHit hit)
        {
            holdJumpPosition = hit.point;
            freezeJumpYPosition = hit.point.y;
            fitter.YOffset = Mathf.Abs(fitter.LastRaycast.point.y - transform.position.y);
        }


        /// <summary>
        /// Calculating changes for transform
        /// </summary>
        protected virtual void HandleTransforming()
        {
            if (fitter.enabled)
            {
                if (fitter.LastRaycast.transform)
                {
                    transform.position = fitter.LastRaycast.point + fitter.YOffset * Vector3.up;
                    holdJumpPosition = transform.position;
                    freezeJumpYPosition = holdJumpPosition.y;
                }
                else
                    inAir = true;
            }
            else
            {
                holdJumpPosition.y = freezeJumpYPosition + fitter.YOffset;
            }

            if (MoveForward)
            {
                if ( !fitter.enabled )
                {
                    fitter.RotationYAxis = Mathf.LerpAngle(fitter.RotationYAxis, Camera.main.transform.eulerAngles.y + RotationOffset, delta * RotateToTargetSpeed * 0.15f);
                    fitter.RotationCalculations();
                }
                else
                    fitter.RotationYAxis = Mathf.LerpAngle(fitter.RotationYAxis, Camera.main.transform.eulerAngles.y + RotationOffset, delta * RotateToTargetSpeed);


                if (!Sprint)
                    ActiveSpeed = Mathf.Lerp(ActiveSpeed, BaseSpeed, delta * AccelerationSpeed);
                else
                    ActiveSpeed = Mathf.Lerp(ActiveSpeed, SprintingSpeed, delta * AccelerationSpeed);
            }
            else
            {
                if (ActiveSpeed > 0f)
                    ActiveSpeed = Mathf.Lerp(ActiveSpeed, -0.01f, delta * DecelerationSpeed);
                else ActiveSpeed = 0f;
            }

            holdJumpPosition += ((transform.forward * ActiveSpeed)) * delta;
            transform.position = holdJumpPosition;
        }

        /// <summary>
        /// Method executed when object is landing on ground from beeing in air lately
        /// </summary>
        protected virtual void HitGround()
        {
            fitter.RefreshLastRaycast();
            fitter.enabled = true;
            inAir = false;
            freezeJumpYPosition = 0f;
        }

        /// <summary>
        /// Trigger this method so object will jump
        /// </summary>
        public virtual void Jump()
        {
            YVelocity = JumpPower;
            fitter.YOffset += JumpPower * Time.deltaTime / 2f;
        }

        /// <summary>
        /// Crossfading to target animation with protection of playing same animation over again
        /// </summary>
        protected virtual void CrossfadeTo(string animation, float transitionTime = 0.25f)
        {
            if (!animator.HasState(0, Animator.StringToHash(animation)))
            {
                // Preventing holding shift for sprint and starting walking freeze on idle  
                if (animation == "Run") animation = "Walk"; else return;
            }

            if (lastAnim != animation)
            {
                animator.CrossFadeInFixedTime(animation, transitionTime);
                lastAnim = animation;
            }
        }

        /// <summary>
        /// Checking if animator have parameter with choosed name
        /// </summary>
        public static bool HasParameter(Animator animator, string paramName)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                    return true;
            }
            return false;
        }
    }
}