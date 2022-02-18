using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class MinoQueue
{
    private Queue<BlockType> m_MinoQueue = null;

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
        if(m_MinoQueue == null)
        {
            m_MinoQueue = new Queue<BlockType>();
        }
        
        if(!m_MinoQueue.Any())
        {
            var newMinoList = CreateRandomizeMinoList();
            newMinoList.ForEach(mino =>
            {
                m_MinoQueue.Enqueue(mino);
            });
        }

        var nextMino = m_MinoQueue.Dequeue();
        return nextMino;
    }
}