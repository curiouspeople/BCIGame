using UnityEngine;
using UnityEditor;

namespace VolumetricFogAndMist2 {

    [CustomEditor(typeof(FogPointLight))]
    public class FogPointLightEditor : Editor {


        SerializedProperty inscattering, intensity;

        private void OnEnable() {
            inscattering = serializedObject.FindProperty("inscattering");
            intensity = serializedObject.FindProperty("intensity");
        }


        public override void OnInspectorGUI() {

            EditorGUILayout.HelpBox("仅针对此点光源的自定义倍增值。使用点光源管理器管理全局设置。Custom multipliers for this point light only. Manage global settings using the Point Light Manager.", MessageType.Info);
            if (GUILayout.Button("Open Point Light Manager")) {
                Selection.activeGameObject = VolumetricFogManager.pointLightManager.gameObject;
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.Separator();

            serializedObject.Update();
            EditorGUILayout.PropertyField(inscattering, new GUIContent("Inscattering 内散射"));
            EditorGUILayout.PropertyField(intensity, new GUIContent("Intensity 强度"));
            serializedObject.ApplyModifiedProperties();
        }
    }

}