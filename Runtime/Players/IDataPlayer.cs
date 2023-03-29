using Audiox.Runtime.Models;

namespace Audiox.Runtime.Players
{
    public interface IDataPlayer
    {
        DataPlayerType PlayerType { get; }
        bool IsPlaying { get; }
        void Play(IPlayData playData, float volume = -1);
        void Update();
    }
}