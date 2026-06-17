//#define FOG_BORDER
//#define FOG_SHADOW_CANCELLATION

using UnityEngine;
using UnityEditor;

namespace VolumetricFogAndMist2 {

    [CustomEditor(typeof(VolumetricFogProfile))]
    public class VolumetricFogProfileEditor : Editor {

        SerializedProperty raymarchQuality, raymarchMinStep, jittering, dithering;
        SerializedProperty renderQueue, sortingLayerID, sortingOrder;
        SerializedProperty constantDensity, noiseTexture, noiseStrength, noiseScale, noiseFinalMultiplier, noiseTextureOptimizedSize;
        SerializedProperty useDetailNoise, detailTexture, detailScale, detailStrength, detailOffset;
        SerializedProperty density;
        SerializedProperty shape, customMesh, scaleNoiseWithHeight, border, customHeight, height, verticalOffset, distance, distanceFallOff, maxDistance, maxDistanceFallOff;
        SerializedProperty terrainFit, terrainFitResolution, terrainLayerMask, terrainFogHeight, terrainFogMinAltitude, terrainFogMaxAltitude;

        SerializedProperty albedo, enableDepthGradient, depthGradient, depthGradientMaxDistance, enableHeightGradient, heightGradient;
        SerializedProperty brightness, deepObscurance, specularColor, specularThreshold, specularIntensity, ambientLightMultiplier;

        SerializedProperty turbulence, windDirection, useCustomDetailNoiseWindDirection, detailNoiseWindDirection;

        SerializedProperty dayNightCycle, sunDirection, sunColor, sunIntensity, lightDiffusionModel, lightDiffusionPower, lightDiffusionIntensity, lightDiffusionNearDepthAtten;
        SerializedProperty receiveShadows, shadowIntensity, shadowCancellation, shadowMaxDistance;
        SerializedProperty cookie;

        SerializedProperty distantFog, distantFogColor, distantFogStartDistance, distantFogDistanceDensity, distantFogMaxHeight, distantFogBaseAltitude, distantFogHeightDensity, distantFogDiffusionIntensity, distantFogRenderQueue, distantFogSymmetrical;

