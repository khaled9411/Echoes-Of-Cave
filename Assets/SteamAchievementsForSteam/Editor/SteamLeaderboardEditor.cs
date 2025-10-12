using System;
using UnityEditor;
using UnityEngine;

namespace LeastSquares
{

    [CustomEditor(typeof(SteamAchievementsAndStats))]
    [CanEditMultipleObjects]
    public class SteamAchievementsAndStatsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Separator();
            GUILayout.Label("Achievement & Stats Settings", EditorStyles.boldLabel);
            EditorGUILayout.Separator();
            GUILayout.Label("Achievements", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            var achievements = serializedObject.FindProperty("AchievementsIdentifiers");
            EditorGUILayout.PropertyField(achievements, true);
            serializedObject.ApplyModifiedProperties();
            EditorGUI.indentLevel--;
        }
    }
}