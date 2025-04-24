// ----------------------
// Copyright (c) 2025 Bartosz Plonka
// Author: Bartosz Plonka
// Year: 2025
// Source: https://github.com/plonkabartosz/IKFootPlacement.git
// License: MIT License (https://opensource.org/license/mit)
// ----------------------

using UnityEngine;
using UnityEngine.Animations;

public struct IKFootPlacementJob : IAnimationJob
{
    #region Struct Variables
    // IK Targets
    public TransformStreamHandle[] targets;
    
    public float targetPositionOffsetWeight;
    public float targetRotationOffsetWeight;

    private Vector3[] targetPositions;
    private float[] targetPositionOffsets;
    private Quaternion[] targetRotations;
    private Quaternion[] targetRotationOffsets;

    // IK Hints
    public TransformStreamHandle[] hints;
    private Vector3[] hintPositions;

    // Body
    public TransformStreamHandle hips;
    public Vector3 rootPosition;

    public float bodyPositionOffsetWeight;
    public float bodyRotationOffsetWeight;
    public bool invertBodyPositionOffset;
    public bool invertBodyRotationOffset;

    private float bodyPositionOffset;
    private Quaternion bodyRotations;
    private Quaternion bodyRotationOffset;

    // Parameters
    public float feetPositionOffsetSmoothing;
    public float feetRotationOffsetSmoothing;

    public float bodyPositionOffsetSmoothing;
    public float bodyRotationOffsetSmoothing;

    public float stationaryToRotateSmoothing;
    public float stationaryToWalkSmoothing;

    public float maxStationaryRotationAngle;
    public float deltaTime;

    // Raycast
    public Vector3 bodyRaycastHitPoint;
    public Vector3 bodyRaycastHitNormal;
    public Vector3 bodyRaycastOrigin;

    public Vector3[] legRaycastHitPoint;
    public Vector3[] legRaycastHitNormal;
    public Vector3[] legRaycastOrigin;

    private Vector3 lowestLegRaycastHitPoint;

    // Control bools
    public bool isActive;
    public bool isGrounded; // Important! "IKFootPlacement.isGrounded" variable should be controlled by another script for character movement!
    public bool isMoving; // Important! "IKFootPlacement.isMoving" variable should be controlled by another script for character movement!
    public bool jumped; // Important! "IKFootPlacement.jumped" variable should be controlled by another script for character movement!
    public bool adjustFeet;
    public bool[] adjustedFoot;
    private bool startup;

    // Other
    public string adjustDirection;
    public float lerpSpeed;
    #endregion

    public void ProcessRootMotion(AnimationStream stream) { } // Leave empty

    public void ProcessAnimation(AnimationStream stream)
    {
        if (startup) { isMoving = true; }

        lowestLegRaycastHitPoint = new Vector3(0, rootPosition.y, 0);

        CalculateLerpSpeed();

        if (isActive)
        {
            OffsetTarget(stream);
            CheckFeetAdjustment();
            if (bodyRotationOffsetWeight > 0) { OffsetBodyRotation(stream); }

            startup = false;
        }
        else { startup = true; }

        if (bodyPositionOffsetWeight > 0) { OffsetBodyPosition(stream); }
    }


    private void OffsetTarget(AnimationStream stream)
    {
        if (adjustDirection == "left")
        {
            for (int i = targets.Length - 1; i >= 0; i--)
            {
                CalculateTargetOffsetPosition(stream, i);
                CalculateTargetOffsetRotation(stream, i);
                CalculateHintOffsetPosition(stream, i);
            }
        }
        else
        {
            for (int i = 0; i < targets.Length; i++)
            {
                CalculateTargetOffsetPosition(stream, i);
                CalculateTargetOffsetRotation(stream, i);
                CalculateHintOffsetPosition(stream, i);
            }
        }
    }