        private void OnEnable() {
            try {
                raymarchQuality = serializedObject.FindProperty("raymarchQuality");
                raymarchMinStep = serializedObject.FindProperty("raymarchMinStep");
                jittering = serializedObject.FindProperty("jittering");
                dithering = serializedObject.FindProperty("dithering");

                renderQueue = serializedObject.FindProperty("renderQueue");
                sortingLayerID = serializedObject.FindProperty("sortingLayerID");
                sortingOrder = serializedObject.FindProperty("sortingOrder");

                constantDensity = serializedObject.FindProperty("constantDensity");

                noiseTexture = serializedObject.FindProperty("noiseTexture");
                noiseStrength = serializedObject.FindProperty("noiseStrength");
                noiseScale = serializedObject.FindProperty("noiseScale");
                noiseFinalMultiplier = serializedObject.FindProperty("noiseFinalMultiplier");
                noiseTextureOptimizedSize = serializedObject.FindProperty("noiseTextureOptimizedSize");

                useDetailNoise = serializedObject.FindProperty("useDetailNoise");
                detailTexture = serializedObject.FindProperty("detailTexture");
                detailScale = serializedObject.FindProperty("detailScale");
                detailStrength = serializedObject.FindProperty("detailStrength");
                detailOffset = serializedObject.FindProperty("detailOffset");

                density = serializedObject.FindProperty("density");
                shape = serializedObject.FindProperty("shape");
                customMesh = serializedObject.FindProperty("customMesh");
                scaleNoiseWithHeight = serializedObject.FindProperty("scaleNoiseWithHeight");
                border = serializedObject.FindProperty("border");

                customHeight = serializedObject.FindProperty("customHeight");
                height = serializedObject.FindProperty("height");
                verticalOffset = serializedObject.FindProperty("verticalOffset");

                distance = serializedObject.FindProperty("distance");
                distanceFallOff = serializedObject.FindProperty("distanceFallOff");
                maxDistance = serializedObject.FindProperty("maxDistance");
                maxDistanceFallOff = serializedObject.FindProperty("maxDistanceFallOff");

                terrainFit = serializedObject.FindProperty("terrainFit");
                terrainFitResolution = serializedObject.FindProperty("terrainFitResolution");
                terrainLayerMask = serializedObject.FindProperty("terrainLayerMask");
                terrainFogHeight = serializedObject.FindProperty("terrainFogHeight");
                terrainFogMinAltitude = serializedObject.FindProperty("terrainFogMinAltitude");
                terrainFogMaxAltitude = serializedObject.FindProperty("terrainFogMaxAltitude");

                albedo = serializedObject.FindProperty("albedo");
                enableDepthGradient = serializedObject.FindProperty("enableDepthGradient");
                depthGradient = serializedObject.FindProperty("depthGradient");
                depthGradientMaxDistance = serializedObject.FindProperty("depthGradientMaxDistance");
                enableHeightGradient = serializedObject.FindProperty("enableHeightGradient");
                heightGradient = serializedObject.FindProperty("heightGradient");

                brightness = serializedObject.FindProperty("brightness");
                deepObscurance = serializedObject.FindProperty("deepObscurance");
                specularColor = serializedObject.FindProperty("specularColor");
                specularThreshold = serializedObject.FindProperty("specularThreshold");
                specularIntensity = serializedObject.FindProperty("specularIntensity");
                ambientLightMultiplier = serializedObject.FindProperty("ambientLightMultiplier");

                turbulence = serializedObject.FindProperty("turbulence");
                windDirection = serializedObject.FindProperty("windDirection");
                useCustomDetailNoiseWindDirection = serializedObject.FindProperty("useCustomDetailNoiseWindDirection");
                detailNoiseWindDirection = serializedObject.FindProperty("detailNoiseWindDirection");

                dayNightCycle = serializedObject.FindProperty("dayNightCycle");
                sunDirection = serializedObject.FindProperty("sunDirection");
                sunColor = serializedObject.FindProperty("sunColor");
                sunIntensity = serializedObject.FindProperty("sunIntensity");

                lightDiffusionModel = serializedObject.FindProperty("lightDiffusionModel");
                lightDiffusionPower = serializedObject.FindProperty("lightDiffusionPower");
                lightDiffusionIntensity = serializedObject.FindProperty("lightDiffusionIntensity");
                lightDiffusionNearDepthAtten = serializedObject.FindProperty("lightDiffusionNearDepthAtten");

                receiveShadows = serializedObject.FindProperty("receiveShadows");
                shadowIntensity = serializedObject.FindProperty("shadowIntensity");
                shadowCancellation = serializedObject.FindProperty("shadowCancellation");
                shadowMaxDistance = serializedObject.FindProperty("shadowMaxDistance");

                cookie = serializedObject.FindProperty("cookie");

                distantFog = serializedObject.FindProperty("distantFog");
                distantFogColor = serializedObject.FindProperty("distantFogColor");
                distantFogStartDistance = serializedObject.FindProperty("distantFogStartDistance");
                distantFogDistanceDensity = serializedObject.FindProperty("distantFogDistanceDensity");
                distantFogMaxHeight = serializedObject.FindProperty("distantFogMaxHeight");
                distantFogBaseAltitude = serializedObject.FindProperty("distantFogBaseAltitude");
                distantFogSymmetrical = serializedObject.FindProperty("distantFogSymmetrical");
                distantFogHeightDensity = serializedObject.FindProperty("distantFogHeightDensity");
                distantFogDiffusionIntensity = serializedObject.FindProperty("distantFogDiffusionIntensity");
                distantFogRenderQueue = serializedObject.FindProperty("distantFogRenderQueue");
            } catch { }
        }


