using FIMSpace.Basics;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.GroundFitter
{
    /// <summary>
    /// FM: Changing transform's orientation to fit to the ground, have controll over rotation in Y axis and possibility to move forward
    /// </summary>
    public class FGroundFitter : MonoBehaviour
    {
        [Tooltip("With this variable you define in which direction model should look - use it fro other scripts, check demo script for patrolling")]
        public float RotationYAxis = 0f;

        [Tooltip("How quick rotation should be corrected to target")]
        [Range(1f, 30f)]
        public float FittingSpeed = 6f;

        [Tooltip("Smoothing whole rotation motion")]
        [Range(0f, 1f)]
        public float TotalSmoother = 0f;

        /// <summary> If algorithm should be executed in LateUpdate queue </summary>
        public EFUpdateClock UpdateClock = EFUpdateClock.Update;

        [Header("> Tweaking Settings <")]
        // V1.1
        [Range(0f, 1f)]
        [Tooltip("If forward rotation value should go in lighter value than real normal hit direction")]
        public float MildForwardValue = 0f;

        [Tooltip("Maximum rotation angle in rotation of x axis, so rotating forward - degrees value of maximum rotation")]
        [Range(0f, 90f)]
        public float MaxForwardRotation = 90f;

        [Range(0f, 1f)]
        [Tooltip("If side rotation value should go in lighter value than real normal hit direction")]
        public float MildHorizontalValue = 0f;

        [Tooltip("If rotation should work on also on x axis - good for spiders, can look wrong on quadropeds etc.")]
        [Range(0f, 90f)]
        public float MaxHorizontalRotation = 90f;

        [Header("> Advanced settings <")]
        [Tooltip("We should cast raycast from position little higher than foots of your game object")]
        public float RaycastHeightOffset = 0.5f;
        [Tooltip("How far ray should cast to check if ground is under feet")]
        public float RaycastCheckRange = 5f;

        [Tooltip("If value is not equal 0 there will be casted second ray in front or back of gameObject")]
        public float LookAheadRaycast = 0f;
        [Tooltip("Blending with predicted forward raycast rotation")]
        public float AheadBlend = 0.5f;

        [Tooltip("Offset over ground")]
        public float YOffset = 0f;

        [Space(8f)]
        //V1.0.1
        [Tooltip("What collision layers should be included by algorithm")]
        // Here I set "TransparentFX" instead of "Terrain" because you would have to define Layer name "Terrain" by yourself
        public LayerMask GroundLayerMask = 1 << 1;
        [Tooltip("When casting down vector should adjust with transform's rotation")]
        public bool RelativeLookUp = true;
        [Range(0f, 1f)]
        public float RelativeLookUpBias = 0.25f;

        /// <summary> For other classes to have access </summary>
        public RaycastHit LastRaycast { get; protected set; }


        // V1.1.1
        [Space(8f, order = 0)]
        [Header("Alternative approach", order = 1)]
        [Tooltip("Casting more raycsts under object to detect ground more precisely, then we use average from all casts to set new rotation")]
        public bool ZoneCast = false;
        public Vector2 ZoneCastDimensions = new Vector2(0.3f, 0.5f);
        [Range(0f, 10f)]
        public float ZoneCastBias = 0f;
        [Range(0f, 1f)]
        [Tooltip("More precision = more raycasts = lower performance")]
        public float ZoneCastPrecision = 0.25f;


        // V1.1
        /// <summary> Quaternion to help controll over transform's rotation when object is placed and rotated to ground </summary>
        protected Quaternion helperRotation = Quaternion.identity;

        protected float delta;

        void Update()
        {
            if (UpdateClock != EFUpdateClock.Update) return;
            delta = Time.deltaTime;
            FitToGround();
        }

        void FixedUpdate()
        {
            if (UpdateClock != EFUpdateClock.FixedUpdate) return;
            delta = Time.fixedDeltaTime;
            FitToGround();
        }

        void LateUpdate()
        {
            if (UpdateClock != EFUpdateClock.LateUpdate) return;
            delta = Time.deltaTime;
            FitToGround();
        }

        private Vector3 GetUpVector()
        {
            if (RelativeLookUp)
            {
                return Vector3.Lerp(transform.up, Vector3.up, RelativeLookUpBias);
            }
            else return Vector3.up;
        }

        protected virtual void FitToGround()
        {
            // If we want we casting second ray to prevent object from clipping at big slopes
            RaycastHit aheadHit = new RaycastHit();

            if (LookAheadRaycast != 0f)
            {
                Physics.Raycast(transform.position + GetUpVector() * RaycastHeightOffset + transform.forward * LookAheadRaycast, -GetUpVector(), out aheadHit, RaycastCheckRange + YOffset, GroundLayerMask, QueryTriggerInteraction.Ignore);
            }

            RefreshLastRaycast();

            // If cast catch ground
            if (LastRaycast.transform)
            {
                // We rotate helper transform to snap ground with smoothness
                Quaternion fromTo = Quaternion.FromToRotation(Vector3.up, LastRaycast.normal);

                // We mix both casts for average rotation
                if (aheadHit.transform)
                {
                    Quaternion aheadFromTo = Quaternion.FromToRotation(Vector3.up, aheadHit.normal);
                    fromTo = Quaternion.Lerp(fromTo, aheadFromTo, AheadBlend);
                }

                helperRotation = Quaternion.Slerp(helperRotation, fromTo, delta * FittingSpeed);
            }
            else // If nothing is under our legs we rotate object smoothly to zero rotations
            {
                helperRotation = Quaternion.Slerp(helperRotation, Quaternion.identity, delta * FittingSpeed);
            }

            RotationCalculations();

            if ( LastRaycast.transform)
            transform.position = LastRaycast.point + Vector3.up * YOffset;
        }

        internal void RotationCalculations()
        {
            // Then we can rotate object to target look in y axis
            Quaternion targetRotation = helperRotation * Quaternion.AngleAxis(RotationYAxis, Vector3.up);

            targetRotation = Quaternion.Euler(Mathf.Clamp(FLogicMethods.WrapAngle(targetRotation.eulerAngles.x), -MaxForwardRotation, MaxForwardRotation) * (1 - MildForwardValue), targetRotation.eulerAngles.y, Mathf.Clamp(FLogicMethods.WrapAngle(targetRotation.eulerAngles.z), -MaxHorizontalRotation, MaxHorizontalRotation) * (1 - MildHorizontalValue));

            if (TotalSmoother == 0f)
                transform.rotation = targetRotation;
            else
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, delta * Mathf.Lerp(50f, 1f, TotalSmoother));
        }

        internal RaycastHit CastRay()
        {
            // Cast ground to get data about surface hit
            RaycastHit outHit;
            Physics.Raycast(transform.position + GetUpVector() * RaycastHeightOffset, -GetUpVector(), out outHit, RaycastCheckRange + Mathf.Abs(YOffset), GroundLayerMask, QueryTriggerInteraction.Ignore);

            // Making zone cast calculations to create average ray hit
            if (ZoneCast)
            {
                Vector3 p = transform.position + GetUpVector() * RaycastHeightOffset;
                Vector3 x = transform.right * ZoneCastDimensions.x;
                Vector3 z = transform.forward * ZoneCastDimensions.y;

                // Drawing rectangle for guide how zone rays will go
                Vector3 downDir = -GetUpVector() * RaycastCheckRange;

                List<RaycastHit> hits = new List<RaycastHit>();
                hits.Add(outHit);

                RaycastHit newHit;

                int c = 0;
                float part = 1f;
                for (int i = 0; i < Mathf.Lerp(4, 24, ZoneCastPrecision); i++)
                {
                    Vector3 dir = downDir;
                    Vector3 sum = Vector3.zero;

                    if (c == 0) sum = x - z;
                    else if (c == 1) sum = x + z;
                    else if (c == 2) sum = -x + z;
                    else if (c == 3)
                    {
                        sum = -x - z;
                        part += 0.75f;
                        c = -1;
                    }

                    Physics.Raycast(p + sum / part, -GetUpVector() + sum * ZoneCastBias, out newHit, RaycastCheckRange + Mathf.Abs(YOffset), GroundLayerMask, QueryTriggerInteraction.Ignore);
                    if (newHit.transform) hits.Add(newHit);

                    c++;
                }

                Vector3 avPos = Vector3.zero;
                Vector3 avNormal = Vector3.zero;

                for (int i = 0; i < hits.Count; i++)
                {
                    avNormal += hits[i].normal;
                    avPos += hits[i].point;
                }

                avPos /= hits.Count;
                avNormal /= hits.Count;

                outHit.normal = avNormal;
                if (!outHit.transform)
                {
                    outHit.point = new Vector3(avPos.x, transform.position.y, avPos.z);
                }
            }

            return outHit;
        }

        internal void RefreshLastRaycast()
        {
            LastRaycast = CastRay();
        }

        // V1.0.1
#if UNITY_EDITOR

        [Space(4f)]
        public bool drawDebug = true;

        // Set it to false if you don't want show clickable gizmo
        [Space(1f)]
        public bool drawGizmo = true;

        protected virtual void OnDrawGizmos()
        {
            if (drawDebug)
            {
                Gizmos.color = new Color(1f, 0.2f, 0.4f, 0.8f);
                Gizmos.DrawRay(transform.position + GetUpVector() * RaycastHeightOffset, GetUpVector() * -RaycastCheckRange);

                if (LookAheadRaycast != 0f)
                {
                    Gizmos.color = new Color(0.2f, 0.4f, 1f, 0.8f);
                    Gizmos.DrawRay(transform.position + GetUpVector() * RaycastHeightOffset + transform.forward * LookAheadRaycast, -GetUpVector() * (RaycastCheckRange /*+ YOffset*/));
                }

                if (ZoneCast)
                {
                    Vector3 p = transform.position + GetUpVector() * RaycastHeightOffset;
                    Vector3 x = transform.right * ZoneCastDimensions.x;
                    Vector3 z = transform.forward * ZoneCastDimensions.y;

                    Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.8f);

                    // Drawing rectangle for guide how zone rays will go
                    Gizmos.DrawLine(p + x - z, p + x + z);
                    Gizmos.DrawLine(p + x + z, p - x + z);
                    Gizmos.DrawLine(p - x + z, p - x - z);
                    Gizmos.DrawLine(p - x - z, p + x - z);

                    Vector3 downDir = -GetUpVector() * RaycastCheckRange;

                    int c = 0;
                    float part = 1f;
                    for (int i = 0; i < Mathf.Lerp(4, 24, ZoneCastPrecision); i++)
                    {
                        Vector3 sum = Vector3.zero;

                        if (c == 0) sum = x - z;
                        else if (c == 1) sum = x + z;
                        else if (c == 2) sum = -x + z;
                        else if (c == 3)
                        {
                            sum = -x - z;
                            part += 0.75f;
                            if (part > 2.5f) part += 1f;
                            if (part > 5.5f) part += 3f;
                            if (part > 10f) part += 7f;
                            c = -1;
                        }

                        float div = 1 + part;
                        Gizmos.DrawRay(p + sum / part, downDir + sum * ZoneCastBias);

                        c++;
                    }
                }
            }

            if (!drawGizmo) return;
            Gizmos.DrawIcon(transform.position, "FIMSpace/GroundFitter/SPR_GroundFitter Gizmo.png", true);
        }
#endif
    }
}