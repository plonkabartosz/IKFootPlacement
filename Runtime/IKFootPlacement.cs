// ----------------------
// Copyright (c) 2025 Bartosz Plonka
// Author: Bartosz Plonka
// Year: 2025
// Source: https://github.com/plonkabartosz/IKFootPlacement.git
// License: CC BY 4.0 International (https://creativecommons.org/licenses/by/4.0/)
// ----------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;

public class IKFootPlacement : MonoBehaviour
{
    #region Class Variables
    [Header("Two Bone IK Constraints")]
    [Tooltip("Two Bone IK Constraints that represent each leg, that is controlled by inverse kinematics. " +
             "\n\nTips: " +
             "\n- be sure to correctly setup the targets and hints inside these constraints.")]
    public List<TwoBoneIKConstraint> iKConstraints = new List<TwoBoneIKConstraint>();

    [Header("Hips Transform")]
    [Tooltip("Element of character's skeleton that represents the hips." +
             "\n\nTips: " +
             "\n- this transform's position will be modified to ensure a stable distance between the hips and the feet, " +
             "\n- make sure that this transform is a parent of leg joints in the skeleton's hierarchy.")]
    public Transform hips;

    [Header("Raycast Properties")]
    [Tooltip("Value that controls the height from which raycasts are casted downwards. " +
             "\n\nTips: " +
             "\n- default value = 0.5, " +
             "\n- toggle Gizmos visibility in the Scene view.")]
    public float raycastHeight = 0.5f;

    [Tooltip("Value that controls the length (depth) of casted raycasts. " +
             "\n\nTips: " +
             "\n- default value = 1.0, " +
             "\n- toggle Gizmos visibility in the Scene view.")]
    public float raycastLength = 1;

    [Header("Feet Offset Weight")]
    [Range(0, 1)]
    [Tooltip("Value that controls the weight of the position offset applied to the IK controls transforms. " +
             "\n\nTips: " +
             "\n- default value = 1.0, " +
             "\n- if the value is set to 0, the IK targets transforms position will not be affected.")]
    public float feetPositionOffsetWeight = 1f;

    [Range(0, 1)]
    [Tooltip("Value that controls the weight of the rotation offset applied to the IK controls transforms. " +
             "\n\nTips: " +
             "\n- default value = 0.5, " +
             "\n- if the value is set to 0, IK targets transforms will not be affected.")]
    public float feetRotationOffsetWeight = 1f;

    [Header("Feet Offset Parameters")]
    [Range(0, 5)]
    [Tooltip("Value that controls the time it takes to adjust the feet position relative to the ground height. " +
             "\n\nTips: " +
             "\n- default value = 0.035, " +
             "\n- suggested value should be the same as \"Body Position Offset Smoothing\", " +
             "\n- this value is set to 0, the adjustment will be instant but may produce shaking.")]
    public float feetPositionOffsetSmoothing = 0.035f;

    [Range(0, 5)]
    [Tooltip("Value that controls the time it takes to adjust the feet rotation relative to the ground angle. " +
             "\n\nTips: " +
             "\n- default value = 0.1, " +
             "\n- if this value is set to 0, the adjustment will be instant but may produce shaking.")]
    public float feetRotationOffsetSmoothing = 0.1f;

    [Header("Body Offset Weight")]
    [Range(0, 1)]
    [Tooltip("Value that controls the weight of the position offset applied to the hips transform. " +
             "\n\nTips: " +
             "\n- default value = 1.0, " +
             "\n- if the value is set to 0, the hips transform position will not be affected.")]
    public float bodyPositionOffsetWeight = 1f;

    [Range(0, 1)]
    [Tooltip("Value that controls the weight of the rotation offset applied to the hips transform. " +
             "\n\nTips: " +
             "\n- default value = 0.0, " +
             "\n- if the value is set to 0, the hips transform rotation will not be affected," +
             "\n- using body rotation offset is not recommended for bipedal characters.")]
    public float bodyRotationOffsetWeight = 0f;

