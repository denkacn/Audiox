using Audiox.Runtime.Models;
using UnityEngine;

namespace Audiox.Runtime.Assets
{
    public class AudioxBaseLibraryAsset : ScriptableObject
    {
        public string LibraryName;
        
        public virtual PlayData GetPlayDataByName(string sampleName)
        {
            return null;
        }
    }
}