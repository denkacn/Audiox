using System;
using System.Collections.Generic;
using Audiox.Runtime.Models;
using UnityEngine;

namespace Audiox.Runtime.Assets
{
    [Serializable]
    [CreateAssetMenu(fileName = "SamplesLibraryAsset", menuName = "Audiox Sample Library", order = 51)]
    public class AudioxSampleLibraryAsset : AudioxBaseLibraryAsset
    {
        public AudioClip Clip;
        public List<Sample> Samples = new List<Sample>();

        public override PlayData GetPlayDataByName(string sampleName)
        {
            return new PlayData(Clip, Samples.Find(s => s.Name == sampleName));
        }
    }
}