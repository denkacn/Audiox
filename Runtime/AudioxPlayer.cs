using System.Collections.Generic;
using System.Linq;
using Audiox.Runtime.Players;
using UnityEngine;

namespace Audiox.Runtime
{
    public class AudioxPlayer : MonoBehaviour
    {
        private readonly List<IDataPlayer> _players = new List<IDataPlayer>();
        
        [SerializeField] private AudioSource _source;
        [SerializeField] private AudioxLibrariesContainerAsset _librariesContainer;

        public bool IsPlaying => _players.Select(player => player.IsPlaying).FirstOrDefault();

        public IDataPlayer Play(string sampleName, float volume = -1)
        {
            var playData = _librariesContainer.GetPlayDataByName(sampleName);
            if (playData != null)
            {
                var player = _players.Find(p => p.PlayerType == playData.PlayerType);
                player?.Play(playData, volume);

                return player;
            }

            return null;
        }

        public List<string> GetAvailableSampleNames()
        {
            return _librariesContainer.GetAvailableSampleNames();
        }

        private void Awake()
        {
            _players.Add(new SimpleDataPlayer(_source));
            _players.Add(new SequenceDataPlayer(this));
        }

        private void Update()
        {
            foreach (var player in _players)
            {
                player.Update();
            }
        }
    }
}