    [Header("Body Offset Parameters")]
    [Range(0, 5)]
    [Tooltip("Value that controls the time it takes to offset the \"hips\" transform position relative to the lowest position of the feet. " +
             "\n\nTips: " +
             "\n- default value = 0.035, " +
             "\n- suggested value should be the same as \"Feet Position Offset Smoothing\", " +
             "\n- if this value is set to 0, the adjustment will be instant but may produce shaking.")]
    public float bodyPositionOffsetSmoothing = 0.035f;

    [Range(0, 5)]
    [Tooltip("Value that controls the time it takes to offset the \"hips\" transform rotation relative to the rotation of the ground beneath that transform. " +
             "\n\nTips: " +
             "\n- default value = 0.2, " +
             "\n- if this value is set to 0, the adjustment will be instant but may produce shaking," +
             "\n- using body rotation offset is good for quadrupedal characters, but not for bipedal.")]
    public float bodyRotationOffsetSmoothing = 0.2f;

    [Tooltip("Boolean that controls the direction in which the hips transform position is offset. " +
             "\n\nTips: " +
             "\n- default value = \"false\"," +
             "\n- if set to \"false\", the hips transform will move down with the legs moving down, " +
             "\n- if set to \"true\", the hips transform will move up with the legs moving down.")]
    public bool invertBodyPositionOffset = false;

    [Tooltip("Boolean that controls the direction in which the hips transform rotation is offset. " +
             "\n\nTips: " +
             "\n- default value = \"false\"," +
             "\n- if set to \"false\", the hips transform will rotate with the angle of the ground beneath that transform, " +
             "\n- if set to \"true\", the hips transform will rotate in reverse to angle of the ground beneath that transform," +
             "\n- using body rotation offset is good for quadrupedal characters, but not for bipedal.")]
    public bool invertBodyRotationOffset = false;

    [Header("Stationary to Walk Feet Adjustment")]
    [Range(0, 5)]
    [Tooltip("Value that controls the time it takes to adjust the IK Controls from a stationary state to a walking state. " +
             "\n\nTips: " +
             "\n- default value = 0.025, " +
             "\n- if this value is set to 0, the adjustment will be instant, " +
             "\n- if the \"Stationary To Rotate Smoothing\" value is set to 0, this value will not change anything, " +
             "\n- if the \"Max Stationary Rotation\" value is set to 0 or 360, this value will not change anything.")]
    public float stationaryToWalkSmoothing = 0.2f;

    [Header("Stationary to Rotate Feet Adjustment")]
    [Range(0, 5)]
    [Tooltip("Value that controls the time it takes to adjust the feet position and rotation when in a stationary state but rotated above \"maxStationaryRotationAngle\" value. " +
             "\n\nTips: " +
             "\n- default value = 0.06, " +
             "\n- if this value is set to 0, the adjustment will be instant and make the feet slide, " +
             "\n- if the \"Max Stationary Rotation\" value is set to 0 or 360, this value will not change anything.")]
    public float stationaryToRotateSmoothing = 0.06f;

    [Range(0, 360)]
    [Tooltip("Value that controls the cone angle in which the IK Controls remain in a stationary state. " +
             "\n\nTips: " +
             "\n- default value is 60.0, " +
             "\n- if this value is set to 0 or 360, the adjustment will be instant and make the feet slide, " +
             "\n- if the \"Stationary To Rotate Smoothing\" value set to 0, this value will not change anything, " +
             "\n- toggle Gizmos visibility in the Scene view to visualize the cone.")]
    public float maxStationaryRotationAngle = 60;

    [Tooltip("Boolean that controls the direction of feet adjustment, when rotating the character but not moving. " +
             "\n\nTips: " +
             "\n- default value = \"false\", " +
             "\n- if set to \"false\", the script will start adjustment from the first element of \"IK Constraints\" list when rotating clockwise, " +
             "\n- if set to \"true\", the script will start adjustment from the last element of \"IK Constraints\" list when rotating clockwise, " +
             "\n- be sure to correctly set the elements of \"IK Constraints\" list.")]
    public bool invertAdjustmentDirection = false;

