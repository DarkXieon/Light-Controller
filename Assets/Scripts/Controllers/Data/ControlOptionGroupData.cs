using LightControls.ControlOptions;

using UnityEngine;

namespace LightControls.Controllers.Data
{
    [System.Serializable]
    public class ControlOptionGroupData
    {
        public Light[] Lights => lights;
        public Renderer[] EmissiveMaterialRenderers => emissiveMaterialRenderers;

        [SerializeField] private Light[] lights;
        [SerializeField] private Renderer[] emissiveMaterialRenderers;
        
        public ControlOptionGroupData()
        {
            lights = new Light[0];
            emissiveMaterialRenderers = new Renderer[0];
        }
    }
}