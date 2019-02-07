using System;
using System.Linq;

using LightControls.Controllers.Data;
using LightControls.ControlOptions;

using UnityEngine;

namespace LightControls.Controllers
{
    public class ControlOptionGroup
    {
        public Light[] Lights => controlOptionGroupData.Lights;
        public Renderer[] EmissiveMaterialRenderers => controlOptionGroupData.EmissiveMaterialRenderers;

        public bool SaveLightColor => saveLightColor;
        public bool SaveMaterialColor => saveMaterialColor;

        public Color[] LightColors => lightColors;
        public Color[][] MaterialColors => materialColors;

        public ApplicationStages CurrentStage;

        public float IntensityMax;
        public float IntensityMin;

        private bool saveLightColor;
        private bool saveMaterialColor;

        private Color[] lightColors;
        private Color[][] materialColors;

        private ControlOptionGroupData controlOptionGroupData;

        public ControlOptionGroup(ControlOptionGroupData data, InstancedControlOption[] options)
        {
            controlOptionGroupData = data;

            lightColors = new Color[0];
            materialColors = new Color[0][];

            saveLightColor = !options.Any(option => option != null && option.ApplyOn(ApplicationStages.LightColorApplication));
            saveMaterialColor = !options.Any(option => option != null && option.ApplyOn(ApplicationStages.MaterialColorApplication));

            InitializeColors();
        }

        private void InitializeColors()
        {
            lightColors = new Color[Lights.Length];

            for (int i = 0; i < Lights.Length; i++)
            {
                if(Lights[i] != null)
                {
                    lightColors[i] = Lights[i].color;
                } 
            }

            materialColors = new Color[EmissiveMaterialRenderers.Length][];

            for (int i = 0; i < EmissiveMaterialRenderers.Length; i++)
            {
                if (EmissiveMaterialRenderers[i] != null)
                {
                    materialColors[i] = new Color[EmissiveMaterialRenderers[i].materials.Length];

                    for (int k = 0; k < materialColors[i].Length; k++)
                    {
                        if (EmissiveMaterialRenderers[i].materials[k] != null)
                        {
                            EmissiveMaterialRenderers[i].materials[k].EnableKeyword("_EMISSION");
                            materialColors[i][k] = EmissiveMaterialRenderers[i].materials[k].GetColor("_EmissionColor");
                        }
                    }

                    EmissiveMaterialRenderers[i].UpdateGIMaterials();
                }
                else
                {
                    materialColors[i] = new Color[0];
                }
            }
        }
    }
}