    [Header("Debug Rays")]
    [Tooltip("Boolean that controls the rendering of the debug rays visible only in the editor. " +
             "\n\nTips: " +
             "\n- default value = \"true\", " +
             "\n- if set to \"true\", the script will render colored rays in the editor's scene view, " +
             "\n- if set to \"false\", the script will disable the rendering of these rays.")]
    public bool drawDebugRay = true;

    // Stationary rotation
    [HideInInspector] public float lastStationaryRotation = 0;
    private float stationaryRotation = 0;

    // Control bools
    [HideInInspector] public bool isActive = true;
    [HideInInspector] public bool startup = true;
    [HideInInspector] public bool jumped = false; // Important! This variable should be controlled by another script for character movement!
    [HideInInspector] public bool isMoving = true; // Important! This variable should be controlled by another script for character movement!
    [HideInInspector] public bool isGrounded = true; // Important! This variable should be controlled by another script for character movement!

    // Component references
    private Animator animator;
    private RigBuilder rigBuilder;

    // IAnimationJob elements
    private PlayableGraph rigBuilderGraph;
    private AnimationScriptPlayable animationScriptPlayable;
    private IKFootPlacementJob iKFootPlacementJob;
    #endregion

    // Create new RigBuilder graph when enabled
    private void OnEnable()
    {
        startup = true;
        animator = GetComponent<Animator>();
        rigBuilder = GetComponent<RigBuilder>();

        if (animator == null)
        {
            Debug.LogError("Animator component is missing in this GameObject!");
            DisableIKFootPlacement();
            return;
        }
        if (rigBuilder == null)
        {
            Debug.LogError("Rig Builder component is missing in this GameObject!");
            DisableIKFootPlacement();
            return;
        }

        if (animator != null || rigBuilder != null) { rigBuilder.Build(); }
    }

    // Destroy existing RigBuilder graph and create a new one when disabled
    private void OnDisable()
    {
        if (rigBuilderGraph.IsValid())
        {
            rigBuilderGraph.Destroy();
        }
    }

    // The logic needs to be calculated inside of Update, as it is executed prior to animation calculations
    private void Update()
    {
        if (startup) { Startup(); }

        iKFootPlacementJob = animationScriptPlayable.GetJobData<IKFootPlacementJob>();

        CheckIfLanded();
        CheckParameters();
        CheckRotations();
        SetJobParameters();
        if (isActive) { GetRaycastData(); }

        animationScriptPlayable.SetJobData(iKFootPlacementJob);
    }

    // Create new IKFootPlacementJob and set it's parameters
    private void Startup()
    {
        if (hips == null)
        {
            Debug.LogError("The script is missing required Hips reference!");
            DisableIKFootPlacement();
            return;
        }

        if (iKConstraints.Count == 0)
        {
            Debug.LogError("The script is missing required Two Bone IK constraints!");
            DisableIKFootPlacement();
            return;
        }

        rigBuilderGraph = rigBuilder.graph;

        var existingOutput = rigBuilderGraph.GetOutputByType<AnimationPlayableOutput>(0);

        if (existingOutput.IsOutputValid())
        {
            var existingSource = existingOutput.GetSourcePlayable();

            iKFootPlacementJob = new IKFootPlacementJob();
            if (!iKFootPlacementJob.Create(iKConstraints.Count))
            {
                Debug.LogError("Failed to create new Animation Job!");
                DisableIKFootPlacement();
                return;
            }

            iKFootPlacementJob.hips = animator.BindStreamTransform(hips);
            for (int i = 0; i < iKConstraints.Count; i++)
            {
                iKFootPlacementJob.targets[i] = animator.BindStreamTransform(iKConstraints[i].data.target);
                iKFootPlacementJob.hints[i] = animator.BindStreamTransform(iKConstraints[i].data.hint);
            }

            animationScriptPlayable = AnimationScriptPlayable.Create(rigBuilderGraph, iKFootPlacementJob);
            animationScriptPlayable.AddInput(existingSource, 0, 1.0f);
            existingOutput.SetSourcePlayable(animationScriptPlayable);
        }
        else
        {
            Debug.LogError("Could not find an existing AnimationPlayableOutput in the Rig Builder graph!");
            gameObject.GetComponent<IKFootPlacement>().enabled = false;
        }

        startup = false;
    }

