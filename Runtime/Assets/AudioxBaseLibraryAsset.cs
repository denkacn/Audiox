using System.Collections.Generic;
using Audiox.Runtime.Models;
using UnityEngine;

namespace Audiox.Runtime.Assets
{
    public class AudioxBaseLibraryAsset : ScriptableObject
    {
        public string LibraryName;
        
        public virtual IPlayData GetPlayDataByName(string key)
        {
            return null;
        }

        public virtual List<string> GetAvailableSampleNames()
        {
            return null;
        }
    }
}