using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;
using System.Reflection;
using UnityEditor.IMGUI.Controls;

namespace VolumetricFogAndMist2 {

    [CustomEditor(typeof(VolumetricFog))]
    public partial class VolumetricFogEditor : Editor {

        VolumetricFogProfile cachedProfile;
        Editor cachedProfileEditor;
        SerializedProperty profile;
        SerializedProperty maskEditorEnabled, maskBrushMode, maskBrushColor, maskBrushWidth, maskBrushFuzziness, maskBrushOpacity;
        SerializedProperty enablePointLights, enableNativeLights, nativeLightsMultiplier, enableAPV, apvIntensityMultiplier;
        SerializedProperty enableVoids;
        SerializedProperty enableFogOfWar, fogOfWarCenter, fogOfWarIsLocal, fogOfWarSize, fogOfWarShowCoverage, fogOfWarTextureWidth, fogOfWarTextureHeight, fogOfWarRestoreDelay, fogOfWarRestoreDuration, fogOfWarSmoothness, fogOfWarBlur;
        SerializedProperty enableFollow, followTarget, followMode, followOffset, followIncludeDistantFog;
        SerializedProperty enableFade, fadeDistance, fadeOut, fadeController, enableSubVolumes, subVolumes;
        SerializedProperty enableUpdateModeOptions, updateMode, updateModeCamera, updateModeBounds;
        SerializedProperty showBoundary;

        static GUIStyle boxStyle;
        VolumetricFog fog;
        public static VolumetricFog lastEditingFog;

        void OnEnable () {
            profile = serializedObject.FindProperty("profile");

            enablePointLights = serializedObject.FindProperty("enablePointLights");
            enableNativeLights = serializedObject.FindProperty("enableNativeLights");
            nativeLightsMultiplier = serializedObject.FindProperty("nativeLightsMultiplier");
            enableAPV = serializedObject.FindProperty("enableAPV");
            apvIntensityMultiplier = serializedObject.FindProperty("apvIntensityMultiplier");
            enableVoids = serializedObject.FindProperty("enableVoids");
            enableFogOfWar = serializedObject.FindProperty("enableFogOfWar");
            fogOfWarCenter = serializedObject.FindProperty("fogOfWarCenter");
            fogOfWarIsLocal = serializedObject.FindProperty("fogOfWarIsLocal");
            fogOfWarSize = serializedObject.FindProperty("fogOfWarSize");
            fogOfWarShowCoverage = serializedObject.FindProperty("fogOfWarShowCoverage");
            fogOfWarTextureWidth = serializedObject.FindProperty("fogOfWarTextureWidth");
            fogOfWarTextureHeight = serializedObject.FindProperty("fogOfWarTextureHeight");
            fogOfWarRestoreDelay = serializedObject.FindProperty("fogOfWarRestoreDelay");
            fogOfWarRestoreDuration = serializedObject.FindProperty("fogOfWarRestoreDuration");
            fogOfWarSmoothness = serializedObject.FindProperty("fogOfWarSmoothness");
            fogOfWarBlur = serializedObject.FindProperty("fogOfWarBlur");

            maskEditorEnabled = serializedObject.FindProperty("maskEditorEnabled");
            maskBrushColor = serializedObject.FindProperty("maskBrushColor");
            maskBrushMode = serializedObject.FindProperty("maskBrushMode");
            maskBrushWidth = serializedObject.FindProperty("maskBrushWidth");
            maskBrushFuzziness = serializedObject.FindProperty("maskBrushFuzziness");
            maskBrushOpacity = serializedObject.FindProperty("maskBrushOpacity");

            enableFollow = serializedObject.FindProperty("enableFollow");
            followTarget = serializedObject.FindProperty("followTarget");
            followMode = serializedObject.FindProperty("followMode");
            followOffset = serializedObject.FindProperty("followOffset");
            followIncludeDistantFog = serializedObject.FindProperty("followIncludeDistantFog");

            enableFade = serializedObject.FindProperty("enableFade");
            fadeDistance = serializedObject.FindProperty("fadeDistance");
            fadeOut = serializedObject.FindProperty("fadeOut");
            fadeController = serializedObject.FindProperty("fadeController");
            enableSubVolumes = serializedObject.FindProperty("enableSubVolumes");
            subVolumes = serializedObject.FindProperty("subVolumes");
            enableUpdateModeOptions = serializedObject.FindProperty("enableUpdateModeOptions");
            updateMode = serializedObject.FindProperty("updateMode");
            updateModeCamera = serializedObject.FindProperty("updateModeCamera");
            updateModeBounds = serializedObject.FindProperty("updateModeBounds");
            showBoundary = serializedObject.FindProperty("showBoundary");

            fog = (VolumetricFog)target;
            lastEditingFog = fog;
        }