    private void GetRaycastData()
    {
        if (!jumped && isGrounded)
        {
            #region Body Raycast
            RaycastHit bodyRaycastHit = new RaycastHit();
            Vector3 bodyRaycastOrigin = new Vector3(hips.position.x,
                                                    transform.position.y + raycastHeight,
                                                    hips.position.z);

            bool bodyHit = false;

            // Don't cast rays when the all of these weights are equal to 0
            if (bodyPositionOffsetWeight > 0 || bodyRotationOffsetWeight > 0)
            {
                bodyHit = Physics.Raycast(bodyRaycastOrigin, 
                                          transform.TransformDirection(Vector3.down), 
                                          out bodyRaycastHit, 
                                          raycastLength);
            }

            if (bodyHit)
            {
                if (drawDebugRay)
                {
                    float raycastDistance = Vector3.Distance(bodyRaycastOrigin, bodyRaycastHit.point);
                    Debug.DrawLine(bodyRaycastOrigin, bodyRaycastHit.point, Color.blue);
                    Debug.DrawRay(bodyRaycastHit.point, transform.TransformDirection(Vector3.down) * (raycastLength - raycastDistance), Color.white);
                }

                iKFootPlacementJob.bodyRaycastHitPoint = bodyRaycastHit.point;
                iKFootPlacementJob.bodyRaycastOrigin = bodyRaycastOrigin;
                iKFootPlacementJob.bodyRaycastHitNormal = bodyRaycastHit.normal;
            }
            else
            {
                iKFootPlacementJob.bodyRaycastHitPoint = Vector3.zero;
                iKFootPlacementJob.bodyRaycastOrigin = Vector3.zero;
                iKFootPlacementJob.bodyRaycastHitNormal = Vector3.zero;
            }
            #endregion

            #region Legs Raycasts
            for (int i = 0; i < iKConstraints.Count; i++)
            {
                RaycastHit legRaycastHit = new RaycastHit();
                Vector3 legRaycastOrigin = new Vector3(iKConstraints[i].data.target.position.x,
                                                       transform.position.y + raycastHeight,
                                                       iKConstraints[i].data.target.position.z);

                Vector3 footLocalEuler = iKConstraints[i].data.target.localEulerAngles;

                bool legHit = false;

                // Don't cast rays when the all of these weights are equal to 0
                if (feetPositionOffsetWeight > 0 || feetRotationOffsetWeight > 0 || bodyPositionOffsetWeight > 0)
                {
                    legHit = Physics.Raycast(legRaycastOrigin, 
                                             transform.TransformDirection(Vector3.down), 
                                             out legRaycastHit, 
                                             raycastLength);
                }

                if (legHit)
                {
                    if (drawDebugRay)
                    {
                        float raycastDistance = Vector3.Distance(legRaycastOrigin, legRaycastHit.point);
                        Debug.DrawLine(legRaycastOrigin, legRaycastHit.point, Color.red);
                        Debug.DrawRay(legRaycastHit.point, transform.TransformDirection(Vector3.down) * (raycastLength - raycastDistance), Color.white);
                    }

                    iKFootPlacementJob.legRaycastHitPoint[i] = legRaycastHit.point;
                    iKFootPlacementJob.legRaycastOrigin[i] = legRaycastOrigin;
                    iKFootPlacementJob.legRaycastHitNormal[i] = legRaycastHit.normal;
                }
                else
                {
                    iKFootPlacementJob.legRaycastHitPoint[i] = Vector3.zero;
                    iKFootPlacementJob.legRaycastOrigin[i] = Vector3.zero;
                    iKFootPlacementJob.legRaycastHitNormal[i] = Vector3.zero;
                }
            }
            #endregion
        }
        else
        {
            iKFootPlacementJob.bodyRaycastHitPoint = Vector3.zero;
            iKFootPlacementJob.bodyRaycastOrigin = Vector3.zero;
            iKFootPlacementJob.bodyRaycastHitNormal = Vector3.zero;

            for (int i = 0; i < iKConstraints.Count; i++)
            {
                iKFootPlacementJob.legRaycastHitPoint[i] = Vector3.zero;
                iKFootPlacementJob.legRaycastOrigin[i] = Vector3.zero;
                iKFootPlacementJob.legRaycastHitNormal[i] = Vector3.zero;
            }          
        }
    }