    private void CalculateTargetOffsetPosition(AnimationStream stream, int i)
    {
        #region Get position from animation
        if (startup) { targetPositions[i] = targets[i].GetPosition(stream); }
        if (isMoving || !isGrounded)
        {
            if (lerpSpeed != 0)
            {
                targetPositions[i].y = targets[i].GetPosition(stream).y;
                targetPositions[i] = Vector3.Lerp(targetPositions[i], 
                                                  targets[i].GetPosition(stream), 
                                                  deltaTime / (stationaryToWalkSmoothing * lerpSpeed));
            }
            else { targetPositions[i] = targets[i].GetPosition(stream); }
        }
        else if (!isMoving && isGrounded)
        {
            if (adjustedFoot[i])
            {
                targetPositions[i].y = targets[i].GetPosition(stream).y;
                float distanceToTaget = Vector3.Distance(targetPositions[i], targets[i].GetPosition(stream));
                if (distanceToTaget > 0.025f)
                {
                    targetPositions[i] = Vector3.Lerp(targetPositions[i], 
                                                      targets[i].GetPosition(stream), 
                                                      deltaTime / stationaryToRotateSmoothing);
                    adjustFeet = false;
                }
            }
            else { targetPositions[i].y = targets[i].GetPosition(stream).y; }
        }
        else { targetPositions[i] = targets[i].GetPosition(stream); }
        #endregion

        #region Calculate and apply position offset
        float footPositionOffset = 0;
        if (targetPositionOffsetWeight <= 0)
        {
            targetPositionOffsets[i] = 0;

            // Check if RaycastHit point and origin are not Vector3.zero
            if (legRaycastHitPoint[i].y != 0 || legRaycastOrigin[i].y != 0)
            {
                if (legRaycastHitPoint[i].y < lowestLegRaycastHitPoint.y) { lowestLegRaycastHitPoint = legRaycastHitPoint[i]; }
            }
        }
        else
        {
            if (isGrounded)
            {
                // Check if RaycastHit point and origin are not Vector3.zero
                if (legRaycastHitPoint[i].y != 0 || legRaycastOrigin[i].y != 0)
                {
                    if (legRaycastHitPoint[i].y < lowestLegRaycastHitPoint.y) { lowestLegRaycastHitPoint = legRaycastHitPoint[i]; }

                    legRaycastOrigin[i] = new Vector3(targetPositions[i].x, legRaycastOrigin[i].y, targetPositions[i].z);
                    legRaycastHitPoint[i] = new Vector3(targetPositions[i].x, legRaycastHitPoint[i].y, targetPositions[i].z);

                    float distanceToIKFoot = Vector3.Distance(legRaycastOrigin[i], targetPositions[i]) + (targetPositions[i].y - rootPosition.y);
                    float distanceToRaycastHit = Vector3.Distance(legRaycastOrigin[i], legRaycastHitPoint[i]);

                    if (distanceToRaycastHit < distanceToIKFoot - 0.001f ||
                       distanceToRaycastHit > distanceToIKFoot + 0.001f)
                    {
                        footPositionOffset = distanceToIKFoot - distanceToRaycastHit;
                    }
                }
            }

            if (targetPositionOffsetWeight != 1) { footPositionOffset = Mathf.Lerp(0, footPositionOffset, targetPositionOffsetWeight); }
            if (feetPositionOffsetSmoothing > 0) 
            { 
                targetPositionOffsets[i] = Mathf.Lerp(targetPositionOffsets[i], 
                                                      footPositionOffset, 
                                                      deltaTime / feetPositionOffsetSmoothing); 
            }
            else { targetPositionOffsets[i] = footPositionOffset; }
        }

        targetPositions[i].y += targetPositionOffsets[i];
        targets[i].SetPosition(stream, targetPositions[i]);
        #endregion
    }

    private void CalculateTargetOffsetRotation(AnimationStream stream, int i)
    {
        #region Get rotation from animation
        if (startup) { targetRotationOffsets[i] = targets[i].GetRotation(stream); }
        if (isMoving || !isGrounded)
        {
            if (lerpSpeed != 0) 
            { 
                targetRotations[i] = Quaternion.Lerp(targetRotations[i], 
                                                     targets[i].GetRotation(stream), 
                                                     deltaTime / (stationaryToWalkSmoothing * lerpSpeed)); 
            }
            else { targetRotations[i] = targets[i].GetRotation(stream); }
        }
        else if (!isMoving && isGrounded)
        {
            if (adjustedFoot[i])
            {
                if (!adjustFeet)
                {
                    targetRotations[i] = Quaternion.Lerp(targetRotations[i],
                                                         targets[i].GetRotation(stream),
                                                         deltaTime / stationaryToRotateSmoothing);
                }         
            }
        }
        else { targetRotationOffsets[i] = targets[i].GetRotation(stream); }
        #endregion

        #region Calculate and apply rotation offset
        Quaternion targetRotationOffset;

        if (targetRotationOffsetWeight <= 0) { targetRotationOffsets[i] = Quaternion.identity; }
        else
        {
            targetRotationOffset = Quaternion.FromToRotation(Vector3.up, legRaycastHitNormal[i]);
            if (targetRotationOffsetWeight != 1)
            {
                targetRotationOffset = Quaternion.Slerp(Quaternion.identity, 
                                                        targetRotationOffset, 
                                                        targetRotationOffsetWeight);
            }
            if (feetRotationOffsetSmoothing > 0 && !startup)
            {
                targetRotationOffsets[i] = Quaternion.Lerp(targetRotationOffsets[i],
                                                           targetRotationOffset,
                                                           deltaTime / feetRotationOffsetSmoothing);
            }
            else { targetRotationOffsets[i] = targetRotationOffset; }
        }

        targetRotationOffset = targetRotationOffsets[i] * targetRotations[i];
        targets[i].SetRotation(stream, targetRotationOffset);
        #endregion
    }

