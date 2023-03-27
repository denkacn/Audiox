using System;
using System.Collections.Generic;
using Audiox.Runtime.Assets;
using Audiox.Runtime.Models;
using UnityEngine;

namespace Audiox.Runtime
{
    [Serializable]
    [CreateAssetMenu(fileName = "LibrariesContainer", menuName = "Audiox Libraries Container", order = 52)]
    public class AudioxLibrariesContainerAsset : ScriptableObject
    {
        [SerializeField] private List<AudioxBaseLibraryAsset> _libraries;

        public PlayData GetPlayDataByName(string sampleName)
        {
            foreach (var library in _libraries)
            {
                var playData = library.GetPlayDataByName(sampleName);
                if (playData != null && playData.Sample != null)
                {
                    return playData;
                }
            }

            return null;
        }
        
        public PlayData GetPlayDataByName(string sampleName, string libraryName)
        {
            var library = _libraries.Find(l => l.LibraryName == libraryName);
            if (library != null)
            {
                return library.GetPlayDataByName(sampleName);
            }

            return null;
        }

        public List<string> GetAvailableSampleNames()
        {
            var availableSampleNames = new List<string>();
            
            foreach (var library in _libraries)
            {
                var sampleNames = library.GetAvailableSampleNames();
                availableSampleNames.AddRange(sampleNames);
            }

            return availableSampleNames;
        }
    }
}
