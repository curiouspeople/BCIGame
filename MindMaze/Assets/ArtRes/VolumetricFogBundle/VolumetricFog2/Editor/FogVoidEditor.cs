using UnityEngine;
using UnityEditor;

namespace VolumetricFogAndMist2 {

    [CustomEditor(typeof(FogVoid))]
    public class FogVoidEditor : Editor {

        SerializedProperty falloff, roundness;

        private void OnEnable() {
            falloff = serializedObject.FindProperty("falloff");
            roundness = serializedObject.FindProperty("roundness");
        }


        public override void OnInspectorGUI() {

            serializedObject.Update();

            EditorGUILayout.PropertyField(roundness, new GUIContent("Roundness 圆度"));
            EditorGUILayout.PropertyField(falloff, new GUIContent("Falloff 衰减"));

            serializedObject.ApplyModifiedProperties();

        }
    }

}