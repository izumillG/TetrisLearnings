using System.Collections;
using System.Collections.Generic;

public class Util
{
    public static bool[,] RightRot(bool[,] mino)
    {
        bool[,] minoMirrored = new bool[mino.GetLength(0), mino.GetLength(0)];
        for (int x = 0; x < mino.GetLength(0); x++)
        {
            for (int y = 0; y < mino.GetLength(0); y++)
            {
                minoMirrored[y, mino.GetLength(0) - x - 1] = mino[x, y];
            }
        }
        return minoMirrored;
    }

    public static bool[,] LeftRot(bool[,] mino)
    {
        bool[,] minoMirrored = new bool[mino.GetLength(0), mino.GetLength(0)];
        for (int x = 0; x < mino.GetLength(0); x++)
        {
            for (int y = 0; y < mino.GetLength(1); y++)
            {
                minoMirrored[mino.GetLength(1) - y - 1, x] = mino[x, y];
            }
        }
        return minoMirrored;
    }
}