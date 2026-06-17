using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

namespace VolumetricFogAndMist2 {

    [CustomEditor(typeof(VolumetricFogManager))]
    public class VolumetricFogManagerEditor : Editor {

        SerializedProperty sun, moon, includeTransparent, depthPeeling, transparentCullMode, includeSemiTransparent, alphaCutOff, semiTransparentCullMode, flipDepthTexture, mainManager;
        SerializedProperty scattering, scatteringThreshold, scatteringIntensity, scatteringAbsorption, scatteringTint, scatteringHighQuality;
        SerializedProperty downscaling, downscalingEdgeDepthThreshold, blurPasses, blurDownscaling, blurSpread, blurHDR, blurEdgePreserve, blurEdgeDepthThreshold, ditherStrength;

        bool toggleOptimizeBuild;
        VolumetricFogShaderOptions shaderAdvancedOptionsInfo;
        int maxIterations;

        private void OnEnable () {
            sun = serializedObject.FindProperty("sun");
            moon = serializedObject.FindProperty("moon");
            includeTransparent = serializedObject.FindProperty("includeTransparent");
            depthPeeling = serializedObject.FindProperty("depthPeeling");
            transparentCullMode = serializedObject.FindProperty("transparentCullMode");
            includeSemiTransparent = serializedObject.FindProperty("includeSemiTransparent");
            alphaCutOff = serializedObject.FindProperty("alphaCutOff");
            semiTransparentCullMode = serializedObject.FindProperty("semiTransparentCullMode");
            flipDepthTexture = serializedObject.FindProperty("flipDepthTexture");
            mainManager = serializedObject.FindProperty("mainManager");
            scattering = serializedObject.FindProperty("scattering");
            scatteringThreshold = serializedObject.FindProperty("scatteringThreshold");
            scatteringIntensity = serializedObject.FindProperty("scatteringIntensity");
            scatteringAbsorption = serializedObject.FindProperty("scatteringAbsorption");
            scatteringTint = serializedObject.FindProperty("scatteringTint");
            scatteringHighQuality = serializedObject.FindProperty("scatteringHighQuality");
            downscaling = serializedObject.FindProperty("downscaling");
            downscalingEdgeDepthThreshold = serializedObject.FindProperty("downscalingEdgeDepthThreshold");
            blurPasses = serializedObject.FindProperty("blurPasses");
            blurDownscaling = serializedObject.FindProperty("blurDownscaling");
            blurSpread = serializedObject.FindProperty("blurSpread");
            blurHDR = serializedObject.FindProperty("blurHDR");
            blurEdgePreserve = serializedObject.FindProperty("blurEdgePreserve");
            blurEdgeDepthThreshold = serializedObject.FindProperty("blurEdgeDepthThreshold");
            ditherStrength = serializedObject.FindProperty("ditherStrength");
            ScanAdvancedOptions();
        }


