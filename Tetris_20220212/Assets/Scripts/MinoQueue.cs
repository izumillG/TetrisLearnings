using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class MinoQueue
{
    // public List<BlockType> MinoQueueList { get; private set; } = new List<BlockType>();
    private Queue<BlockType> MinoQueue = null;

    private List<BlockType> CreateRandomizeMinoList()
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
        return rentMinoQueue.OrderBy(i => Guid.NewGuid()).ToList();
    }

    public BlockType GetNextMino()
    {
        if(MinoQueue == null)
        {
            MinoQueue = new Queue<BlockType>();
        }
        
        if(!MinoQueue.Any())
        {
            var newMinoList = CreateRandomizeMinoList();
            newMinoList.ForEach(mino =>
            {
                MinoQueue.Enqueue(mino);
            });
        }

        var nextMino = MinoQueue.Dequeue();
        return nextMino;

        /*
        if (MinoQueueList.Count <= 7)
        {
            MinoQueueList.AddRange();
        }
        BlockType retNextMino = MinoQueueList[0];
        MinoQueueList.RemoveAt(0);
        return retNextMino;
        */
    }
}