using System;
using System.Linq;

using LightControls.Controllers.Data;
using UnityEngine;

namespace LightControls.Controllers
{
    [System.Serializable]
    public class GroupedLightController : LightController
    {
        public override ILightControllerGroup[] LightControllerInfo => lightControllerGroups;

        [SerializeField] private LightControllerGroupData[] lightControllerGroupsData = new LightControllerGroupData[1];

        private LightControllerGroup[] lightControllerGroups;

        private void Awake()
        {
            lightControllerGroupsData = lightControllerGroupsData ?? new LightControllerGroupData[1];

            InitilizeControllerGroups();
        }

        private void InitilizeControllerGroups()
        {
            lightControllerGroups = new LightControllerGroup[lightControllerGroupsData.Length];

            for(int i = 0; i < lightControllerGroups.Length; i++)
            {
                lightControllerGroups[i] = new LightControllerGroup(lightControllerGroupsData[i]);
            }
        }

        //public LightControllerGroup GetGroupWithData(LightControllerGroupData data)
        //{
        //    return lightControllerGroups
        //        .SingleOrDefault(group => group.InstancedVersionOf(data));
        //}
    }
}