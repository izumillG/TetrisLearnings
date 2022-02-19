using System.Collections;
using System.Collections.Generic;

public class Mino
{
    public enum MoveType
    {
        Left,
        Right,
        Drop        
    }

    public int PosX { get; set; } = default;
    public int PosY { get; set; } = default;
    public int GhostPosX { get; set; } = default;
    public int GhostPosY { get; set; } = default;
    public int Size { get; set; } = default;
    public BlockType Type { get; set; } = BlockType.Empty;
    public bool[,] Shape { get; set; } = new bool[4, 4];

    private bool[,] minoT =
        {
        { false, true, false},
        { true, true, true},
        { false, false, false}
        };

    private bool[,] minoS =
        {
        { false, true, true, },
        { true, true, false, },
        { false, false, false}
        };

    private bool[,] minoZ =
    {
        { true, true, false},
        { false, true, true},
        { false, false, false}
        };

    private bool[,] minoL =
    {
        { true, false, false},
        { true, true, true},
        { false, false, false}
     };

    private bool[,] minoJ =
    {
        { false, false, true},
        { true, true, true},
        { false, false, false}
    };

    private bool[,] minoO =
    {
        {true, true},
        {true, true}
    };

    private bool[,] minoI =
    {
        { false, false, false, false },
        { true, true, true, true },
        { false, false, false, false },
        { false, false, false, false },
    };

    public void GenerateMino(BlockType blockType)
    {
        switch (blockType)
        {
            case BlockType.MinoT:
                Shape = minoT;
                PosX = 4;
                PosY = 0;
                Size = 3;
                break;
            case BlockType.MinoS:
                Shape = minoS;
                PosX = 4;
                PosY = 0;
                Size = 3;
                break;
            case BlockType.MinoZ:
                Shape = minoZ;
                PosX = 4;
                PosY = 0;
                Size = 3;
                break;
            case BlockType.MinoL:
                Shape = minoL;
                PosX = 4;
                PosY = 0;
                Size = 3;
                break;
            case BlockType.MinoJ:
                Shape = minoJ;
                PosX = 4;
                PosY = 0;
                Size = 3;
                break;
            case BlockType.MinoO:
                Shape = minoO;
                PosX = 5;
                PosY = 0;
                Size = 2;
                break;
            case BlockType.MinoI:
                Shape = minoI;
                PosX = 4;
                PosY = 0;
                Size = 4;
                break;
            default:
                Shape = null;
                PosX = default;
                PosY = default;
                break;
        }
        GhostPosX = PosX;
        GhostPosY = PosY;
        Shape = Util.LeftRot(Shape);
        Type = blockType;
    }

    public void RightRot()
    {
        Shape = Util.RightRot(Shape);
    }

    public void LeftRot()
    {
        Shape = Util.LeftRot(Shape);
    }

    public void GoToLeft()
    {
        PosX--;
    }

    public void GoToRight()
    {
        PosX++;
    }

    public void Drop()
    {
        PosY++;
    }

    public void UnDrop()
    {
        PosY--;
    }
}
