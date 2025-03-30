// ----------------------
// Copyright (c) 2025 Bartosz Plonka
// Author: Bartosz Plonka
// Year: 2025
// Source: https://github.com/plonkabartosz/IKFootPlacement.git
// License: CC BY 4.0 International (https://creativecommons.org/licenses/by/4.0/)
// ----------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IKSetup))]
public class IKSetupGUI : Editor
{
    //Skeleton
    SerializedProperty hips;
    SerializedProperty leftLegsTransforms;
    SerializedProperty rightLegsTransforms;

    void OnEnable()
    {
        hips = serializedObject.FindProperty("hips");
        leftLegsTransforms = serializedObject.FindProperty("leftLegsTransforms");
        rightLegsTransforms = serializedObject.FindProperty("rightLegsTransforms");
    }

    public override void OnInspectorGUI()
    {
        IKSetup iKSetup = (IKSetup)target;

        GUIStyle title = new GUIStyle();
        title.fontStyle = FontStyle.Bold;
        title.normal.textColor = Color.white;
        title.wordWrap = true;

        serializedObject.Update();

        #region Labels
        GUILayout.Label("This script runs automatically.", title);
        GUILayout.Space(10);
        GUILayout.Label("Please ensure that:", title);
        GUILayout.Space(5);
        GUILayout.Label("1. This project has Animation Rigging package installed,", title);
        GUILayout.Space(5);
        GUILayout.Label("2. This GameObject has Animator component attached,", title);
        GUILayout.Space(5);
        GUILayout.Label("3. Character bones use the exact namings.", title);
        GUILayout.Space(5);
        GUILayout.Label(" 3.1. For bipedal characters:", title);
        GUILayout.Space(5);
        GUILayout.Label(" " + iKSetup.hipsName);
        GUILayout.Label("   " + iKSetup.leftUpLegName);
        GUILayout.Label("       " + iKSetup.leftLegName);
        GUILayout.Label("           " + iKSetup.leftFootName);
        GUILayout.Label("   " + iKSetup.rightUpLegName);
        GUILayout.Label("       " + iKSetup.rightLegName);
        GUILayout.Label("           " + iKSetup.rightFootName);
        GUILayout.Space(5);
        GUILayout.Space(5);
        GUILayout.Label(" 3.2. For other characters:", title);
        GUILayout.Space(5);
        GUILayout.Label(" " + iKSetup.hipsName);
        GUILayout.Label("   " + iKSetup.leftUpLegName + "1");
        GUILayout.Label("       " + iKSetup.leftLegName + "1");
        GUILayout.Label("           " + iKSetup.leftFootName + "1");
        GUILayout.Label("   " + iKSetup.leftUpLegName + "2");
        GUILayout.Label("       " + iKSetup.leftLegName + "2");
        GUILayout.Label("           " + iKSetup.leftFootName + "2");
        GUILayout.Label("   (...)");
        GUILayout.Label("   " + iKSetup.rightUpLegName + "1");
        GUILayout.Label("       " + iKSetup.rightLegName + "1");
        GUILayout.Label("           " + iKSetup.rightFootName + "1");
        GUILayout.Label("   (...)");
        GUILayout.Space(5);
        GUILayout.Space(10);

        GUILayout.Label("If the requirements have been met, You can press Find References button while in the SCENE VIEW.", title);
        #endregion

        GUILayout.Space(10);

        if (GUILayout.Button("Find References"))
        {
            iKSetup.FindReferences();
        }

        GUILayout.Space(10);

        #region References
        GUILayout.Label("References", title);
        EditorGUILayout.PropertyField(hips);
        EditorGUILayout.PropertyField(leftLegsTransforms, true);
        EditorGUILayout.PropertyField(rightLegsTransforms, true);
        #endregion

        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10);

        GUILayout.Label("If all of the references are attached, proceed by pressing Start Setup button in the scene view, or the isolated view of Your prefab.", title);
        GUILayout.Space(5);
        GUILayout.Label("If not, try attaching each reference manually and press Start Setup button again.", title);

        GUILayout.Space(10);

        if (GUILayout.Button("Start Setup"))
        {
            iKSetup.SetupIKRig();
        }
    }
}
