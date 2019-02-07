using LightControls.ControlOptions;

using UnityEngine;

namespace LightControls.Controllers.Data
{
    [System.Serializable]
    public class ControlOptionGroupData
    {
        //public ApplicationStages CurrentStage;

        public Light[] Lights => lights;
        public Renderer[] EmissiveMaterialRenderers => emissiveMaterialRenderers;

        [SerializeField] private Light[] lights;
        [SerializeField] private Renderer[] emissiveMaterialRenderers;

        //public bool SaveLightColor;
        //public bool SaveMaterialColor;

        //public Color[] LightColors;
        //public Color[][] MaterialColors;

        //public float IntensityMax;
        //public float IntensityMin;

        public ControlOptionGroupData()
        {
            lights = new Light[0];
            emissiveMaterialRenderers = new Renderer[0];
            //LightColors = new Color[0];
            //MaterialColors = new Color[0][];
        }
    }
}