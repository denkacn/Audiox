using System.Collections.Generic;
using Audiox.Runtime.Models;
using UnityEngine;

namespace Audiox.Runtime
{
    public class AudioxPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;
        [SerializeField] private AudioxLibrariesContainerAsset _librariesContainer;

        public bool IsPlaying => _isPlaying;
        
        private PlayData _playData;
        private bool _isPlaying = false;
        
        public void Play(string sampleName)
        {
            var playData = _librariesContainer.GetPlayDataByName(sampleName);
            if (playData != null)
            {
                _playData = playData;
                
                _source.clip = _playData.Clip;
                _source.time = _playData.Sample.StartPosition;
                _isPlaying = true;
                
                _source.Play();
            }
        }

        public List<string> GetAvailableSampleNames()
        {
            return _librariesContainer.GetAvailableSampleNames();
        }

        private void Update()
        {
            if (_isPlaying && _source.time >= _playData.Sample.EndPosition)
            {
                _source.Pause();
                _isPlaying = false;
            }
        }
    }
}
