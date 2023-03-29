using Audiox.Runtime.Players;

namespace Audiox.Runtime.Models
{
    public interface IPlayData
    {
        DataPlayerType PlayerType { get; }
        bool IsCorrect { get; }
    }
}