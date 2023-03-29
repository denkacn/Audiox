using System;
using System.Collections.Generic;
using System.Linq;
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

        public override IPlayData GetPlayDataByName(string key)
        {
            return new PlayDataSimple(Clip, Samples.Find(s => s.Name == key));
        }

        public override List<string> GetAvailableSampleNames()
        {
            return Samples.Select(clipData => clipData.Name).ToList();
        }
    }
}