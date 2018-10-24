using System.Collections.Generic;
using LightControls.Controllers;
using LightControls.ControlOptions.ControlGroups;

using UnityEngine;

namespace LightControls.ControlOptions
{
    public class InstancedAudioControlOption : InstancedControlOption
    {
        private AudioControlOption audioControlOption;
        
        private Dictionary<Light, AudioControlGroup> audioDictionary; //This is made to keep track of all the lights and thier corrosponding control, each light needs their own so the audio noise originates from each light individually

        public InstancedAudioControlOption(AudioControlOption option)
        {
            audioControlOption = option;

            audioDictionary = new Dictionary<Light, AudioControlGroup>();
        }
        
        /// <summary>
        /// Determines if the this control will apply audio on a given stage
        /// </summary>
        /// 
        /// <param name="stage">
        /// The stage to check against
        /// </param>
        /// 
        /// <returns>
        /// Only returns true when the stage is ApplicationStages.OtherApplications
        /// </returns>
        public override bool ApplyOn(ApplicationStages stage)
        {
            if (stage == ApplicationStages.OtherApplications)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Applies audio generation to the provided lights
        /// </summary>
        /// 
        /// <param name="controlOptionInfo">
        /// The info that this control is updating
        /// </param>
        public override void ApplyControl(ControlOptionInfo controlOptionInfo)
        {
            if (controlOptionInfo.CurrentStage == ApplicationStages.OtherApplications) //Audio application takes place in the last apply call
            {
                UpdateLightAudio(controlOptionInfo);
            }
        }

        /// <summary>
        /// Implementation of the above method
        /// </summary>
        /// 
        /// <param name="controlOptionInfo">
        /// The info that this control is updating
        /// </param>
        private void UpdateLightAudio(ControlOptionInfo controlOptionInfo)
        {
            for (int i = 0; i < controlOptionInfo.Lights.Length; i++)
            {
                Light light = controlOptionInfo.Lights[i];

                AudioControlGroup controller;

                if (!audioDictionary.TryGetValue(light, out controller)) //If the key doesn't exist
                {
                    var source = Object.Instantiate<AudioSource>(audioControlOption.AudioSourcePrefab, light.transform);
                    source.transform.parent = light.transform; //Make sure the audio follows the light if it moves

                    controller = new AudioControlGroup(source); //Make a key

                    audioDictionary.Add(light, controller); //And add it
                }

                LightInfo info = new LightInfo(light.intensity, controlOptionInfo.IntensityMax, controlOptionInfo.IntensityMin); //Just a data wrapper

                controller.SetIntensity(info, audioControlOption.AudioNodes);
            }
        }
    }
}