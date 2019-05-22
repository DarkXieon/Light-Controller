using LightControls.Controllers;
using UnityEngine;

namespace LightControls.ControlOptions.Stages
{
    [System.Serializable]
    public class LightControllerStage : Stage
    {
        public LightController[] LightControllers => lightControllers;

        [SerializeField] private LightController[] lightControllers;

        public LightControllerStage() : base()
        {
            lightControllers = lightControllers ?? new LightController[0];
        }
    }
}