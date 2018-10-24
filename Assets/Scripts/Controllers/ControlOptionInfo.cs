using LightControls.ControlOptions;

using UnityEngine;

namespace LightControls.Controllers
{
    [System.Serializable]
    public class ControlOptionInfo
    {
        public ApplicationStages CurrentStage;

        public Light[] Lights;
        public Renderer[] EmissiveMaterialRenderers;

        public bool SaveLightColor;
        public bool SaveMaterialColor;

        public Color[] LightColors;
        public Color[][] MaterialColors;

        public float IntensityMax;
        public float IntensityMin;

        public ControlOptionInfo()
        {
            Lights = new Light[0];
            EmissiveMaterialRenderers = new Renderer[0];
            LightColors = new Color[0];
            MaterialColors = new Color[0][];
        }
    }
}