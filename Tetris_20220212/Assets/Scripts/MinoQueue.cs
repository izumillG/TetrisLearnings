using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class MinoQueue
{
    public List<BlockType> MinoQueueList { get; private set; } = new List<BlockType>();
    public BlockType[] RandomizeMino()
    {
        BlockType[] rentMinoQueue =
        {
            BlockType.MinoT,
            BlockType.MinoS,
            BlockType.MinoZ,
            BlockType.MinoL,
            BlockType.MinoJ,
            BlockType.MinoO,
            BlockType.MinoI,
        };
        return rentMinoQueue.OrderBy(i => Guid.NewGuid()).ToArray();
    }
    public BlockType GetNextMino()
    {
        if (MinoQueueList.Count <= 7)
        {
            MinoQueueList.AddRange(RandomizeMino());
        }
        BlockType retNextMino = MinoQueueList[0];
        MinoQueueList.RemoveAt(0);
        return retNextMino;
    }
}