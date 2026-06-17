using UnityEngine;
using UnityEditor;

namespace VolumetricFogAndMist2 {

    [CustomEditor(typeof(PointLightManager))]
    public class PointLightManagerEditor : Editor {

        public override void OnInspectorGUI() {
            EditorGUILayout.HelpBox("为了在体积雾与迷雾2中使用快速点光源，请将FogPointLight脚本添加到所需的点光源上（仅点光源管理器需要此操作；如果您使用'原生光源'选项，则完全不需要）。In order to use fast point lights with Volumetric Fog & Mist 2, add a FogPointLight script to the desired point lights (only required by the point light manager; if you're using the option 'Native Lights' this is not required at all).", MessageType.Info);
            DrawDefaultInspector();
        }
    }

}