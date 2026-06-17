#define FOG_VOID_ROTATION

using UnityEngine;
using UnityEngine.Rendering;

namespace VolumetricFogAndMist2 {

    [ExecuteInEditMode]
    [HelpURL("https://kronnect.com/guides/volumetric-fog-urp-adding-volumetric-fog-mist-to-your-scene/#ftoc-heading-1")]
    public class VolumetricFogManager : MonoBehaviour, IVolumetricFogManager {

        public string managerName {
            get {
                return "Volumetric Fog Manager";
            }
        }

        static PointLightManager _pointLightManager;
        static FogVoidManager _fogVoidManager;
        static VolumetricFogManager _instance;

        [Tooltip("用作太阳的方向光。Directional light used as the Sun")]
        public Light sun;
        [Tooltip("用作月亮的方向光。Directional light used as the Moon")]
        public Light moon;
        [Tooltip("翻转深度纹理。仅在URP中深度在GameView中显示反转时作为Bug的临时解决方案使用。你也可以启用MSAA或HDR来代替此选项。Flip depth texture. Use only as a workaround to a bug in URP if the depth shows inverted in GameView. Alternatively you can enable MSAA or HDR instead of using this option.")]
        public bool flipDepthTexture;
        [Tooltip("启用此选项可在其他子场景可能加载管理器时选择当前管理器。Enable this option to choose this manager when others could be loaded from sub-scenes")]
        public bool mainManager;
        [Tooltip("可选择指定哪些透明层必须包含在深度预通道中。仅在避免雾与某些透明物体裁剪时使用。Optionally specify which transparent layers must be included in the depth prepass. Use only to avoid fog clipping with certain transparent objects.")]
        public LayerMask includeTransparent;
        [Tooltip("透明深度预通道的剔除模式。Cull mode for the transparent depth prepass")]
        public CullMode transparentCullMode = CullMode.Back;
        [Tooltip("分两个阶段渲染雾：在透明物体（如粒子）的背面和正面。Renders fog in two stages: on the back and on the front of transparent objects such as particles")]
        public bool depthPeeling;
        [Tooltip("可选择指定哪些半透明（使用透明度裁剪或cut-off的材质）必须包含在深度预通道中。仅在避免雾与某些透明物体裁剪时使用。Optionally specify which semi-transparent (materials using alpha clipping or cut-off) must be included in the depth prepass. Use only to avoid fog clipping with certain transparent objects.")]
        public LayerMask includeSemiTransparent;
        [Tooltip("可选择设置半透明物体的透明度裁剪阈值。Optionally determines the alpha cut off for semitransparent objects")]
        [Range(0, 1)]
        public float alphaCutOff;
        [Tooltip("半透明深度预通道的剔除模式。Cull mode for the semitransparent depth prepass")]
        public CullMode semiTransparentCullMode = CullMode.Back;

        [Tooltip("雾中光线散射效果。Light scattering effect through fog")]
        [Range(0, 1)]
        public float scattering;

        [Tooltip("输入亮度的阈值。Threshold applied to input brightness")]
        public float scatteringThreshold;
        [Tooltip("输入亮度倍增系数。Brightness multiplier applied to input")]
        public float scatteringIntensity;
        [Tooltip("雾中的吸收或亮度衰减。Absorption or brightness decay inside the fog")]
        [Range(0, 1)]
        public float scatteringAbsorption = 0.35f;
        public Color scatteringTint = Color.white;
        [Tooltip("使用更高分辨率的中间缓冲区和边缘感知的上采样滤镜。Uses higher resolution intermediate buffers and edge-aware upsampling filter")]
        public bool scatteringHighQuality;

        [Range(1, 8)]
        public float downscaling = 1;
        [Tooltip("上采样重建滤镜的基于深度的检测阈值。Depth-based detection threshold for the upscaling reconstruction filter")]
        public float downscalingEdgeDepthThreshold = 0.05f;
        [Range(0, 6)]
        public int blurPasses;
        [Range(1, 8)]
        public float blurDownscaling = 1;
        [Range(0.1f, 4)]
        public float blurSpread = 1f;
        [Tooltip("使用16位RGBA浮点像素格式进行雾体积的渲染和模糊。如果禁用，将使用8位RGBA像素格式，这可以在某些设备上提高性能。注意：如果你使用Bloom或其他基于HDR的效果，也应启用此HDR选项。Uses 16-bit RGBA floating point pixel format for rendering & blur fog volumes. If disabled, 8-bit RGBA pixel format will be used which can improve performance on certain devices. Note that if you use bloom or other HDR-based effects, you should enable this HDR option as well.")]
        public bool blurHDR = true;
        [Tooltip("启用以使用边缘感知模糊。Enable to use an edge-aware blur.")]
        public bool blurEdgePreserve = true;
        [Tooltip("当雾颜色强度低于此值时忽略模糊。Ignores blur when fog color intensity is below this value.")]
        public float blurEdgeDepthThreshold = 0.008f;
        [Range(0, 0.2f)]
        public float ditherStrength;