    private void CalculateHintOffsetPosition(AnimationStream stream, int i)
    {
        #region Get position from animation
        if (startup) { hintPositions[i] = hints[i].GetPosition(stream); }
        if (isMoving || !isGrounded)
        {
            if (lerpSpeed != 0)
            {
                hintPositions[i] = Vector3.Lerp(hintPositions[i],
                                                hints[i].GetPosition(stream),
                                                deltaTime / (stationaryToWalkSmoothing * lerpSpeed));
                hintPositions[i].y = hints[i].GetPosition(stream).y;
            }
            else { hintPositions[i] = hints[i].GetPosition(stream); }
        }
        else if (!isMoving && isGrounded)
        {
            if (adjustedFoot[i])
            {
                if (!adjustFeet)
                {
                    hintPositions[i] = Vector3.Lerp(hintPositions[i],
                                                    hints[i].GetPosition(stream),
                                                    deltaTime / stationaryToRotateSmoothing);
                }
            }
            hintPositions[i].y = hints[i].GetPosition(stream).y;
        }
        else { hintPositions[i] = hints[i].GetPosition(stream); }
        #endregion

        #region Apply position offset
        hints[i].SetPosition(stream, hintPositions[i]);
        #endregion
    }

    private void OffsetBodyPosition(AnimationStream stream)
    {
        #region Get position from animation
        float bodyPositionOffestTarget = 0;
        if (isGrounded)
        {
            if (!invertBodyPositionOffset) { bodyPositionOffestTarget = (rootPosition.y - lowestLegRaycastHitPoint.y); }
            else { bodyPositionOffestTarget = (rootPosition.y - lowestLegRaycastHitPoint.y) * -1; }
        }
        if (isActive)
        {
            if (bodyPositionOffsetSmoothing > 0)
            {
                bodyPositionOffset = Mathf.Lerp(bodyPositionOffset,
                                                bodyPositionOffestTarget,
                                                deltaTime / bodyPositionOffsetSmoothing);
            }
            else { bodyPositionOffset = bodyPositionOffestTarget; }
        }
        else
        {
            if (bodyPositionOffsetSmoothing > 0 && bodyPositionOffset != 0)
            {
                bodyPositionOffset = Mathf.Lerp(bodyPositionOffset,
                                                0,
                                                deltaTime / bodyPositionOffsetSmoothing);
            }
            else { bodyPositionOffset = 0; }
        }
        #endregion

        #region Calculate and apply position offset
        Vector3 currentBodyPosition = hips.GetPosition(stream);
        currentBodyPosition.y -= bodyPositionOffset * bodyPositionOffsetWeight;
        hips.SetPosition(stream, currentBodyPosition);
        #endregion
    }

    private void OffsetBodyRotation(AnimationStream stream)
    {
        #region Get rotation from animation
        bodyRotations = hips.GetRotation(stream);
        if (startup) { bodyRotationOffset = Quaternion.identity; }
        #endregion

        #region Calculate and apply rotation offset
        Quaternion targetRotationOffset = Quaternion.FromToRotation(Vector3.up, bodyRaycastHitNormal);
        if (invertBodyRotationOffset) { targetRotationOffset = Quaternion.Inverse(targetRotationOffset); }

        if (feetRotationOffsetSmoothing > 0)
        {
            bodyRotationOffset = Quaternion.Lerp(bodyRotationOffset,
                                                  targetRotationOffset,
                                                  deltaTime / bodyRotationOffsetSmoothing);
        }
        else { bodyRotationOffset = targetRotationOffset; }

        targetRotationOffset = (bodyRotationOffset * bodyRotations);

        if (bodyRotationOffsetWeight != 1)
        {
            targetRotationOffset = Quaternion.Slerp(bodyRotations, 
                                                    targetRotationOffset, 
                                                    bodyRotationOffsetWeight);
        }

        Vector3 hipsEuler = hips.GetLocalRotation(stream).eulerAngles;
        hips.SetRotation(stream, targetRotationOffset);
        Vector3 currentHipsEuler = hips.GetLocalRotation(stream).eulerAngles;
        currentHipsEuler.y = hipsEuler.y;
        currentHipsEuler.z = hipsEuler.z;
        hips.SetLocalRotation(stream, Quaternion.Euler(currentHipsEuler));
        #endregion
    }

