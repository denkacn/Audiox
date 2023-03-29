using System.Threading.Tasks;
using Audiox.Runtime.Models;
using UnityEngine;

namespace Audiox.Runtime.Players
{
    public class SequenceDataPlayer : IDataPlayer
    {
        private AudioxPlayer _player;
        public DataPlayerType PlayerType => DataPlayerType.Sequence;
        public bool IsPlaying => _isPlaying;
        
        private bool _isPlaying = false;
        private PlayDataSequence _playData;

        public SequenceDataPlayer(AudioxPlayer player)
        {
            _player = player;
        }

        public void Play(IPlayData playData, float volume = -1)
        {
            if (playData is PlayDataSequence playDataSequence)
            {
                _playData = playDataSequence;
                _isPlaying = true;
                
                PlaySequence();
            }
        }

        private async Task PlaySequence()
        {
            foreach (var item in _playData.SequenceItems)
            {
                await Task.Delay(item.DelayMs);

                Debug.Log(Time.time + "PlaySequence: " + item.SampleName);
                
                var player = _player.Play(item.SampleName);
                
                while (player.IsPlaying)
                {
                    await Task.Yield();
                }
            }
            
            _isPlaying = false;
        }

        public void Update(){}
    }
}