using System;
using System.Linq;

using LightControls.Controllers.Data;
using LightControls.ControlOptions;

using UnityEngine;

namespace LightControls.Controllers
{
    public struct RendererColor
    {
        public Color[] Colors;
        
        public RendererColor(Color[] materialColors)
        {
            Colors = materialColors;
        }
    }

    public class ControlOptionGroup
    {
        public Light[] Lights => controlOptionGroupData.Lights;
        public Renderer[] EmissiveMaterialRenderers => controlOptionGroupData.EmissiveMaterialRenderers;

        public bool SaveLightColor => saveLightColor;
        public bool SaveMaterialColor => saveMaterialColor;

        public Color[] LightColors => lightColors;
        //public Color[][] MaterialColors => materialColors;
        public RendererColor[] MaterialColors => materialColors;

        public ApplicationStages CurrentStage;

        public bool UpdateColorInfo;
        public float IntensityMax;
        public float IntensityMin;

        private bool saveLightColor;
        private bool saveMaterialColor;

        private Color[] lightColors;
        //private Color[][] materialColors;
        private RendererColor[] materialColors;

        private ControlOptionGroupData controlOptionGroupData;

        public ControlOptionGroup(ControlOptionGroupData data, InstancedControlOption[] options)
        {
            controlOptionGroupData = data;

            UpdateColorInfo = false;
            lightColors = new Color[0];
            //materialColors = new Color[0][];
            materialColors = new RendererColor[0];

            InitializeColors();

            UpdateColorSaving(options);
        }

        public void UpdateColorSaving(InstancedControlOption[] options)
        {
            bool wasSavingLightColor = saveLightColor;
            bool wasSavingMaterialColor = saveMaterialColor;

            saveLightColor = !options.Any(option => option != null && option.ApplyOn(ApplicationStages.LightColorApplication));
            saveMaterialColor = !options.Any(option => option != null && option.ApplyOn(ApplicationStages.MaterialColorApplication));
            
            if (!wasSavingLightColor && saveLightColor)
            {
                RestoreLightColors();
            }

            if(!wasSavingMaterialColor && saveMaterialColor)
            {
                RestoreMaterialColors();
            }
        }

        private void RestoreLightColors()
        {
            for(int i = 0; i < Lights.Length; i++)
            {
                if(Lights[i] != null)
                {
                    Lights[i].color = lightColors[i];
                }
            }
        }

        private void RestoreMaterialColors()
        {
            for (int i = 0; i < EmissiveMaterialRenderers.Length; i++)
            {
                if (EmissiveMaterialRenderers[i] != null)
                {
                    for (int k = 0; k < materialColors[i].Colors.Length; k++)
                    {
                        if (EmissiveMaterialRenderers[i].materials[k] != null)
                        {
                            EmissiveMaterialRenderers[i].materials[k].SetColor("_EmissionColor", materialColors[i].Colors[k]);
                        }
                    }

                    //for (int k = 0; k < materialColors[i].Length; k++)
                    //{
                    //    if (EmissiveMaterialRenderers[i].materials[k] != null)
                    //    {
                    //        EmissiveMaterialRenderers[i].materials[k].SetColor("_EmissionColor", materialColors[i][k]);
                    //    }
                    //}
                }
            }
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

            materialColors = new RendererColor[EmissiveMaterialRenderers.Length];

            for (int i = 0; i < EmissiveMaterialRenderers.Length; i++)
            {
                if (EmissiveMaterialRenderers[i] != null)
                {
                    materialColors[i].Colors = new Color[EmissiveMaterialRenderers[i].materials.Length];

                    for (int k = 0; k < materialColors[i].Colors.Length; k++)
                    {
                        if (EmissiveMaterialRenderers[i].materials[k] != null)
                        {
                            EmissiveMaterialRenderers[i].materials[k].EnableKeyword("_EMISSION");
                            materialColors[i].Colors[k] = EmissiveMaterialRenderers[i].materials[k].GetColor("_EmissionColor");
                        }
                    }

                    EmissiveMaterialRenderers[i].UpdateGIMaterials();
                }
                else
                {
                    materialColors[i].Colors = new Color[0];
                }
            }

            //materialColors = new Color[EmissiveMaterialRenderers.Length][];

            //for (int i = 0; i < EmissiveMaterialRenderers.Length; i++)
            //{
            //    if (EmissiveMaterialRenderers[i] != null)
            //    {
            //        materialColors[i] = new Color[EmissiveMaterialRenderers[i].materials.Length];

            //        for (int k = 0; k < materialColors[i].Length; k++)
            //        {
            //            if (EmissiveMaterialRenderers[i].materials[k] != null)
            //            {
            //                EmissiveMaterialRenderers[i].materials[k].EnableKeyword("_EMISSION");
            //                materialColors[i][k] = EmissiveMaterialRenderers[i].materials[k].GetColor("_EmissionColor");
            //            }
            //        }

            //        EmissiveMaterialRenderers[i].UpdateGIMaterials();
            //    }
            //    else
            //    {
            //        materialColors[i] = new Color[0];
            //    }
            //}
        }
    }
}
