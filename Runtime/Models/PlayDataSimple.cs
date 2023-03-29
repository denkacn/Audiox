using Audiox.Runtime.Players;
using UnityEngine;

namespace Audiox.Runtime.Models
{
    public class PlayDataSimple : IPlayData
    {
        public readonly AudioClip Clip;
        public readonly Sample Sample;
        public readonly bool IsSample;

        public DataPlayerType PlayerType => DataPlayerType.Simple;
        public bool IsCorrect => Sample != null;

        public PlayDataSimple(AudioClip clip, Sample sample)
        {
            Clip = clip;
            Sample = sample;
            IsSample = true;
        }
        
        public PlayDataSimple(AudioClip clip, string name)
        {
            Clip = clip;
            Sample = new Sample(0, clip.length, name);
        }

        
    }
}