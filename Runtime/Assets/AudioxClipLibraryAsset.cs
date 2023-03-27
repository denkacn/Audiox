using System;
using System.Collections.Generic;
using Audiox.Runtime.Models;
using UnityEngine;

namespace Audiox.Runtime.Assets
{
    [Serializable]
    [CreateAssetMenu(fileName = "ClipsLibraryAsset", menuName = "Audiox Clip Library", order = 51)]
    
    public class AudioxClipLibraryAsset : AudioxBaseLibraryAsset
    {
        [SerializeField] private List<ClipData> _clipsData;
        
        public override PlayData GetPlayDataByName(string sampleName)
        {
            var clipData = _clipsData.Find(c => c.ClipName == sampleName);
            if (clipData != null)
            {
                return new PlayData(clipData.Clip, clipData.ClipName);
            }
            
            return null;
        } 
    }

    [Serializable]
    public class ClipData
    {
        public string ClipName;
        public AudioClip Clip;
    }
}