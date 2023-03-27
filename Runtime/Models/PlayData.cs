using UnityEngine;

namespace Audiox.Runtime.Models
{
    public class PlayData
    {
        public readonly AudioClip Clip;
        public readonly Sample Sample;
        public readonly bool IsSample;
        
        public PlayData(AudioClip clip, Sample sample)
        {
            Clip = clip;
            Sample = sample;
            IsSample = true;
        }
        
        public PlayData(AudioClip clip, string name)
        {
            Clip = clip;
            Sample = new Sample(0, clip.length, name);
        }
    }
}