    // Feet adjustment logic when rotating but not moving
    private void CheckFeetAdjustment()
    {
        // When not moving but adjusting, check if any of the feet are still adjusting
        if (!adjustFeet && !isMoving)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (adjustedFoot[i]) { adjustFeet = true; }
            }
        }
        // Check if adjustment should continue or stop
        else if (adjustFeet && !isMoving)
        {
            // Adjusting from last to first TwoBoneIKConstraint target
            if (adjustDirection == "left")
            {
                for (int i = targets.Length - 1; i >= 0; i--)
                {
                    if (adjustedFoot[i])
                    {
                        adjustedFoot[i] = false;
                        // Set next foot to adjust
                        if (i - 1 >= 0)
                        {
                            adjustedFoot[i - 1] = true;
                            break;
                        }
                        // Stop adjustment
                        else { adjustFeet = false; }
                    }
                }
            }
            // Adjusting from first to last TwoBoneIKConstraint target
            else if (adjustDirection == "right")
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (adjustedFoot[i])
                    {
                        adjustedFoot[i] = false;
                        // Set next foot to adjust
                        if (i + 1 < targets.Length)
                        {
                            adjustedFoot[i + 1] = true;
                            break;
                        }
                        // Stop adjustment
                        else { adjustFeet = false; }
                    }
                }
            }
            // Stop adjustment if all feet are done adjusting
            else
            {
                for (int i = 0; i < targets.Length; i++) { adjustedFoot[i] = false; }
                adjustFeet = false;
            }
        }
    }

    // When blending from stationary to walking state, LerpSpeed is to help make sure that this blend will end and not lerp forever
    private void CalculateLerpSpeed()
    {
        if (isMoving || !isGrounded)
        {
            if (adjustFeet)
            {
                adjustFeet = false;
                lerpSpeed = 1;
            }
            if (stationaryToWalkSmoothing > 0 && lerpSpeed > 0.001f && isGrounded && !jumped)
            {
                lerpSpeed = Mathf.Lerp(lerpSpeed, 0, deltaTime / (stationaryToWalkSmoothing / 2f));
            }
            else if (stationaryToWalkSmoothing > 0 && lerpSpeed > 0.001 && !isGrounded && jumped)
            {
                lerpSpeed = Mathf.Lerp(lerpSpeed, 0, deltaTime / (stationaryToWalkSmoothing / 4f));
            }
            else lerpSpeed = 0;
        }
        else lerpSpeed = 1;
    }

    // Create arrays and set values to start with
    public bool Create(int length)
    {
        hips = new TransformStreamHandle();
        targets = new TransformStreamHandle[length];
        targetPositions = new Vector3[length];
        targetPositionOffsets = new float[length];
        targetRotations = new Quaternion[length];
        targetRotationOffsets = new Quaternion[length];

        legRaycastOrigin = new Vector3[length];
        legRaycastHitPoint = new Vector3[length];
        legRaycastHitNormal = new Vector3[length];

        hints = new TransformStreamHandle[length];
        hintPositions = new Vector3[length];

        bodyPositionOffset = 0;

        isActive = true;
        isGrounded = true;
        isMoving = true;
        adjustFeet = false;
        adjustedFoot = new bool[length];

        startup = true;
        return true;
    }

    // Reset values to zero
    public void ResetValues()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            legRaycastOrigin[i] = Vector3.zero;
            legRaycastHitPoint[i] = Vector3.zero;
            legRaycastHitNormal[i] = Vector3.zero;
            bodyRaycastOrigin = Vector3.zero;
            bodyRaycastHitPoint = Vector3.zero;
            bodyRaycastHitNormal = Vector3.zero;
            rootPosition = Vector3.zero;
            bodyPositionOffset = 0;
            bodyRotationOffset = Quaternion.identity;
            lerpSpeed = 1;
        }
    }
}
