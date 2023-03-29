using Audiox.Runtime.Models;
using UnityEngine;

namespace Audiox.Runtime.Players
{
    public class SimpleDataPlayer : IDataPlayer
    {
        private readonly AudioSource _source;
        
        private PlayDataSimple _playData;
        private bool _isPlaying = false;
        
        public DataPlayerType PlayerType => DataPlayerType.Simple;
        public bool IsPlaying => _isPlaying;

        public SimpleDataPlayer(AudioSource source)
        {
            _source = source;
        }

        public void Play(IPlayData playData, float volume = -1)
        {
            if (playData is PlayDataSimple playDataSimple)
            {
                _playData = playDataSimple;
                
                if (volume >= 0)
                {
                    _source.volume = volume;
                }
                
                _source.clip = _playData.Clip;
                _source.time = _playData.Sample.StartPosition;
                _isPlaying = true;
                
                _source.Play();
            }
        }

        public void Update()
        {
            if (_isPlaying && _source.time >= _playData.Sample.EndPosition)
            {
                _source.Pause();
                _isPlaying = false;
            }
        }
    }
}