    // Additional parameter value check
    private void CheckParameters()
    {
        // Slide feet with these Stationary to Rotate Feet Adjustment values
        if (stationaryToRotateSmoothing <= 0 || maxStationaryRotationAngle == 0 || maxStationaryRotationAngle == 360) { isMoving = true; }
    }

    // Pass parameters values from this script to iKFootPlacementJob struct
    private void SetJobParameters()
    {
        // Feet
        iKFootPlacementJob.feetPositionOffsetSmoothing = feetPositionOffsetSmoothing;
        iKFootPlacementJob.feetRotationOffsetSmoothing = feetRotationOffsetSmoothing;
        iKFootPlacementJob.targetPositionOffsetWeight = feetPositionOffsetWeight;
        iKFootPlacementJob.targetRotationOffsetWeight = feetRotationOffsetWeight;
        // Body
        iKFootPlacementJob.bodyPositionOffsetSmoothing = bodyPositionOffsetSmoothing;
        iKFootPlacementJob.bodyRotationOffsetSmoothing = bodyRotationOffsetSmoothing;
        iKFootPlacementJob.bodyPositionOffsetWeight = bodyPositionOffsetWeight;
        iKFootPlacementJob.bodyRotationOffsetWeight = bodyRotationOffsetWeight;
        iKFootPlacementJob.invertBodyPositionOffset = invertBodyPositionOffset;
        iKFootPlacementJob.invertBodyRotationOffset = invertBodyRotationOffset;
        // Stationary
        iKFootPlacementJob.stationaryToRotateSmoothing = stationaryToRotateSmoothing;
        iKFootPlacementJob.stationaryToWalkSmoothing = stationaryToWalkSmoothing;
        iKFootPlacementJob.maxStationaryRotationAngle = maxStationaryRotationAngle;
        // gameObject position (root object)
        iKFootPlacementJob.rootPosition = transform.position;
        // Bools
        iKFootPlacementJob.isActive = isActive;
        iKFootPlacementJob.isGrounded = isGrounded; // Important! "isGrounded" variable should be controlled by another script for character movement!
        iKFootPlacementJob.isMoving = isMoving; // Important! "isMoving" variable should be controlled by another script for character movement!
        iKFootPlacementJob.jumped = jumped; // Important! "jumped" variable should be controlled by another script for character movement!
        // deltaTime for Lerp (IAnimationJob does not support Time.deltaTime)
        iKFootPlacementJob.deltaTime = Time.deltaTime;
    }

