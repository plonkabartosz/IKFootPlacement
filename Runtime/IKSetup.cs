// ----------------------
// Copyright (c) 2025 Bartosz Plonka
// Author: Bartosz Plonka
// Year: 2025
// Source: https://github.com/plonkabartosz/IKFootPlacement.git
// License: CC BY 4.0 International (https://creativecommons.org/licenses/by/4.0/)
// ----------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKSetup : MonoBehaviour
{
    // Skeleton
    [Tooltip("If the namings of your bones are the same, then this will fill automatically")]
    [SerializeField] private Transform hips;

    [Tooltip("If the namings of your bones are the same, then this will fill automatically")]
    [SerializeField] private List<IKLeg> leftLegsTransforms = new List<IKLeg>();

    [Tooltip("If the namings of your bones are the same, then this will fill automatically")]
    [SerializeField] private List<IKLeg> rightLegsTransforms = new List<IKLeg>();

    private RigBuilder rigBuilder;
    private Rig rig;

    // Bone namings (can be adjusted to your naming scheme)
    public string hipsName = "Hips";

    public string leftUpLegName = "LeftUpLeg";
    public string leftLegName = "LeftLeg";
    public string leftFootName = "LeftFoot";

    public string rightUpLegName = "RightUpLeg";
    public string rightLegName = "RightLeg";
    public string rightFootName = "RightFoot";

    public bool SetupIKRig()
    {
        #region Safety Checks
        if (hips != null)
        {
            foreach (var leg in leftLegsTransforms)
            {
                if (leg.upLeg == null || leg.leg == null || leg.foot == null)
                {
                    Debug.LogError("Not all of the references are attached. Please try finding references again, or attach each component manually before proceeding.");
                    return false;
                }
            }
            foreach (var leg in rightLegsTransforms)
            {
                if (leg.upLeg == null || leg.leg == null || leg.foot == null)
                {
                    Debug.LogError("Not all of the references are attached. Please try finding references again, or attach each component manually before proceeding.");
                    return false;
                }
            }
        }
        else
        {
            Debug.LogError("Not all of the references are attached. Please try finding references again, or attach each component manually before proceeding.");
            return false;
        }
        #endregion

        #region Adding Rig Builder and Rig components
        try
        {
            rigBuilder = gameObject.AddComponent(typeof(RigBuilder)) as RigBuilder;
            Debug.Log("Added Rig Builder component!");
            rig = gameObject.AddComponent(typeof(Rig)) as Rig;
            Debug.Log("Added Rig component!");
        }
        catch
        {
            Debug.LogError("Could not add Rig Builder and Rig components to the GameObject. Please try running the script again or add them manually.");
            return false;
        }
        #endregion

        #region Building new Rig Builder
        try
        {
            rigBuilder.layers.Clear();
            rigBuilder.layers.Add(new RigLayer(rig, true));
            rigBuilder.enabled = true;
        }
        catch
        {
            Debug.LogError("Could not implement newly added Rig Builder component on the GameObject. Please read console log, remove components that are listed and try running the script again.");

            return false;
        }
        #endregion

        #region Creating IK Controls
        GameObject iKLegs = new GameObject("IK");
        iKLegs.transform.parent = gameObject.transform;
        GameObject iKConstraints = new GameObject("IKConstraints");
        iKConstraints.transform.parent = iKLegs.transform;

        GameObject[] leftLegIKConstraints = new GameObject[leftLegsTransforms.Count];
        GameObject[] rightLegIKConstraints = new GameObject[rightLegsTransforms.Count];
        GameObject leftIKLegs;
        GameObject rightIKLegs;

        if (leftLegsTransforms.Count == 1)
        {
            leftLegIKConstraints[0] = new GameObject(leftLegName + "IKConstraint");
            leftLegIKConstraints[0].transform.parent = iKConstraints.transform;
            leftIKLegs = new GameObject("LeftIKLeg");
        }
        else
        {
            for (int i = 0; i < leftLegsTransforms.Count; i++)
            {
                leftLegIKConstraints[i] = new GameObject(leftLegName + (i + 1) + "IKConstraint");
                leftLegIKConstraints[i].transform.parent = iKConstraints.transform;
            }
            leftIKLegs = new GameObject("LeftIKLegs");
        }

        if (rightLegsTransforms.Count == 1)
        {
            rightLegIKConstraints[0] = new GameObject(rightLegName + "IKConstraint");
            rightLegIKConstraints[0].transform.parent = iKConstraints.transform;
            rightIKLegs = new GameObject("RightIKLeg");
        }
        else
        {
            for (int i = 0; i < rightLegsTransforms.Count; i++)
            {
                rightLegIKConstraints[i] = new GameObject(rightLegName + (i + 1) + "IKConstraint");
                rightLegIKConstraints[i].transform.parent = iKConstraints.transform;
            }
            rightIKLegs = new GameObject("RightIKLegs");
        }


        leftIKLegs.transform.parent = iKLegs.transform;
        GameObject[] leftTargets = new GameObject[leftLegsTransforms.Count];
        GameObject[] leftHints = new GameObject[leftLegsTransforms.Count];
        rightIKLegs.transform.parent = iKLegs.transform;
        GameObject[] rightTargets = new GameObject[rightLegsTransforms.Count];
        GameObject[] rightHints = new GameObject[rightLegsTransforms.Count];

        if (leftLegsTransforms.Count == 1)
        {
            leftTargets[0] = new GameObject(leftLegName + "Target");
            leftHints[0] = new GameObject(leftLegName + "Hint");
            leftTargets[0].transform.parent = leftIKLegs.transform;
            leftTargets[0].transform.position = leftLegsTransforms[0].foot.transform.position;
            leftHints[0].transform.parent = leftIKLegs.transform;
            leftHints[0].transform.position = leftLegsTransforms[0].leg.transform.position;
        }
        else
        {
            for (int i = 0; i < leftLegsTransforms.Count; i++)
            {
                leftTargets[i] = new GameObject(leftLegName + (i + 1) + "Target");
                leftHints[i] = new GameObject(leftLegName + (i + 1) + "Hint");
                leftTargets[i].transform.parent = leftIKLegs.transform;
                leftTargets[i].transform.position = leftLegsTransforms[i].foot.transform.position;
                leftHints[i].transform.parent = leftIKLegs.transform;
                leftHints[i].transform.position = leftLegsTransforms[i].leg.transform.position;
            }
        }
        if (rightLegsTransforms.Count == 1)
        {
            rightTargets[0] = new GameObject(rightLegName + "Target");
            rightHints[0] = new GameObject(rightLegName + "Hint");
            rightTargets[0].transform.parent = rightIKLegs.transform;
            rightTargets[0].transform.position = rightLegsTransforms[0].foot.transform.position;
            rightHints[0].transform.parent = rightIKLegs.transform;
            rightHints[0].transform.position = rightLegsTransforms[0].leg.transform.position;
        }
        else
        {
            for (int i = 0; i < rightLegsTransforms.Count; i++)
            {
                rightTargets[i] = new GameObject(rightLegName + (i + 1) + "Target");
                rightHints[i] = new GameObject(rightLegName + (i + 1) + "Hint");
                rightTargets[i].transform.parent = rightIKLegs.transform;
                rightTargets[i].transform.position = rightLegsTransforms[i].foot.transform.position;
                rightHints[i].transform.parent = rightIKLegs.transform;
                rightHints[i].transform.position = rightLegsTransforms[i].leg.transform.position;
            }
        }

        
        iKLegs.transform.localPosition = Vector3.zero;

        Debug.Log("Created IK Controls!");
        #endregion

        #region Adding IK Constraint components

        TwoBoneIKConstraint[] leftLegsTwoBoneIK = new TwoBoneIKConstraint[leftLegsTransforms.Count];
        TwoBoneIKConstraint[] rightLegsTwoBoneIK = new TwoBoneIKConstraint[rightLegsTransforms.Count];

        for (int i = 0; i < leftLegsTransforms.Count; i++)
        {
            leftLegsTwoBoneIK[i] = leftLegIKConstraints[i].AddComponent(typeof(TwoBoneIKConstraint)) as TwoBoneIKConstraint;
        }
        for (int i = 0; i < rightLegsTransforms.Count; i++)
        {
            rightLegsTwoBoneIK[i] = rightLegIKConstraints[i].AddComponent(typeof(TwoBoneIKConstraint)) as TwoBoneIKConstraint;
        }
        Debug.Log("Added Two Bone Constraint components!");

        for (int i = 0; i < leftLegsTransforms.Count; i++)
        {
            leftLegsTwoBoneIK[i].data.root = leftLegsTransforms[i].upLeg;
            leftLegsTwoBoneIK[i].data.mid = leftLegsTransforms[i].leg;
            leftLegsTwoBoneIK[i].data.tip = leftLegsTransforms[i].foot;
            leftLegsTwoBoneIK[i].data.target = leftTargets[i].transform;
            leftLegsTwoBoneIK[i].data.hint = leftHints[i].transform;
            leftLegsTwoBoneIK[i].data.maintainTargetRotationOffset = true;
        }
        for (int i = 0; i < rightLegsTransforms.Count; i++)
        {
            rightLegsTwoBoneIK[i].data.root = rightLegsTransforms[i].upLeg;
            rightLegsTwoBoneIK[i].data.mid = rightLegsTransforms[i].leg;
            rightLegsTwoBoneIK[i].data.tip = rightLegsTransforms[i].foot;
            rightLegsTwoBoneIK[i].data.target = rightTargets[i].transform;
            rightLegsTwoBoneIK[i].data.hint = rightHints[i].transform;
            rightLegsTwoBoneIK[i].data.maintainTargetRotationOffset = true;
        }
        #endregion

        #region Adding IK Foot Placement component
        IKFootPlacement iKFootPlacement = gameObject.AddComponent(typeof(IKFootPlacement)) as IKFootPlacement;
        iKFootPlacement.hips = hips;
        for (int i = 0; i < leftLegIKConstraints.Length; i++) { iKFootPlacement.iKConstraints.Add(leftLegIKConstraints[i].GetComponent<TwoBoneIKConstraint>()); }
        for (int i = rightLegIKConstraints.Length - 1; i >= 0; i--) { iKFootPlacement.iKConstraints.Add(rightLegIKConstraints[i].GetComponent<TwoBoneIKConstraint>()); }
        #endregion

        Debug.Log("Setup finished successfully!");
        return true;
    }

    public void FindReferences()
    {
        #region Looking for Animator component
        if (gameObject.GetComponent<Animator>() == null)
        {
            Debug.LogError("This GameObject does not have the Animator component attached. Please attach this script to the correct GameObject and try again.");
            return;
        }
        #endregion

        #region Attaching character skeleton
        if (hips == null) { hips = GameObject.Find(hipsName).transform; };
        IKLeg iKLeg;
        if (leftLegsTransforms.Count == 0)
        {
            iKLeg = new IKLeg();
            if (GameObject.Find(leftUpLegName) != null)
            {
                iKLeg.upLeg = GameObject.Find(leftUpLegName).transform;
                if (GameObject.Find(leftLegName) != null)
                {
                    iKLeg.leg = GameObject.Find(leftLegName).transform;
                    if (GameObject.Find(leftFootName) != null)
                    {
                        iKLeg.foot = GameObject.Find(leftFootName).transform;
                        leftLegsTransforms.Add(iKLeg);
                    }
                }
            }

            int i = 1;
            while (true)
            {
                iKLeg = new IKLeg();
                if (GameObject.Find(leftUpLegName + i) != null)
                {
                    iKLeg.upLeg = GameObject.Find(leftUpLegName + i).transform;
                }
                else break;
                if (GameObject.Find(leftLegName + i) != null)
                {
                    iKLeg.leg = GameObject.Find(leftLegName + i).transform;
                }
                else break;
                if (GameObject.Find(leftFootName + i) != null)
                {
                    iKLeg.foot = GameObject.Find(leftFootName + i).transform;
                }
                else break;
                leftLegsTransforms.Add(iKLeg);
                i++;
            }
        }
        if (rightLegsTransforms.Count == 0)
        {
            iKLeg = new IKLeg();
            if (GameObject.Find(rightUpLegName) != null)
            {
                iKLeg.upLeg = GameObject.Find(rightUpLegName).transform;
                if (GameObject.Find(rightLegName) != null)
                {
                    iKLeg.leg = GameObject.Find(rightLegName).transform;
                    if (GameObject.Find(rightFootName) != null)
                    {
                        iKLeg.foot = GameObject.Find(rightFootName).transform;
                        rightLegsTransforms.Add(iKLeg);
                    }
                }
            }

            int i = 1;
            while (true)
            {
                iKLeg = new IKLeg();
                if (GameObject.Find(rightUpLegName + i) != null)
                {
                    iKLeg.upLeg = GameObject.Find(rightUpLegName + i).transform;
                }
                else break;
                if (GameObject.Find(rightLegName + i) != null)
                {
                    iKLeg.leg = GameObject.Find(rightLegName + i).transform;
                }
                else break;
                if (GameObject.Find(rightFootName + i) != null)
                {
                    iKLeg.foot = GameObject.Find(rightFootName + i).transform;
                }
                else break;
                rightLegsTransforms.Add(iKLeg);
                i++;
            }
        }
        if (rightLegsTransforms.Count == 0 && leftLegsTransforms.Count == 0)
        {
            Debug.LogError("Could not automatically detect character's legs references. Be sure that the namings of the joints are correct. " +
                           "Try reseting IK Setup script or manully assign each reference.");
            return;
        }
        #endregion

        Debug.Log("All references found successfully!");
    }

}