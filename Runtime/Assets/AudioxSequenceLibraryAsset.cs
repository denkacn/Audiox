using System;
using System.Collections.Generic;
using System.Linq;
using Audiox.Runtime.Models;
using UnityEngine;

namespace Audiox.Runtime.Assets
{
    [Serializable]
    [CreateAssetMenu(fileName = "SequenceLibraryAsset", menuName = "Audiox Sequence Library", order = 51)]
    public class AudioxSequenceLibraryAsset : AudioxBaseLibraryAsset
    {
        [SerializeField] private List<SequenceData> _sequences;
        
        public override IPlayData GetPlayDataByName(string key)
        {
            var sequenceData = _sequences.Find(c => c.SequenceName == key);
            if (sequenceData != null)
            {
                return new PlayDataSequence(sequenceData.SequenceItems);
            }
            
            return null;
        }
        
        public override List<string> GetAvailableSampleNames()
        {
            return _sequences.Select(sequence => sequence.SequenceName).ToList();
        }
    }

    [Serializable]
    public class SequenceData
    {
        public string SequenceName;
        public List<SequenceItemData> SequenceItems;
    }

    [Serializable]
    public class SequenceItemData
    {
        public string SampleName;
        public int DelayMs;
    }
}
