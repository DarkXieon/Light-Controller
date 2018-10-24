using System.Collections.Generic;

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
    }

    /// <summary>
    /// This is simply a data class that is passed into the audio control groups
    /// </summary>
    public struct LightInfo
    {
        public float CurrentIntensity; //The current intensity of the lights
        public float MaxIntensity; //The max intensity possible based on the control options that the lights have attached to them
        public float MinIntensity; //The min intensity possible based on the control options that the lights have attached to them

        public LightInfo(float current, float max, float min)
        {
            CurrentIntensity = current;
            MaxIntensity = max;
            MinIntensity = min;
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

        /// <summary>
        /// Initilize variables here
        /// </summary>
        private void OnEnable()
        {
            AudioNodes = AudioNodes ?? new AudioNode[0];
        }

        public override InstancedControlOption GetInstanced()
        {
            return new InstancedAudioControlOption(this);
        }
    }
}