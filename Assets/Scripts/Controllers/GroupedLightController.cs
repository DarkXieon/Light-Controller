using System.Linq;

using UnityEngine;

namespace LightControls.Controllers
{
    [System.Serializable]
    public class GroupedLightController : LightController
    {
        public override ILightControllerInfo[] LightControllerInfo => instancedInfo;

        public LightControlInfo[] ControlInfo = new LightControlInfo[0];

        private InstancedLightControllerInfo[] instancedInfo;

        private void Awake()
        {
            ControlInfo = ControlInfo ?? new LightControlInfo[0];

            instancedInfo = ControlInfo
                .Select(info => new InstancedLightControllerInfo(info))
                .ToArray();

            InitializeColors();
        }

        private void InitializeColors()
        {
            for (int i = 0; i < LightControllerInfo.Length; i++)
            {
                ControlInfo[i].ControlOptionInfo.LightColors = ControlInfo[i].ControlOptionInfo.Lights.Select(light => light.color).ToArray();

                ControlInfo[i].ControlOptionInfo.EmissiveMaterialRenderers.ToList().ForEach(renderer => renderer.materials
                    .ToList().ForEach(material => material.EnableKeyword("_EMISSION")));

                ControlInfo[i].ControlOptionInfo.MaterialColors = new Color[ControlInfo[i].ControlOptionInfo.EmissiveMaterialRenderers.Length][];

                for (int k = 0; k < ControlInfo[i].ControlOptionInfo.EmissiveMaterialRenderers.Length; k++)
                {
                    ControlInfo[i].ControlOptionInfo.EmissiveMaterialRenderers[k].UpdateGIMaterials();
                    ControlInfo[i].ControlOptionInfo.MaterialColors[k] = ControlInfo[i].ControlOptionInfo.EmissiveMaterialRenderers[k].materials.Select(material => material.GetColor("_EmissionColor")).ToArray();
                }
            }
        }
    }
}