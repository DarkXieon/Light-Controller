using UnityEngine;

namespace LightControls.ControlOptions.ControlGroups
{
    [System.Serializable]
    public class AudioControlGroup
    {
        public AudioSource Source;
        private int? previousIndex = -1;
        private float currentIntensity = 0f;
        private int previousEndIndex = 0;

        public AudioControlGroup(AudioSource source)
        {
            Source = source;
        }

        public void SetIntensity(LightInfo lightInfo, AudioNode[] audioClips)
        {
            int? index = IndexOf(audioClips, lightInfo.CurrentIntensity);
            
            if ((lightInfo.IntensityGenerator.HasAuthority && lightInfo.ControlOptionInfo.CurrentStage == ApplicationStages.IntensityApplication) 
            || (!lightInfo.IntensityGenerator.HasAuthority && lightInfo.ControlOptionInfo.CurrentStage == ApplicationStages.OtherApplications))
            {
                if (index.HasValue)
                {
                    if (index.Value != previousIndex.Value)
                    {
                        Source.Stop();
                        Source.clip = audioClips[index.Value].Clip;
                        Source.time = audioClips[index.Value].AudioStart;
                        Source.loop = false; // maybe? might need a fixed update call to make this work better
                        Source.volume = audioClips[index.Value].MaxVolume;
                        Source.Play();
                        previousEndIndex = Source.timeSamples;
                    }
                    if (audioClips[index.Value].Loop && (!Source.isPlaying || Source.time < audioClips[index.Value].AudioStart || Source.time >= audioClips[index.Value].AudioEnd))
                    {
                        Source.time = audioClips[index.Value].AudioStart;
                        previousEndIndex = Source.timeSamples;
                        Source.Play();
                        
                    }
                    previousIndex = index;

                    if (lightInfo.IntensityGenerator.HasAuthority)
                    {
                        int endingSample = Source.timeSamples;
                        
                        if (endingSample > previousEndIndex)
                        {
                            int samplesLength = (int)Mathf.Pow(2, Mathf.Min(14, (int)Mathf.Log(endingSample - previousEndIndex, 2) + 1));
                            float[] samples = GetAllOutputSamples(samplesLength); //new float[samplesLength];
                            //Source.GetOutputData(samples, 0);

                            float minSample = 1f;
                            float maxSample = 0f;
                            int loopLength = Mathf.Min(endingSample - previousEndIndex, samplesLength);

                            for (int i = 1; i < loopLength; i++)
                            {
                                float currentSample = Mathf.Abs(samples[i]);

                                minSample = Mathf.Min(currentSample, minSample);

                                maxSample = Mathf.Max(currentSample, maxSample);
                            }

                            float minIntensity = GetSampleIntensity(
                                minSample,
                                audioClips[index.Value].MinSampleVolume,
                                audioClips[index.Value].MaxSampleVolume,
                                lightInfo.IntensityGenerator.MinIntensity,
                                lightInfo.IntensityGenerator.MaxIntensity);

                            float maxIntensity = GetSampleIntensity(
                                maxSample,
                                audioClips[index.Value].MinSampleVolume,
                                audioClips[index.Value].MaxSampleVolume,
                                lightInfo.IntensityGenerator.MinIntensity,
                                lightInfo.IntensityGenerator.MaxIntensity);

                            currentIntensity = currentIntensity - minIntensity > maxIntensity - currentIntensity //maybe add an average intensity to determine the proper chosen intensity
                                ? minIntensity
                                : maxIntensity;

                            previousEndIndex = endingSample;
                        }
                    }
                    
                    if (audioClips[index.Value].ChangeVolumeFromIntensity) /*&& !lightInfo.IntensityGenerator.HasAuthority*/// && lightInfo.MinIntensity != lightInfo.MaxIntensity)
                    {
                        float volume = index.Value == audioClips.Length - 1
                            ? (lightInfo.CurrentIntensity - audioClips[index.Value].PlayAtIntensity) / (lightInfo.MaxIntensity - audioClips[index.Value].PlayAtIntensity)
                            : (lightInfo.CurrentIntensity - audioClips[index.Value].PlayAtIntensity) / (audioClips[index.Value + 1].PlayAtIntensity - audioClips[index.Value].PlayAtIntensity);

                        volume = audioClips[index.Value].MinVolume == audioClips[index.Value].MaxVolume
                            ? audioClips[index.Value].MinVolume
                            : Utilities.MathUtils.ReMap(volume, 0f, 1f, audioClips[index.Value].MinVolume, audioClips[index.Value].MaxVolume);

                        Source.volume = volume;
                    }
                    else if(!audioClips[index.Value].ChangeVolumeFromIntensity)
                    {
                        Source.volume = audioClips[index.Value].MaxVolume;
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
                        currentIntensity = 0f;
                    }
                }
            }

            if(lightInfo.ControlOptionInfo.CurrentStage != ApplicationStages.OtherApplications)
            {
                lightInfo.IntensityGenerator.ApplyIntensity(lightInfo.ControlOptionInfo, currentIntensity);
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

        private float[] GetAllOutputSamples(int outputPerChannel)
        {
            float[] allSamples = new float[outputPerChannel * Source.clip.channels];
            
            for(int i = 0; i < Source.clip.channels; i++)
            {
                float[] samples = new float[outputPerChannel];
                Source.GetOutputData(samples, i);

                for(int k = 0; k < samples.Length; k++)
                {
                    allSamples[k + outputPerChannel * i] = samples[k];
                }
            }

            return allSamples;
        }
    }
}