        // used to keep shader option in sync with scripting side
#if FOG_VOID_ROTATION
        public static bool allowFogVoidRotation => true;
#else
        public static bool allowFogVoidRotation => false;
#endif

        const string SKW_FLIP_DEPTH_TEXTURE = "VF2_FLIP_DEPTH_TEXTURE";

        public const uint FOG_VOLUMES_RENDERING_LAYER = 1 << 49;

        public static VolumetricFogManager instance {
            get {
                if (_instance == null) {
                    _instance = Tools.CheckMainManager();
                }
                return _instance;
            }
        }

        public static VolumetricFogManager GetManagerIfExists() {
            if (_instance != null && _instance.gameObject == null) _instance = null;
            if (_instance == null) {
                VolumetricFogManager[] managers = Misc.FindObjectsOfType<VolumetricFogManager>(true);
                int count = managers.Length;
                // look for main manager
                for (int k = 0; k < count; k++) {
                    VolumetricFogManager manager = managers[k];
                    if (manager.mainManager) {
                        _instance = manager;
                        return _instance;
                    }
                }
                if (count > 0) {
                    _instance = managers[0];
                }
            }
            return _instance;
        }

        public static PointLightManager pointLightManager {
            get {
                Tools.CheckManager(ref _pointLightManager);
                return _pointLightManager;
            }
        }

        public static FogVoidManager fogVoidManager {
            get {
                Tools.CheckManager(ref _fogVoidManager);
                return _fogVoidManager;
            }
        }

        void OnEnable() {
            // Forces other managers to be found
            _pointLightManager = null;
            _fogVoidManager = null;
            // Ensures no other fog manager exist
            VolumetricFogManager[] managers = Misc.FindObjectsOfType<VolumetricFogManager>(true);
            if (managers.Length > 1) {
                bool isThisTheMainManager = mainManager;
                for (int k = 0; k < managers.Length; k++) {
                    if (!managers[k].mainManager) DestroyImmediate(managers[k].gameObject);
                }
                if (!isThisTheMainManager) return;
            }
            if (_instance == null) _instance = this;

            SetupLights();
            SetupDepthPrePass();
            Tools.CheckManager(ref _pointLightManager);
            Tools.CheckManager(ref _fogVoidManager);
        }

        void OnValidate() {
            downscalingEdgeDepthThreshold = Mathf.Max(0.0001f, downscalingEdgeDepthThreshold);
            blurEdgeDepthThreshold = Mathf.Max(0.0001f, blurEdgeDepthThreshold);
            scatteringThreshold = Mathf.Max(0, scatteringThreshold);
            scatteringIntensity = Mathf.Max(0, scatteringIntensity);
            SetupDepthPrePass();
        }


        void SetupLights() {
            Light[] lights = Misc.FindObjectsOfType<Light>();
            for (int k = 0; k < lights.Length; k++) {
                Light l = lights[k];
                if (l.type == LightType.Directional) {
                    if (sun == null) {
                        sun = l;
                    }
                    return;
                }
            }
        }

        void SetupDepthPrePass() {
            #if !UNITY_EDITOR
                Shader.SetGlobalInt(SKW_FLIP_DEPTH_TEXTURE, flipDepthTexture ? 1 : 0);
            #endif
            DepthRenderPrePassFeature.DepthRenderPass.SetupLayerMasks(includeTransparent, includeSemiTransparent);
        }

        /// <summary>
        /// 创建一个新的雾体积。Creates a new fog volume
        /// </summary>
        public static GameObject CreateFogVolume(string name) {
            GameObject go = Resources.Load<GameObject>("Prefabs/FogVolume2D");
            go = Instantiate(go);
            go.name = name;
            return go;
        }

        /// <summary>
        /// 创建一个新的雾空洞。Creates a new fog void
        /// </summary>
        public static GameObject CreateFogVoid(string name) {
            return new GameObject(name, typeof(FogVoid));
        }


        /// <summary>
        /// 创建一个新的雾子体积。Creates a new fog sub-volume
        /// </summary>
        public static GameObject CreateFogSubVolume(string name) {
            GameObject go = Resources.Load<GameObject>("Prefabs/FogSubVolume");
            go = Instantiate(go);
            go.name = name;
            return go;
        }

    }
}
