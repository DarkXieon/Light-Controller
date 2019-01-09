using System.Linq;
using UnityEngine;

namespace LightControls.ControlOptions.ControlGroups
{
    [System.Serializable]
    public class AudioControlGroup
    {
        public AudioSource Source;
        private int? previousIndex = -1;
        private int previousSampleIndex = 0;
        private float currentIntensity = 0f;

        public AudioControlGroup(AudioSource source)
        {
            Source = source;
        }

        public void SetIntensity(LightInfo lightInfo, AudioNode[] audioClips)
        {
            int? index = IndexOf(audioClips, lightInfo.CurrentIntensity);

            if(!(lightInfo.ControlOptionInfo.CurrentStage == ApplicationStages.IntensityApplication || lightInfo.ControlOptionInfo.CurrentStage == ApplicationStages.OtherApplications))
            {
                lightInfo.IntensityGenerator.ApplyIntensity(lightInfo.ControlOptionInfo, currentIntensity);

                return;
            }

            if (index.HasValue)
            {
                if (index.Value != previousIndex.Value)
                {
                    Source.Stop();
                    Source.clip = audioClips[index.Value].Clip;
                    Source.time = audioClips[index.Value].AudioStart;
                    Source.loop = false;
                    Source.Play();
                    previousIndex = (int)(audioClips[index.Value].AudioStart * audioClips[index.Value].Clip.frequency);// * audioClips[index.Value].Clip.channels);
                }
                //else
                //{
                //    if (audioClips[index.Value].Loop && Source.time >= audioClips[index.Value].AudioEnd)
                //    {
                //        Source.time = audioClips[index.Value].AudioStart;
                //        Source.Play();
                //        previousIndex = (int)(audioClips[index.Value].AudioStart * audioClips[index.Value].Clip.frequency);
                //    }
                //}
                if (audioClips[index.Value].Loop && Source.time >= audioClips[index.Value].AudioEnd)
                {
                    //Source.Stop();
                    Source.time = audioClips[index.Value].AudioStart;
                    //Source.Play();
                    previousIndex = (int)(audioClips[index.Value].AudioStart * audioClips[index.Value].Clip.frequency);// * audioClips[index.Value].Clip.channels);
                }
                if (audioClips[index.Value].ChangeVolumeFromIntensity && !lightInfo.IntensityGenerator.HasAuthority && lightInfo.MinIntensity != lightInfo.MaxIntensity)
                {
                    float volume = index.Value == audioClips.Length - 1
                        ? (lightInfo.CurrentIntensity - audioClips[index.Value].PlayAtIntensity) / (lightInfo.MaxIntensity - audioClips[index.Value].PlayAtIntensity)
                        : (lightInfo.CurrentIntensity - audioClips[index.Value].PlayAtIntensity) / (audioClips[index.Value + 1].PlayAtIntensity - audioClips[index.Value].PlayAtIntensity);

                    volume = audioClips[index.Value].MinVolume == audioClips[index.Value].MaxVolume
                        ? audioClips[index.Value].MinVolume
                        : Utilities.MathUtils.ReMap(volume, 0f, 1f, audioClips[index.Value].MinVolume, audioClips[index.Value].MaxVolume);

                    Source.volume = volume;
                }
                
                previousIndex = index;

                if (lightInfo.IntensityGenerator.HasAuthority)
                {
                    //int currentSampleIndex = (int)(Source.time * Source.clip.channels * Source.clip.frequency);
                    float minSample = 1f;
                    float maxSample = 0f;
                    float currentSample = audioClips[index.Value].Samples[(int)(Source.time * Source.clip.channels * Source.clip.frequency)];

                    for (int i = 0; i < (int)(Source.time * Source.clip.channels * Source.clip.frequency) - previousSampleIndex - 1; i++)
                    {
                        float found = Mathf.Abs(audioClips[index.Value].Samples[previousSampleIndex + 1 + i]);

                        minSample = found < minSample
                            ? found
                            : minSample;

                        maxSample = found > maxSample
                            ? found
                            : maxSample;
                    }
                    
                    float currentVolume = currentSample - minSample > maxSample - currentSample
                        ? minSample
                        : maxSample;
                    
                    currentIntensity = GetSampleIntensity(
                        currentVolume, 
                        audioClips[index.Value].MinSampleVolume, 
                        audioClips[index.Value].MaxSampleVolume, 
                        lightInfo.IntensityGenerator.MinIntensity, 
                        lightInfo.IntensityGenerator.MaxIntensity);

                    previousSampleIndex = (int)(Source.time * Source.clip.channels * Source.clip.frequency);
                    
                    lightInfo.IntensityGenerator.ApplyIntensity(lightInfo.ControlOptionInfo, currentIntensity);
                }
            }
            else
            {
                if (Source.isPlaying)
                {
                    Source.Stop();
                }

                previousIndex = -1;

                if (lightInfo.IntensityGenerator.HasAuthority)
                {
                    lightInfo.IntensityGenerator.ApplyIntensity(lightInfo.ControlOptionInfo, 0f);
                }
            }
        }
        
        private int? IndexOf(AudioNode[] audioClips, float intensity)
        {
            if (intensity >= audioClips[0].PlayAtIntensity)
            {
                for (int i = 0; i < audioClips.Length; i++)
                {
                    if (intensity < audioClips[i].PlayAtIntensity)
                    {
                        return i - 1;
                    }
                }

                return audioClips.Length - 1;
            }
            else
            {
                return null;
            }
        }

        private float GetSampleIntensity(float currentVolume, float minVolume, float maxVolume, float minIntensity, float maxIntensity)
        {
            float percentage = (currentVolume - minVolume) / (maxVolume - minVolume);

            return Mathf.Pow(maxIntensity, percentage) * Mathf.Pow(minIntensity, 1f - percentage);
        }
    }
}