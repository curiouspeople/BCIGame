using System;
using UnityEngine;

namespace VolumetricFogAndMist2 {

    [ExecuteInEditMode]
    public class FogPointLight : MonoBehaviour {

        [NonSerialized] public Light pointLight;

        [Tooltip("此点光源的内散射倍增系数。Inscattering multiplier for this point light")]
        public float inscattering = 1f;
        [Tooltip("此点光源的强度倍增系数。Intensity multiplier for this point light")]
        public float intensity = 1f;

        private void OnEnable() {
            pointLight = GetComponent<Light>();
            PointLightManager.RegisterPointLight(this);
        }

        private void OnDisable() {
            PointLightManager.UnregisterPointLight(this);
        }

        private void OnValidate() {
            inscattering = Mathf.Max(0, inscattering);
            intensity = Mathf.Max(0, intensity);
        }

    }
}