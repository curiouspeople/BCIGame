using UnityEngine;


namespace VolumetricFogAndMist
{
    [ExecuteInEditMode]
    public class CheckShadowDistance : MonoBehaviour
    {
        void OnEnable()
        {
            if (QualitySettings.shadowDistance < 250)
            {
                QualitySettings.shadowDistance = 250;
            }
        }
    }

}