        public override void OnInspectorGUI () {

            var pipe = GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            if (pipe == null) {
                EditorGUILayout.HelpBox("通用渲染管线资源未在项目设置/图形中设置！Universal Rendering Pipeline asset is not set in Project Settings / Graphics !", MessageType.Error);
                return;
            }

            if (!pipe.supportsCameraDepthTexture) {
                EditorGUILayout.HelpBox("需要在通用渲染管线资源中启用深度纹理选项！Depth Texture option is required in Universal Rendering Pipeline asset!", MessageType.Error);
                if (GUILayout.Button("Go to Universal Rendering Pipeline Asset 前往通用渲染管线资源")) {
                    Selection.activeObject = pipe;
                }
                EditorGUILayout.Separator();
                GUI.enabled = false;
            }

            // Check depth texture mode
            FieldInfo renderers = pipe.GetType().GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);
            if (renderers == null) return;
            foreach (var renderer in (object[])renderers.GetValue(pipe)) {
                if (renderer == null) continue;
                FieldInfo depthTextureModeField = renderer.GetType().GetField("m_CopyDepthMode", BindingFlags.NonPublic | BindingFlags.Instance);
                if (depthTextureModeField != null) {
                    int depthTextureMode = (int)depthTextureModeField.GetValue(renderer);
                    if (depthTextureMode == 1) { // transparent copy depth mode
                        EditorGUILayout.HelpBox("URP资源中的深度纹理模式必须设置为'After Opaques'或'Force Prepass'。Depth Texture Mode in URP asset must be set to 'After Opaques' or 'Force Prepass'.", MessageType.Warning);
                        if (GUILayout.Button("Show Pipeline Asset 显示管线资源")) {
                            Selection.activeObject = (Object)renderer;
                            GUIUtility.ExitGUI();
                        }
                        EditorGUILayout.Separator();
                    }
                }
            }

            if (boxStyle == null) {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.padding = new RectOffset(15, 10, 5, 5);
            }

            serializedObject.Update();

			EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(profile, new GUIContent("Profile 配置文件"));

            if (profile.objectReferenceValue != null) {
                if (cachedProfile != profile.objectReferenceValue) {
                    cachedProfile = null;
                }
                if (cachedProfile == null) {
                    cachedProfile = (VolumetricFogProfile)profile.objectReferenceValue;
                    cachedProfileEditor = CreateEditor(profile.objectReferenceValue);
                }

                // Drawing the profile editor
                EditorGUILayout.BeginVertical(boxStyle);
                cachedProfileEditor.OnInspectorGUI();
                EditorGUILayout.EndVertical();
            } else {
                EditorGUILayout.HelpBox("Create or assign a fog profile. 创建或指定一个雾配置文件", MessageType.Info);
                if (GUILayout.Button("New Fog Profile 新建雾配置")) {
                    CreateFogProfile();
                }
            }
            if (EditorGUI.EndChangeCheck()) {
                if (!Application.isPlaying) {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                }
            }

