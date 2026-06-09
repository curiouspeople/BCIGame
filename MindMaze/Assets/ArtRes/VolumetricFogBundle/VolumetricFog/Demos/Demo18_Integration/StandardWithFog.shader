Shader "Custom/StandardShaderWithFog"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _BumpMap ("Bumpmap", 2D) = "bump" {}
        _MetallicGlossMap("Metallic", 2D) = "white" {}        
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        [Gamma] _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard noinstancing alpha:fade finalcolor:overlayFogStandard
        #pragma multi_compile_local _ FOG_DISTANCE_ON
        #pragma multi_compile_local _ FOG_POINT_LIGHTS
        #pragma multi_compile_local _ FOG_SUN_SHADOWS_ON

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _MetallicGlossMap;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;            
            float4 screenPos;
            float3 worldPos;
        };

        #include "Assets/VolumetricFog/Resources/Shaders/VolumetricFogOverlay.cginc"

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            // Normal map
            o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));            
            
            // Sample metallic texture and apply metallic/smoothness values
            fixed4 metallicGloss = tex2D(_MetallicGlossMap, IN.uv_MainTex);
            o.Metallic = metallicGloss.r * _Metallic;
            o.Smoothness = metallicGloss.a * _Glossiness;
            
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Standard"
}