        public override void OnInspectorGUI() {

            serializedObject.Update();

            EditorGUILayout.PropertyField(raymarchQuality, new GUIContent("Raymarch Quality 光线步进质量"));
            EditorGUILayout.PropertyField(raymarchMinStep, new GUIContent("Raymarch Min Step 光线步进最小步长"));
            EditorGUILayout.PropertyField(jittering, new GUIContent("Jittering 抖动"));
            EditorGUILayout.PropertyField(dithering, new GUIContent("Dithering 递色"));
            EditorGUILayout.PropertyField(renderQueue, new GUIContent("Render Queue 渲染队列"));
            EditorGUILayout.PropertyField(sortingLayerID, new GUIContent("Sorting Layer ID 排序图层ID"));
            EditorGUILayout.PropertyField(sortingOrder, new GUIContent("Sorting Order 排序顺序"));

            EditorGUILayout.PropertyField(constantDensity, new GUIContent("Constant Density 恒定密度"));
            if (!constantDensity.boolValue) {
                EditorGUILayout.PropertyField(noiseTexture, new GUIContent("Noise Texture 噪声纹理"));
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(noiseStrength, new GUIContent("Strength 强度"));
                EditorGUILayout.PropertyField(noiseScale, new GUIContent("Scale 缩放"));
                EditorGUILayout.PropertyField(scaleNoiseWithHeight, new GUIContent("Scale Noise With Height 随高度缩放噪声"));
                EditorGUILayout.PropertyField(noiseFinalMultiplier, new GUIContent("Multiplier 倍增"));
                EditorGUILayout.PropertyField(noiseTextureOptimizedSize, new GUIContent("Final Texture Size 最终纹理大小"));
                EditorGUI.indentLevel--;
                EditorGUILayout.PropertyField(useDetailNoise, new GUIContent("Detail Noise 细节噪声"));
                if (useDetailNoise.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(detailTexture, new GUIContent("Detail Texture 细节纹理"));
                    EditorGUILayout.PropertyField(detailStrength, new GUIContent("Strength 强度"));
                    EditorGUILayout.PropertyField(detailScale, new GUIContent("Scale 缩放"));
                    EditorGUILayout.PropertyField(detailOffset, new GUIContent("Offset 偏移"));
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.PropertyField(density, new GUIContent("Density 密度"));
            EditorGUILayout.PropertyField(shape, new GUIContent("Shape 形状"));
            if (shape.enumValueIndex == (int)VolumetricFogShape.Custom)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(customMesh, new GUIContent("Custom Mesh 自定义网格"));
                EditorGUI.indentLevel--;
            }
#if FOG_BORDER
            EditorGUILayout.PropertyField(border);
#else
            GUI.enabled = false;
            EditorGUILayout.LabelField("Border 边界", "(Disabled in Volumetric Fog Manager)");
            GUI.enabled = true;
#endif
            EditorGUILayout.PropertyField(customHeight, new GUIContent("Custom Volume Height 自定义体积高度"));
            if (customHeight.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(height, new GUIContent("Height 高度"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(verticalOffset, new GUIContent("Vertical Offset 垂直偏移"));
            EditorGUILayout.PropertyField(distance, new GUIContent("Distance 距离"));
            if (distance.floatValue > 0) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(distanceFallOff, new GUIContent("Distance Fall Off 距离衰减"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(maxDistance, new GUIContent("Max Distance 最大距离"));
            EditorGUILayout.PropertyField(maxDistanceFallOff, new GUIContent("Max Distance Fall Off 最大距离衰减"));

            EditorGUILayout.PropertyField(terrainFit, new GUIContent("Terrain Fit 地形适配"));
            if (terrainFit.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(terrainFitResolution, new GUIContent("Resolution 分辨率"));
                EditorGUILayout.PropertyField(terrainLayerMask, new GUIContent("Layer Mask 层遮罩"));
                if (Terrain.activeTerrain != null) {
                    int terrainLayer = Terrain.activeTerrain.gameObject.layer;
                    if ((terrainLayerMask.intValue & (1 << terrainLayer)) == 0) {
                        EditorGUILayout.HelpBox("当前地形层不包含在此层遮罩中，地形适配可能无法正常工作。Current terrain layer is not included in this layer mask. Terrain fit may not work properly.", MessageType.Warning);
                    }
                }
                EditorGUILayout.PropertyField(terrainFogHeight, new GUIContent("Fog Height 雾高度"));
                EditorGUILayout.PropertyField(terrainFogMinAltitude, new GUIContent("Min Altitude 最小海拔"));
                EditorGUILayout.PropertyField(terrainFogMaxAltitude, new GUIContent("Max Altitude 最大海拔"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(albedo, new GUIContent("Albedo 反照率"));
            Color albedoColor = albedo.colorValue;
            albedoColor.a = EditorGUILayout.Slider(new GUIContent("Alpha 透明度"), albedoColor.a, 0, 1f);
            albedo.colorValue = albedoColor;
            EditorGUILayout.PropertyField(enableDepthGradient, new GUIContent("Enable Depth Gradient 启用深度渐变"));
            if (enableDepthGradient.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(depthGradient, new GUIContent("Depth Gradient 深度渐变"));
                EditorGUILayout.PropertyField(depthGradientMaxDistance, new GUIContent("Max Distance 最大距离"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(enableHeightGradient, new GUIContent("Enable Height Gradient 启用高度渐变"));
            if (enableHeightGradient.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(heightGradient, new GUIContent("Height Gradient 高度渐变"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(brightness, new GUIContent("Brightness 亮度"));
            EditorGUILayout.PropertyField(deepObscurance, new GUIContent("Deep Obscurance 深层遮蔽"));
            EditorGUILayout.PropertyField(specularColor, new GUIContent("Specular Color 高光颜色"));
            EditorGUILayout.PropertyField(specularThreshold, new GUIContent("Specular Threshold 高光阈值"));
            EditorGUILayout.PropertyField(specularIntensity, new GUIContent("Specular Intensity 高光强度"));

            EditorGUILayout.PropertyField(turbulence, new GUIContent("Turbulence 湍流"));
            EditorGUILayout.PropertyField(windDirection, new GUIContent("Wind Direction 风向"));
            EditorGUILayout.PropertyField(useCustomDetailNoiseWindDirection, new GUIContent("Custom Detail Noise Wind 自定义细节噪声风"));
            if (useCustomDetailNoiseWindDirection.boolValue) {
                EditorGUILayout.PropertyField(detailNoiseWindDirection, new GUIContent("Detail Noise Wind Direction 细节噪声风向"));
            }

            EditorGUILayout.PropertyField(dayNightCycle, new GUIContent("Day Night Cycle 昼夜循环"));
            if (dayNightCycle.boolValue) {
                VolumetricFogManager manager = VolumetricFogManager.GetManagerIfExists();
                if (manager != null && manager.sun == null) {
                    EditorGUILayout.HelpBox("您必须为体积雾管理器的Sun属性指定一个方向光。You must assign a directional light to the Sun property of the Volumetric Fog Manager.", MessageType.Warning);
                    if (GUILayout.Button("Go to Volumetric Fog Manager")) {
                        Selection.activeGameObject = manager.gameObject;
                        EditorGUIUtility.ExitGUI();
                        return;
                    }
                }
            } else { 
                EditorGUILayout.PropertyField(sunDirection, new GUIContent("Sun Direction 太阳方向"));
                EditorGUILayout.PropertyField(sunColor, new GUIContent("Sun Color 太阳颜色"));
                EditorGUILayout.PropertyField(sunIntensity, new GUIContent("Sun Intensity 太阳强度"));
            }
            EditorGUILayout.PropertyField(ambientLightMultiplier, new GUIContent("Ambient Light 环境光", "Amount of ambient light that influences fog colors"));
            EditorGUILayout.PropertyField(lightDiffusionModel, new GUIContent("Light Diffusion Model 光照扩散模型"));
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(lightDiffusionPower, new GUIContent("Spread 扩散"));
            EditorGUILayout.PropertyField(lightDiffusionIntensity, new GUIContent("Intensity 强度"));
            EditorGUILayout.PropertyField(lightDiffusionNearDepthAtten, new GUIContent("Near Depth Attenuation 近深度衰减", "Reduces the intensity of the sun light diffusion effect at distances below this threshold"));

            EditorGUI.indentLevel--;
#if UNITY_2021_3_OR_NEWER
                EditorGUILayout.PropertyField(cookie, new GUIContent("Cookie 光Cookie"));
#endif

            EditorGUILayout.PropertyField(receiveShadows, new GUIContent("Receive Shadows 接收阴影"));
            if (receiveShadows.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(shadowIntensity, new GUIContent("Shadow Intensity 阴影强度"));
#if FOG_SHADOW_CANCELLATION
                EditorGUILayout.PropertyField(shadowCancellation);
#endif
                EditorGUILayout.PropertyField(shadowMaxDistance, new GUIContent("Shadow Max Distance 阴影最大距离"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(distantFog, new GUIContent("Enable Distant Fog 启用远处雾"));
            if (distantFog.boolValue) {
                EditorGUILayout.PropertyField(distantFogColor, new GUIContent("Color 颜色"));
                EditorGUILayout.PropertyField(distantFogStartDistance, new GUIContent("Start Distance 起始距离"));
                EditorGUILayout.PropertyField(distantFogDistanceDensity, new GUIContent("Distance Density 距离密度"));
                EditorGUILayout.PropertyField(distantFogBaseAltitude, new GUIContent("Base Altitude 基础海拔"));
                EditorGUILayout.PropertyField(distantFogMaxHeight, new GUIContent("Max Height 最大高度"));
                EditorGUILayout.PropertyField(distantFogSymmetrical, new GUIContent("Symmetrical 对称"));
                EditorGUILayout.PropertyField(distantFogHeightDensity, new GUIContent("Height Density 高度密度"));
                EditorGUILayout.PropertyField(distantFogDiffusionIntensity, new GUIContent("Diffusion Intensity Multiplier 扩散强度倍增"));
                EditorGUILayout.PropertyField(distantFogRenderQueue, new GUIContent("Render Queue 渲染队列"));
                if (VolumetricFogRenderFeature.isRenderingBeforeTransparents && distantFogRenderQueue.intValue > 2500) {
                    EditorGUILayout.HelpBox("如果体积雾渲染功能在'透明物体之前'运行，请确保渲染队列为2500或更低。Please make sure the render queue is 2500 or less if Volumetric Fog Renderer Feature runs 'Before Transparent Objects'.", MessageType.Warning);
                }
            }

            serializedObject.ApplyModifiedProperties();

        }
    }

}