        public override void OnInspectorGUI () {

            EditorGUILayout.Separator();

            var pipe = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (pipe == null) {
                EditorGUILayout.HelpBox("请指定通用渲染管线资源（前往项目设置 -> 图形）。您可以使用演示文件夹中包含的UniversalRenderPipelineAsset或创建新的管线资源（查看文档了解逐步设置）。Please assign the Universal Rendering Pipeline asset (go to Project Settings -> Graphics). You can use the UniversalRenderPipelineAsset included in the demo folder or create a new pipeline asset (check documentation for step by step setup).", MessageType.Error);
                return;
            }

            if (QualitySettings.renderPipeline != null) {
                pipe = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            }

            if (!pipe.supportsCameraDepthTexture) {
                EditorGUILayout.HelpBox("需要在通用渲染管线资源中启用深度纹理选项！Depth Texture option is required in Universal Rendering Pipeline asset!", MessageType.Error);
                if (GUILayout.Button("Go to Universal Rendering Pipeline Asset 前往通用渲染管线资源")) {
                    Selection.activeObject = pipe;
                }
                EditorGUILayout.Separator();
                GUI.enabled = false;
            }

            if (VolumetricFogEditor.lastEditingFog != null) {
                if (GUILayout.Button("<< Back To Last Volumetric Fog")) {
                    Selection.SetActiveObjectWithContext(VolumetricFogEditor.lastEditingFog, null);
                    GUIUtility.ExitGUI();
                }
            }

            serializedObject.Update();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("General Settings 常规设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sun, new GUIContent("Sun 太阳"));
            EditorGUILayout.PropertyField(moon, new GUIContent("Moon 月亮"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(flipDepthTexture, new GUIContent("Flip Depth Texture 翻转深度纹理"));
            if (flipDepthTexture.boolValue) {
                GUILayout.Label("(Applies only in build 仅构建时生效)");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(mainManager, new GUIContent("Main Manager 主管理器"));

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Custom Depth Pre-Pass 自定义深度预通道", "Support for transparent or semi-transparent objects that need custom depth pass. Click help button for more info."), EditorStyles.boldLabel);
            if (GUILayout.Button(new GUIContent("Help 帮助", "Open transparency support help page in browser 在浏览器中打开透明度支持帮助页面"), GUILayout.Width(60))) {
                Application.OpenURL("https://kronnect.com/guides/volumetric-fog-urp-special-features/#ftoc-heading-9");
            }
            EditorGUILayout.EndHorizontal();
            int transparentLayerMask = includeTransparent.intValue;
            DrawSectionField(includeTransparent, new GUIContent("Transparent Objects 透明物体", "Specify which layers contain transparent objects that should be covered by fog"), transparentLayerMask != 0);
            if (transparentLayerMask != 0) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(transparentCullMode, new GUIContent("Cull Mode 剔除模式"));
                EditorGUILayout.PropertyField(depthPeeling, new GUIContent("Depth Peeling 深度剥离"));
                EditorGUI.indentLevel--;
            }
            int includeSemiTransparentMask = includeSemiTransparent.intValue;
            DrawSectionField(includeSemiTransparent, new GUIContent("Alpha Clipping Alpha裁剪", "Specify which smi-transparent objects (cutout materials) should be covered by fog."), includeSemiTransparentMask != 0);
            if (includeSemiTransparentMask != 0) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(alphaCutOff, new GUIContent("Alpha CutOff Alpha截断值"), true);
                EditorGUILayout.PropertyField(semiTransparentCullMode, new GUIContent("Cull Mode 剔除模式"));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent(""), GUIContent.none);
                if (GUILayout.Button("Refresh 刷新")) {
                    DepthRenderPrePassFeature.DepthRenderPass.FindAlphaClippingRenderers();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            if (includeTransparent.intValue != 0 || includeSemiTransparent.intValue != 0) {
                if (!DepthRenderPrePassFeature.installed) {
                    EditorGUILayout.HelpBox("透明选项需要将'DepthRendererPrePass Feature'添加到通用渲染管线资源中。请查看文档获取说明。Transparent option requires 'DepthRendererPrePass Feature' added to the Universal Rendering Pipeline asset. Check the documentation for instructions.", MessageType.Warning);
                    if (pipe != null && GUILayout.Button("Show Pipeline Asset 显示管线资源")) Selection.activeObject = pipe;
                }
                if ((includeTransparent.intValue & includeSemiTransparent.intValue) != 0) {
                    EditorGUILayout.HelpBox("'透明物体'和'Alpha裁剪'选项不应重叠并包含相同的物体。请确保每个选项中指定的层不同。The options 'Transparent Objects' and 'Alpha Clipping' should not overlap and include same objects. Make sure the specified layers are different in each option.", MessageType.Warning);
                }
            } else if (DepthRenderPrePassFeature.installed) {
                EditorGUILayout.HelpBox("未包含透明物体。从通用渲染管线资源中移除'DepthRendererPrePass Feature'以节省性能。No transparent objects included. Remove 'DepthRendererPrePass Feature' from the Universal Rendering Pipeline asset to save performance.", MessageType.Warning);
                if (pipe != null && GUILayout.Button("Show Pipeline Asset 显示管线资源")) Selection.activeObject = pipe;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(new GUIContent("Final Composition 最终合成", "Support for off-screen rendering and composition to screen target. Allows optimizations like downsampling."), EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            DrawSectionField(scattering, new GUIContent(scattering.displayName, scattering.tooltip), scattering.floatValue > 0);
            if (scattering.floatValue > 0) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(scatteringIntensity, new GUIContent("Brightness 亮度"));
                EditorGUILayout.PropertyField(scatteringThreshold, new GUIContent("Brightness Threshold 亮度阈值"));
                EditorGUILayout.PropertyField(scatteringAbsorption, new GUIContent("Absorption 吸收"));
                EditorGUILayout.PropertyField(scatteringTint, new GUIContent("Tint Color 色调颜色"));
                EditorGUILayout.PropertyField(scatteringHighQuality, new GUIContent("High Quality 高质量"));
                EditorGUI.indentLevel--;
            }
            DrawSectionField(downscaling, new GUIContent(downscaling.displayName, downscaling.tooltip), downscaling.floatValue > 1);
            if (downscaling.floatValue > 1f) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(downscalingEdgeDepthThreshold, new GUIContent("Edge Threshold 边缘阈值"));
                EditorGUI.indentLevel--;
            }
            DrawSectionField(blurPasses, new GUIContent(blurPasses.displayName, blurPasses.tooltip), blurPasses.intValue > 0);
            if (blurPasses.intValue > 0) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(blurDownscaling, new GUIContent("Downscaling 降采样"));
                EditorGUILayout.PropertyField(blurSpread, new GUIContent("Spread 扩散"));
                EditorGUILayout.PropertyField(blurEdgePreserve, new GUIContent("Preserve Edges 保留边缘"));
                if (blurEdgePreserve.boolValue) {
                    EditorGUILayout.PropertyField(blurEdgeDepthThreshold, new GUIContent("Edge Threshold 边缘阈值"));
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(blurHDR, new GUIContent("HDR 高动态范围"));

            if (EditorGUI.EndChangeCheck()) {
                EditorApplication.delayCall += () => UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            EditorGUILayout.PropertyField(ditherStrength, new GUIContent("Dither Strength 递色强度"));

            if (blurPasses.intValue > 0 || downscaling.floatValue > 1 || scattering.floatValue > 0 || (includeTransparent.intValue != 0 && depthPeeling.boolValue)) {
                if (!VolumetricFogRenderFeature.installed) {
                    EditorGUILayout.HelpBox("这些选项需要将'Volumetric Fog Render Feature'添加到通用渲染管线资源中。请查看文档获取说明。These options require 'Volumetric Fog Render Feature' added to the Universal Rendering Pipeline asset. Check the documentation for instructions.", MessageType.Warning);
                    if (pipe != null && GUILayout.Button("Show Pipeline Asset 显示管线资源")) Selection.activeObject = pipe;
                }
                EditorGUILayout.HelpBox("当启用降采样、模糊或散射选项时，雾体积会忽略渲染队列值。请在体积雾渲染功能中选择渲染通道事件。When downscaling, blur or scattering options are enabled, fog volumes ignore render queue value. Select the render pass event in the Volumetric Fog Render Feature.", MessageType.Info);
            } else if (VolumetricFogRenderFeature.installed) {
                EditorGUILayout.HelpBox("未使用最终合成选项（降采样/模糊/散射/深度剥离）。考虑从通用渲染管线资源中移除'Volumetric Fog Render Feature'以节省性能。No final composition options used (downscaling/blur/scattering/depth peeling). Consider removing 'Volumetric Fog Render Feature' from the Universal Rendering Pipeline asset to save performance.", MessageType.Warning);
                if (pipe != null && GUILayout.Button("Show Pipeline Asset 显示管线资源")) Selection.activeObject = pipe;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();

            bool shaderOptionsOpen = toggleOptimizeBuild;
            if (shaderOptionsOpen) {
                EditorGUILayout.BeginVertical(GUI.skin.box);
            }

            if (GUILayout.Button(toggleOptimizeBuild ? "Hide Shader Options 隐藏着色器选项" : "Shader Options 着色器选项", GUILayout.Width(150))) {
                toggleOptimizeBuild = !toggleOptimizeBuild;
            }

            if (toggleOptimizeBuild && shaderAdvancedOptionsInfo != null) {

                int optionsCount = shaderAdvancedOptionsInfo.options.Length;
                for (int k = 0; k < optionsCount; k++) {
                    ShaderAdvancedOption option = shaderAdvancedOptionsInfo.options[k];
                    if (option.hasValue) continue;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", GUILayout.Width(10));
                    bool prevState = option.enabled;
                    bool newState = EditorGUILayout.Toggle(prevState, GUILayout.Width(18));
                    if (prevState != newState) {
                        shaderAdvancedOptionsInfo.options[k].enabled = newState;
                        shaderAdvancedOptionsInfo.pendingChanges = true;
                        GUIUtility.ExitGUI();
                        return;
                    }
                    EditorGUILayout.LabelField(option.name);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", GUILayout.Width(10));
                    EditorGUILayout.LabelField("", GUILayout.Width(18));
                    GUIStyle wrapStyle = new GUIStyle(GUI.skin.label);
                    wrapStyle.wordWrap = true;
                    EditorGUILayout.LabelField(option.description, wrapStyle);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.Width(10));
                EditorGUI.BeginChangeCheck();
                float prevLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 100;
                maxIterations = EditorGUILayout.IntField(new GUIContent("Max Iterations 最大迭代次数", "The maximum number of raymarching steps."), maxIterations, GUILayout.Width(175));
                if (EditorGUI.EndChangeCheck()) {
                    shaderAdvancedOptionsInfo.pendingChanges = true;
                }
                EditorGUIUtility.labelWidth = prevLabelWidth;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Separator();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Refresh 刷新", GUILayout.Width(60))) {
                    ScanAdvancedOptions();
                    GUIUtility.ExitGUI();
                    return;
                }
                if (!shaderAdvancedOptionsInfo.pendingChanges)
                    GUI.enabled = false;
                if (GUILayout.Button("Save Changes 保存更改", GUILayout.Width(110))) {
                    shaderAdvancedOptionsInfo.SetOptionValue("MAX_ITERATIONS", maxIterations);
                    shaderAdvancedOptionsInfo.UpdateAdvancedOptionsFile();
                    GUIUtility.ExitGUI();
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }

            if (shaderOptionsOpen) {
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Managers 管理器", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Point Light Manager 点光源管理器", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button("Open 打开", GUILayout.Width(150))) {
                Selection.activeGameObject = VolumetricFogManager.pointLightManager.gameObject;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Fog Void Manager 雾空洞管理器", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button("Open", GUILayout.Width(150))) {
                Selection.activeGameObject = VolumetricFogManager.fogVoidManager.gameObject;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Tools 工具", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Noise Generator 噪声生成器", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button("Open", GUILayout.Width(150))) {
                NoiseGenerator window = EditorWindow.GetWindow<NoiseGenerator>("Noise Generator", true);
                window.minSize = new Vector2(400, 400);
                window.Show();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        void ScanAdvancedOptions () {
            if (shaderAdvancedOptionsInfo == null) {
                shaderAdvancedOptionsInfo = new VolumetricFogShaderOptions();
            }
            shaderAdvancedOptionsInfo.ReadOptions();
            maxIterations = shaderAdvancedOptionsInfo.GetOptionValue("MAX_ITERATIONS");
        }

        void DrawSectionField (SerializedProperty property, GUIContent content, bool active) {
            EditorGUILayout.PropertyField(property, new GUIContent(active ? content.text + " •" : content.text, content.tooltip));
        }

    }
}