    // Calculate and validate character rotation in order to adjust the feet position
    private void CheckRotations()
    {
        float currentStationaryRotation = transform.eulerAngles.y;

        if (stationaryToRotateSmoothing > 0 && maxStationaryRotationAngle != 0 && maxStationaryRotationAngle != 360 && iKConstraints.Count != 0)
        {
            if (!isMoving && isGrounded)
            {
                // When the character is not making adjustment
                if (!iKFootPlacementJob.adjustFeet)
                {
                    float rotationDifference = Mathf.DeltaAngle(stationaryRotation, currentStationaryRotation);
                    if (rotationDifference < 0)
                    {
                        if (!invertAdjustmentDirection) { iKFootPlacementJob.adjustDirection = "left"; }
                        else { iKFootPlacementJob.adjustDirection = "right"; }
                    }
                    else if (rotationDifference > 0)
                    {
                        if (!invertAdjustmentDirection) { iKFootPlacementJob.adjustDirection = "right"; }
                        else { iKFootPlacementJob.adjustDirection = "left"; }
                    }

                    float lastRotationDifference = Mathf.DeltaAngle(lastStationaryRotation, currentStationaryRotation);
                    if (Mathf.Abs(lastRotationDifference) > maxStationaryRotationAngle / 2f)
                    {
                        lastStationaryRotation = currentStationaryRotation;
                        if (iKFootPlacementJob.adjustDirection == "right") { iKFootPlacementJob.adjustedFoot[0] = true; } // Adjust starting from first element of iKConstraints list
                        else { iKFootPlacementJob.adjustedFoot[iKConstraints.Count - 1] = true; } // Adjust starting from last element of iKConstraints list
                        iKFootPlacementJob.adjustFeet = true;
                    }
                }
                // When the character is already adjusting feet
                else if (iKFootPlacementJob.adjustFeet)
                {
                    float lastRotationDifference = Mathf.DeltaAngle(lastStationaryRotation, currentStationaryRotation);
                    if (Mathf.Abs(lastRotationDifference) > maxStationaryRotationAngle / 2f)
                    {
                        lastStationaryRotation = currentStationaryRotation;
                        for (int i = 0; i < iKConstraints.Count; i++) { iKFootPlacementJob.adjustedFoot[i] = true; }
                        iKFootPlacementJob.adjustFeet = true;
                        iKFootPlacementJob.adjustDirection = "both"; // Adjust all feet
                    }
                }
            }
            // When stationaryToWalkSmoothing > 0, make sure the feet correctly adjust to character rotation
            else if (isMoving && isGrounded && iKFootPlacementJob.lerpSpeed > 0)
            {
                float lastRotationDifference = Mathf.DeltaAngle(lastStationaryRotation, currentStationaryRotation);
                if (Mathf.Abs(lastRotationDifference) > maxStationaryRotationAngle / 4f)
                {
                    lastStationaryRotation = currentStationaryRotation;
                    for (int i = 0; i < iKConstraints.Count; i++) { iKFootPlacementJob.adjustedFoot[i] = true; }
                    iKFootPlacementJob.adjustDirection = "both"; // Adjust all feet
                }
            }
            else { lastStationaryRotation = currentStationaryRotation; }

            if (drawDebugRay)
            {
                Debug.DrawRay(transform.position, transform.forward, Color.yellow);
                Debug.DrawRay(transform.position, Quaternion.Euler(0, lastStationaryRotation + (maxStationaryRotationAngle / 2.0f), 0) * Vector3.forward, Color.red);
                Debug.DrawRay(transform.position, Quaternion.Euler(0, lastStationaryRotation - (maxStationaryRotationAngle / 2.0f), 0) * Vector3.forward, Color.red);
            }
        }
        else { lastStationaryRotation = currentStationaryRotation; }
        stationaryRotation = currentStationaryRotation;
    }

    // Checks if the character has landed after jumping (to avoid beeing one frame late)
    private void CheckIfLanded()
    {
        if (jumped && isGrounded) { jumped = false; } // Important! "jumped", "isGrounded" and "isMoving" variables should be controlled by another script for character movement!
    }

    // Disable the script
    private void DisableIKFootPlacement()
    {
        gameObject.GetComponent<IKFootPlacement>().enabled = false;
    }
}
