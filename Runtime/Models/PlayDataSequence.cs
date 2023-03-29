using System.Collections.Generic;
using Audiox.Runtime.Assets;
using Audiox.Runtime.Players;

namespace Audiox.Runtime.Models
{
    public class PlayDataSequence : IPlayData
    {
        public List<SequenceItemData> SequenceItems;
        
        public DataPlayerType PlayerType => DataPlayerType.Sequence;
        public bool IsCorrect => true;

        public PlayDataSequence(List<SequenceItemData> sequenceItems)
        {
            SequenceItems = sequenceItems;
        }
    }
}