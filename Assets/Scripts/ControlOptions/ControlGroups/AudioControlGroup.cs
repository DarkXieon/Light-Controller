using UnityEngine;

namespace LightControls.ControlOptions.ControlGroups
{
    [System.Serializable]
    public class AudioControlGroup
    {
        public AudioSource Source;
        
        private int? previousIndex = -1;

        public AudioControlGroup(AudioSource source)
        {
            Source = source;
        }

        public void SetIntensity(LightInfo lightInfo, AudioNode[] audioClips)
        {
            int? index = IndexOf(audioClips, lightInfo.CurrentIntensity);

            if (index.HasValue)
            {
                if (index.Value != previousIndex.Value)
                {
                    Source.Stop();
                    Source.clip = audioClips[index.Value].Clip;
                    Source.time = audioClips[index.Value].AudioStart;
                    Source.loop = false;
                    Source.Play();
                }
                else
                {
                    if (audioClips[index.Value].Loop && Source.time >= audioClips[index.Value].AudioEnd)
                    {
                        Source.time = audioClips[index.Value].AudioStart;
                    }
                }
                if (audioClips[index.Value].ChangeVolumeFromIntensity && lightInfo.MinIntensity != lightInfo.MaxIntensity)
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
            }
            else
            {
                if (Source.isPlaying)
                {
                    Source.Stop();
                }

                previousIndex = -1;
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
    }
}