            EditorGUIUtility.labelWidth = 170;
            EditorGUILayout.PropertyField(enableNativeLights, new GUIContent("Enable Native Lights 启用原生光照"));
            if (enableNativeLights.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(nativeLightsMultiplier, new GUIContent("Intensity Multiplier 强度倍数"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(enableAPV, new GUIContent("Enable APV (Probe Volumes) 启用APV(探针体积)"));
            if (enableAPV.boolValue) {
                EditorGUI.indentLevel++;
#if !UNITY_2023_1_OR_NEWER
                EditorGUILayout.HelpBox("自适应探针体积仅在 Unity 2023 中可用。Adaptative Probe Volumes are only available in Unity 2023.", MessageType.Warning);
#endif
                EditorGUILayout.PropertyField(apvIntensityMultiplier, new GUIContent("Intensity Multiplier 强度倍数"));
                EditorGUI.indentLevel--;
            }

            GUI.enabled = !enableNativeLights.boolValue;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(enablePointLights, new GUIContent("Enable Point Lights 启用点光源"));
            if (GUILayout.Button("Point Light Manager 点光源管理器", GUILayout.Width(180))) {
                Selection.activeGameObject = VolumetricFogManager.pointLightManager.gameObject;
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(enableVoids, new GUIContent("Enable Voids 启用雾空洞"));
            if (GUILayout.Button("Void Manager 空洞管理器", GUILayout.Width(180))) {
                Selection.activeGameObject = VolumetricFogManager.fogVoidManager.gameObject;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(enableFollow, new GUIContent("Enable Follow 启用跟随"));
            if (enableFollow.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(followTarget, new GUIContent("Target 目标"));
                EditorGUILayout.PropertyField(followMode, new GUIContent("Mode 模式"));
                if ((VolumetricFogFollowMode)followMode.intValue == VolumetricFogFollowMode.FullXYZ) {
                    EditorGUILayout.PropertyField(followIncludeDistantFog, new GUIContent("Include Distant Fog 包含远处雾", "Also adjusts distant fog base altitude to the followed object altitude."));
                }
                EditorGUILayout.PropertyField(followOffset, new GUIContent("Offset 偏移"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(enableFade, new GUIContent("Enable Fade 启用淡入淡出"));
            if (enableFade.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(fadeDistance, new GUIContent("Fade Distance 淡入淡出距离"));
                EditorGUILayout.PropertyField(fadeOut, new GUIContent("Fade Out 淡出"));
                EditorGUILayout.PropertyField(fadeController, new GUIContent("Fade Controller 淡入淡出控制器"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(enableSubVolumes, new GUIContent("Enable Sub Volumes 启用子体积"));
            if (enableSubVolumes.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("如果下方未指定子体积，将使用场景中的任何子体积。If no sub-volumes are specified below, any sub-volume in the scene will be used.", MessageType.Info);
                EditorGUILayout.PropertyField(fadeController, new GUIContent("Character Controller 角色控制器"));
                EditorGUILayout.PropertyField(subVolumes, new GUIContent("Sub Volumes 子体积"));
                EditorGUI.indentLevel--;
            }

            bool requiresFogOfWarTextureReload = false;
            EditorGUILayout.PropertyField(enableFogOfWar, new GUIContent("Enable Fog Of War 启用战争迷雾"));
            if (enableFogOfWar.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(fogOfWarCenter, new GUIContent("World Center 世界中心"));
                EditorGUILayout.PropertyField(fogOfWarIsLocal, new GUIContent("Is Local 局部坐标", "Enable if fog of war center is local to the fog volume"));
                EditorGUILayout.PropertyField(fogOfWarSize, new GUIContent("World Coverage 世界覆盖范围"));
                EditorGUILayout.PropertyField(fogOfWarShowCoverage, new GUIContent("Show Coverage Bounds 显示覆盖边界"));
                EditorGUILayout.PropertyField(fogOfWarTextureWidth, new GUIContent("Texture Width 纹理宽度"));
                EditorGUILayout.PropertyField(fogOfWarTextureHeight, new GUIContent("Texture Height 纹理高度"));
                EditorGUILayout.PropertyField(fogOfWarRestoreDelay, new GUIContent("Restore Delay 恢复延迟"));
                EditorGUILayout.PropertyField(fogOfWarRestoreDuration, new GUIContent("Restore Duration 恢复持续时间"));
                EditorGUILayout.PropertyField(fogOfWarSmoothness, new GUIContent("Border Smoothness 边界平滑度"));
                EditorGUILayout.PropertyField(fogOfWarBlur, new GUIContent("Blur 模糊"));

                EditorGUILayout.Separator();
                EditorGUILayout.PropertyField(maskEditorEnabled, new GUIContent("Fog Of War Editor 战争迷雾编辑器", "Activates terrain brush to paint/remove fog of war at custom locations."));

                if (maskEditorEnabled.boolValue) {
                    if (GUILayout.Button("Create New Mask Texture 创建新遮罩纹理")) {
                        if (EditorUtility.DisplayDialog("Create Mask Texture 创建遮罩纹理", "A texture asset will be created with the size specified in current profile (" + fog.fogOfWarTextureWidth + "x" + fog.fogOfWarTextureHeight + ").\n\nContinue?", "Ok", "Cancel")) {
                            CreateNewMaskTexture();
                            GUIUtility.ExitGUI();
                        }
                    }
                    EditorGUI.BeginChangeCheck();
                    fog.fogOfWarTexture = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Coverage Texture 覆盖纹理", "Fog of war coverage mask. A value of alpha of zero means no fog."), fog.fogOfWarTexture, typeof(Texture2D), false);
                    Texture2D tex = fog.fogOfWarTexture;
                    if (EditorGUI.EndChangeCheck()) {
                        requiresFogOfWarTextureReload = true;
                        if (tex != null) {
                            string assetPath = AssetDatabase.GetAssetPath(tex);
                            if (!string.IsNullOrEmpty(assetPath)) {
                                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                                if (importer != null) {
                                    bool settingsChanged = false;
                                    if (!importer.isReadable) {
                                        importer.isReadable = true;
                                        settingsChanged = true;
                                    }

                                    if (importer.textureCompression != TextureImporterCompression.Uncompressed) {
                                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                                        settingsChanged = true;
                                    }
                                    if (settingsChanged) {
                                        importer.SaveAndReimport();
                                        GUIUtility.ExitGUI();
                                    }
                                }
                            }
                        }
                    }

                    if (tex != null) {
                        EditorGUILayout.LabelField("   Texture Width", tex.width.ToString());
                        EditorGUILayout.LabelField("   Texture Height", tex.height.ToString());
                        string path = AssetDatabase.GetAssetPath(tex);
                        if (string.IsNullOrEmpty(path)) {
                            path = "(Temporary texture)";
                        }
                        EditorGUILayout.LabelField("   Texture Path", path);
                    }

                    if (tex != null) {
                        EditorGUILayout.Separator();
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(maskBrushMode, new GUIContent("Brush Mode 笔刷模式", "Select brush operation mode."));
                        if (GUILayout.Button("Toggle 切换", GUILayout.Width(70))) {
                            maskBrushMode.intValue = maskBrushMode.intValue == 0 ? 1 : 0;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (maskBrushMode.intValue == (int)MaskTextureBrushMode.ColorFog) {
                            EditorGUILayout.PropertyField(maskBrushColor, new GUIContent("   Color 颜色", "Brush color."));
                        }
                        EditorGUILayout.PropertyField(maskBrushWidth, new GUIContent("   Width 宽度", "Width of the snow editor brush."));
                        EditorGUILayout.PropertyField(maskBrushFuzziness, new GUIContent("   Fuzziness 模糊度", "Solid vs spray brush."));
                        EditorGUILayout.PropertyField(maskBrushOpacity, new GUIContent("   Opacity 不透明度", "Stroke opacity."));
                        EditorGUILayout.BeginHorizontal();
                        if (tex == null) GUI.enabled = false;
                        if (GUILayout.Button("Fill Mask 填充遮罩")) {
                            fog.ResetFogOfWar(1f);
                            maskBrushMode.intValue = (int)MaskTextureBrushMode.RemoveFog;
                        }
                        if (GUILayout.Button("Clear Mask 清除遮罩")) {
                            fog.ResetFogOfWar(0);
                            maskBrushMode.intValue = (int)MaskTextureBrushMode.AddFog;
                        }

                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(enableUpdateModeOptions, new GUIContent("Enable Update Mode Options 启用更新模式选项"));
            if (enableUpdateModeOptions.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(updateMode, new GUIContent("Update Mode 更新模式"));
                EditorGUILayout.PropertyField(updateModeCamera, new GUIContent("Camera 相机"));
                if (updateMode.intValue == (int)VolumetricFogUpdateMode.WhenCameraIsInsideArea) {
                    EditorGUILayout.PropertyField(updateModeBounds, new GUIContent("Bounds 边界"));
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(showBoundary, new GUIContent("Show Boundary 显示边界"));

            EditorGUILayout.Separator();
            if (GUILayout.Button("Show Volumetric Fog Manager Settings 显示体积雾管理器设置")) {
                Selection.activeObject = VolumetricFogManager.instance;
            }

            serializedObject.ApplyModifiedProperties();
            
            if (requiresFogOfWarTextureReload) {
                fog.ReloadFogOfWarTexture();
            }

        }


        void CreateFogProfile () {

            // Find directional light and adjusts brightness to avoid excessive bright fog
            float brightness = 1f;
#if UNITY_2023_2_OR_NEWER
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
#else
            Light[] lights = FindObjectsOfType<Light>();
#endif
            if (lights != null) {
                foreach (Light light in lights) {
                    if (light.type == LightType.Directional) {
                        brightness /= light.intensity;
                        break;
                    }
                }
            }

            string path = "Assets";
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets)) {
                path = AssetDatabase.GetAssetPath(obj);
                if (File.Exists(path)) {
                    path = Path.GetDirectoryName(path);
                }
                break;
            }
            VolumetricFogProfile fp = CreateInstance<VolumetricFogProfile>();
            fp.name = "New Volumetric Fog Profile";
            fp.brightness = brightness;
            AssetDatabase.CreateAsset(fp, path + "/" + fp.name + ".asset");
            AssetDatabase.SaveAssets();
            profile.objectReferenceValue = fp;
            EditorGUIUtility.PingObject(fp);
        }


        void OnSceneGUI () {
            OnSceneGUI_FogOfWar();
            OnSceneGUI_TransformHandle();
        }

        private readonly BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        void OnSceneGUI_TransformHandle () {
            if (fog == null) return;
            Bounds bounds = fog.GetBounds();
            m_BoundsHandle.center = bounds.center;
            m_BoundsHandle.size = bounds.size;

            // draw the handle
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck()) {
                // record the target object before setting new values so changes can be undone/redone
                Undo.RecordObject(fog, "Change Bounds");

                // copy the handle's updated data back to the target object
                Bounds newBounds = new Bounds();
                newBounds.center = m_BoundsHandle.center;
                newBounds.size = m_BoundsHandle.size;
                fog.SetBounds(newBounds);
            }
        }
    }
}
