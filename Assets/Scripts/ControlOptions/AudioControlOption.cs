using System;
using System.Collections.Generic;
using System.Linq;
using LightControls.Controllers;
using LightControls.ControlOptions.ControlGroups;

using UnityEngine;

namespace LightControls.ControlOptions
{
    /// <summary>
    /// Audio Nodes are used to store information about how and when to use a given audio clip for the light's audio
    /// </summary>
    [System.Serializable]
    public class AudioNode
    {
        public AudioClip Clip; //The clip to be played
        public float PlayAtIntensity; //This is the intensity threshold that the light must meet in order for this to be able to be played
        public float AudioStart; //The time that the clip should start at
        public float AudioEnd; //The time that the clip audio should end at
        public float MinVolume; //The minimum volume possible for the audio--this is the same as max volume if ChangeVolumeFromIntensity is false
        public float MaxVolume; //The maximum volume possible for the audio--this is the same as min volume if ChangeVolumeFromIntensity is false
        public bool Loop; //If enabled, the audio will loop through the times of the clip defined by AudioStart and AudioEnd
        public bool ChangeVolumeFromIntensity; //This will cause the volume to be variable and change based on the current intensity of the light

        //This stuff gets set when the user drags on a clip and when they change other variables that affect this
        public float MaxSampleVolume;
        public float MinSampleVolume;
    }

    /// <summary>
    /// This is simply a data class that is passed into the audio control groups
    /// </summary>
    public struct LightInfo
    {
        public AudioIntensityGenerator IntensityGenerator;
        public ControlOptionGroup ControlOptionInfo;
        public float CurrentIntensity; //The current intensity of the lights
        public float MaxIntensity; //The max intensity possible based on the control options that the lights have attached to them
        public float MinIntensity; //The min intensity possible based on the control options that the lights have attached to them

        public LightInfo(ControlOptionGroup controlOptionInfo, AudioIntensityGenerator intensityGenerator, float current, float max, float min)
        {
            ControlOptionInfo = controlOptionInfo;
            IntensityGenerator = intensityGenerator;
            CurrentIntensity = current;
            MaxIntensity = max;
            MinIntensity = min;
        }
    }

    [System.Serializable]
    public class AudioIntensityGenerator
    {
        public float MinIntensity => minIntensity;
        public float MaxIntensity => maxIntensity;
        public bool HasAuthority => hasAuthority;

        [SerializeField] private float minIntensity;
        [SerializeField] private float maxIntensity;
        [SerializeField] private bool hasAuthority;
        [SerializeField] private IntensityControlTarget controlTarget = IntensityControlTarget.LightIntensity | IntensityControlTarget.MaterialColorIntensity; //IntensityControlTarget.Nothing;
        [SerializeField] private ControlTargetModifier[] intensityModifers; //each of these corrospond to a Control Target with a value > 0 in value. The equation for the correct index of a IntensityControlTarget with a value > 0 is
                                                                            //log base two of the enum's int value, going the other way it's the opposite, two to the index power ex. log2(int enum value) = index and 2^(index) = int enum value
                                                                            
        //private IntensityControlTarget previousTarget = IntensityControlTarget.Nothing;

        public AudioIntensityGenerator()
        {
            intensityModifers = intensityModifers ?? new ControlTargetModifier[]
            {
                new ControlTargetModifier(), new ControlTargetModifier(), new ControlTargetModifier(), new ControlTargetModifier(), new ControlTargetModifier()
            };

            hasAuthority = true;
            minIntensity = 8f;
            maxIntensity = 100f;
        }

        public bool ApplyOn(ApplicationStages stage)
        {
            return InstancedIntensityControlOption.ApplyOn(stage, controlTarget);
        }

        public void ApplyIntensity(ControlOptionGroup controlOptionInfo, float intensity)
        {
            InstancedIntensityControlOption.ApplyControl(
                intensity: intensity,
                intensityFloor: minIntensity,
                intensityCeiling: maxIntensity,
                currentTarget: controlTarget,
                //previousTarget: ref previousTarget,
                modifiers: intensityModifers,
                controlOptionInfo: controlOptionInfo);
        }
    }

    /// <summary>
    /// This controls the audio generation of lights
    /// </summary>
    [CreateAssetMenu(menuName = "Light Controller/Audio Control Option")]
    public class AudioControlOption : LightControlOption
    {
        public AudioSource AudioSourcePrefab; //This is an audio source that you'd like to be instantiated on each light that needs audio
        public AudioNode[] AudioNodes; //These hold information used in determining the proper audio to play for the lights
        public AudioIntensityGenerator IntensityGenerator;

        /// <summary>
        /// Initilize variables here
        /// </summary>
        private void OnEnable()
        {
            AudioNodes = AudioNodes ?? new AudioNode[0];
            IntensityGenerator = IntensityGenerator ?? new AudioIntensityGenerator();
        }

        public override InstancedControlOption GetInstanced()
        {
            return new InstancedAudioControlOption(this);
        }
    }
}