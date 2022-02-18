using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class GameManager : MonoBehaviour
{
    private enum PressType
    {
        NonPressed,
        HalfPressed,
        StrongPressed,
        PressTypeMax
    }
    private const int STAGE_MAX_X = 12; // ƒXƒe[ƒW‚Ì‰¡•‚Í10s‚É•ÇƒuƒƒbƒN2‚ÂB
    private const int STAGE_MAX_Y = 21; // ƒXƒe[ƒW‚Ìc•20s‚É°ƒuƒƒbƒN1‚ÂB
    private bool m_isGameOver = false;
    private bool m_isDownButtonPressed = false;
    private int m_totalClearedLines = 0;

    private PressType m_isLeftButtonPressed     = PressType.NonPressed;
    private PressType m_isRightButtonPressed    = PressType.NonPressed;

    [SerializeField] private float m_buttonPressedJudgeSpan = 0.3f; // ’·‰Ÿ‚µ”»’è‚ÌŽžŠÔ
    [SerializeField] private float m_minoMoveSpan = 0.05f; // ƒuƒƒbƒNˆÚ“®‚ÌŠÔŠu
    [SerializeField] private int m_speedMaximizeLineNum = 40; // —Ž‰º‘¬“x‚ªÅ‘å‚É‚È‚éƒ‰ƒCƒ“”
    [SerializeField] private float m_minDropSpeed = 1f; // Ž©“®—Ž‰º‚ÌŠî–{‘¬“x
    [SerializeField] private float m_maxDropSpeed = 0.2f; // Ž©“®—Ž‰º‚ÌÅ‘å‘¬“x
    private float m_refreshSpan = 1f; // Ž©“®—Ž‰º‚ÌŠî–{‘¬“x
    private float m_dropSpeedCoefficient = 0; // —Ž‰º‘¬“xŒW”

    private float m_dropCurrentTime = 0f; // —Ž‰º‚É‚Â‚¢‚Ä‚ÌƒtƒŒ[ƒ€‚²‚Æ‚Ì•b”‚ÌƒJƒEƒ“ƒ^[
    private float m_moveCurrentTime = 0f; // ¶‰EˆÚ“®‚É‚Â‚¢‚Ä‚ÌƒtƒŒ[ƒ€‚²‚Æ‚Ì•b”‚ÌƒJƒEƒ“ƒ^[
    private bool m_isFirstUpdate = true;

    private Mino m_mino = new Mino(); // —Ž‰º’†‚Ìƒ~ƒm‚ÌƒCƒ“ƒXƒ^ƒ“ƒX
    private MinoQueue m_queue = new MinoQueue();

    [SerializeField] private Block m_blockObject = null; // prefab‚ð“Ç‚Ýž‚ÞUnity‚Ìˆ—‚ð‚·‚éB
    private Block[,] m_stage = new Block[STAGE_MAX_X, STAGE_MAX_Y];
    // ƒuƒƒbƒN‚Ì”z—ñ‚É‚æ‚éƒtƒB[ƒ‹ƒh‚Ì¶¬Bƒ‚ƒjƒ^[‚ÌƒCƒ[ƒWB
    void Start()
    {
        GenerateStage();
        CreateWall();
    }
    void Update()
    {
        if (m_isGameOver)
        {
            Debug.Log("‚¨‚µ‚Ü‚¢I@" + m_totalClearedLines +"ƒ‰ƒCƒ“Á‚µ‚Ü‚µ‚½B‚Ü‚½—V‚ñ‚Å‚ËI");
            return;
        }

        if (m_isFirstUpdate) // Œ»Ý‚Ìˆ—‚Å‚Í1•b–Ú‚ª‹ó”’‚É‚È‚é‚½‚ß•Ê“r1•b–Úê—p‚Ìˆ—‚ð‘–‚ç‚¹‚éB
        {
            CreateMino();
            m_isFirstUpdate = false;
        }

        DropSpeedManager(m_totalClearedLines);
        ControlMino();
        WaitControl();

        if (!m_isDownButtonPressed)
        {
            m_dropCurrentTime += Time.deltaTime; // 1•b‚²‚Æ‚Éˆ—‚ð‘–‚ç‚¹‚é‚½‚ß‚ÌƒMƒ~ƒbƒNB
            if (m_dropCurrentTime > m_refreshSpan)
            {
                m_dropCurrentTime = 0f;
                MoveMino(m_mino.Drop, m_mino.UnDrop, true);
            }
        }
     }
    private void GenerateStage()
    {
        for (int x = 0; x < STAGE_MAX_X; x++)
        {
            for (int y = 0; y < STAGE_MAX_Y; y++)
            {
                m_stage[x, y] = Instantiate<Block>(m_blockObject, new Vector3(x, STAGE_MAX_Y - y, 0f), Quaternion.identity);
            }
        }
    }

    private void DropSpeedManager(int totalClearedLines)
    {
        m_dropSpeedCoefficient = (m_minDropSpeed - m_maxDropSpeed) / m_speedMaximizeLineNum;
        m_refreshSpan = m_totalClearedLines > m_speedMaximizeLineNum ? m_maxDropSpeed : m_minDropSpeed - m_dropSpeedCoefficient * totalClearedLines;
    }

    private void CreateWall()
    {
        for (int x = 0; x < STAGE_MAX_X; x++)
        {
            for (int y = 0; y < STAGE_MAX_Y; y++)
            {
                if (x == 0 || x == STAGE_MAX_X - 1 || y == STAGE_MAX_Y - 1)
                {
                    m_stage[x, y].BlockType = BlockType.Wall;
                    // •Ç‚ÉŠY“–‚·‚é‰ÓŠ‚¾‚¯ƒuƒƒbƒNƒ^ƒCƒv‚ð•Ç‚É•ÏX
                }
            }
        }
    }
    private void CreateMino()
    {
        m_mino.GenerateMino(m_queue.GetNextMino()); // ‰ŠúˆÊ’u‚Éƒ~ƒm‚ð¶¬‚·‚éB
        if (IsCanPut())
        {
            PutMino();
            PutGhost();
        }
        else
        {
            m_isGameOver = true;
            Debug.Log("ƒ~ƒm‚ªo‚¹‚È‚¢");
            return;
        }
        PrintStatus();
        m_moveCurrentTime = 0f;
    }
    private void ControlMino()
    {
        if (Gamepad.current == null) return;

        if (Gamepad.current.dpad.left.wasPressedThisFrame)
        {
            m_moveCurrentTime = 0f;
            MoveMino(m_mino.GoToLeft, m_mino.GoToRight);
            m_isLeftButtonPressed = PressType.HalfPressed;
        }
        else if (Gamepad.current.dpad.right.wasPressedThisFrame)
        {
            m_moveCurrentTime = 0f;
            MoveMino(m_mino.GoToRight, m_mino.GoToLeft);
            m_isRightButtonPressed = PressType.HalfPressed;
        }

        if (Gamepad.current.dpad.left.wasReleasedThisFrame)
        {
            m_isLeftButtonPressed = PressType.NonPressed;
        }

        if (Gamepad.current.dpad.right.wasReleasedThisFrame)
        {
            m_isRightButtonPressed = PressType.NonPressed;
        }

        if (Gamepad.current.dpad.down.wasPressedThisFrame)
        {
            m_dropCurrentTime = 0f;
            MoveMino(m_mino.Drop, m_mino.UnDrop);
            m_isDownButtonPressed = true;

        }
        if (Gamepad.current.dpad.down.wasReleasedThisFrame)
        {
            m_isDownButtonPressed = false;
        }

        if (Gamepad.current.aButton.wasPressedThisFrame)
        {
            MoveMino(m_mino.RightRot,m_mino.LeftRot);
        }

        if (Gamepad.current.bButton.wasPressedThisFrame)
        {
            MoveMino(m_mino.LeftRot, m_mino.RightRot);
        }

        if (Gamepad.current.dpad.up.wasPressedThisFrame)
        {
            while (MoveMino(m_mino.Drop, m_mino.UnDrop, true));
        }
    }

    private void PrintStatus()
    {
        for (int x = 0; x < STAGE_MAX_X; x++)
        {
            for (int y = 0; y < STAGE_MAX_Y; y++)
            {
                m_stage[x, y].DyeBlock();
                // ƒXƒe[ƒW‚ÌƒuƒƒbƒNó‘Ô‚É‡‚í‚¹‚Ä•`ŽÊ‚·‚éB
            }
        }
    }

    private void PutMino(bool isErase = false, bool isGhost = false) // ƒAƒNƒeƒBƒ”ƒ~ƒm‚ÌˆÚ“®‚ÌŒã‰æ‘œ‚ðƒNƒŠƒA‚·‚éˆ—‚ð“ü‚ê‚éB
    {
        BlockType blockType = BlockType.Empty;
        if (isErase)
        {
            blockType = BlockType.Empty;
        }
        else
        {
            blockType = m_mino.Type;
        }

        bool[,] mino = m_mino.Shape;
        for (int x = 0; x < m_mino.Size; x++)
        {
            for (int y = 0; y < m_mino.Size; y++)
            {
                if (mino[x, y])
                {
                    if (isGhost)
                    {
                        m_stage[m_mino.GhostPosX + x, m_mino.GhostPosY + y].BlockType = blockType;
                    }
                    else
                    {
                        m_stage[m_mino.PosX + x, m_mino.PosY + y].BlockType = blockType;
                    }
                } // ƒAƒNƒeƒBƒ”ƒ~ƒm‚ÌŒ»ÝˆÊ’u‚ðŽn“®‚ÉŠY“–”ÍˆÍ“à‚ÌƒuƒƒbƒN‚ÌƒXƒe[ƒ^ƒX‚ð•ÏX‚·‚éB
            }
        }
    }

    private bool IsCanPut()
    {
        for (int x = 0; x < m_mino.Size; x++)
        {
            for (int y = 0; y < m_mino.Size; y++)
            {
                if (m_mino.Shape[x, y])
                {
                    if (m_stage[m_mino.PosX + x, m_mino.PosY + y].BlockType != BlockType.Empty)
                    {
                        Debug.Log((m_mino.PosX + x) + ","+ (m_mino.PosY + y) + "‚É‚Í’u‚¯‚È‚¢‚æ");
                        return false;
                    }
                }
            }
        }
        return true;
    }

    // Minoã¨å‹•ãæ–¹å‘ã‚’å¼•æ•°ã«æ¸¡ã™ã¨ã€ãã®æ–¹å‘ã«å‹•ã‘ã‚‹ã‹ã‚’è¿”ã™
    private bool CalcPutMino(Mino mino, Mino.MoveType moveType)
    {
        // è¨ˆç®—ç”¨Minoã‚¯ãƒ©ã‚¹ã‚’è¤‡è£½
        var calcMino = new Mino();
        calcMino = mino;

        // è¨ˆç®—ç”¨Minoã‚’æŒ‡å®šæ–¹å‘ã«å‹•ã‹ã™
        switch(moveType)
        {
            case Mino.MoveType.Left:
                calcMino.GoToLeft();
                break;
            case Mino.MoveType.Right:
                calcMino.GoToRight();
                break;
            case Mino.MoveType.Drop:
                calcMino.Drop();
                break;
        }

        // è¨ˆç®—ç”¨MinoãŒç½®ã‘ã‚‹çŠ¶æ…‹ã‹æ¤œè¨¼
        for (int x = 0; x < calcMino.Size; x++)
        {
            for (int y = 0; y < calcMino.Size; y++)
            {
                if (calcMino.Shape[x, y])
                {
                    if (m_stage[calcMino.PosX + x, calcMino.PosY + y].BlockType != BlockType.Empty)
                    {
                        Debug.Log((calcMino.PosX + x) + ","+ (calcMino.PosY + y) + "ï¿½É‚Í’uï¿½ï¿½ï¿½È‚ï¿½ï¿½ï¿½");
                        return false;
                    }
                }
            }
        }
        return true;
    }
    
    private bool MoveMino(Action Doing, Action Undoing, bool isLanding = false, bool isGhost = false)
    {// to do
        bool result = false;

        PutMino(true, isGhost);
        Doing();
        if (IsCanPut())
        {
            PutMino(false,isGhost);
            result = true;
        }
        else
        {
            Undoing();
            PutMino(false,isGhost);
            if (isLanding)
            {
                m_totalClearedLines += ClearLine();
                CreateMino();
            }
        }
        PrintStatus();
        return result;
    }

    private void WaitControl()
    {
        if (m_isDownButtonPressed)
        {
            m_dropCurrentTime += Time.deltaTime;
            if (m_dropCurrentTime > m_minoMoveSpan)
            {
                m_dropCurrentTime = 0f;
                MoveMino(m_mino.Drop, m_mino.UnDrop);
            }
        }

        if (m_isLeftButtonPressed == PressType.HalfPressed)
        {
            m_moveCurrentTime += Time.deltaTime;
            if (m_moveCurrentTime > m_buttonPressedJudgeSpan)
            {
                m_isLeftButtonPressed = PressType.StrongPressed;
                m_moveCurrentTime = 0f;
            }
        }

        if (m_isLeftButtonPressed == PressType.StrongPressed)
        {
            m_moveCurrentTime += Time.deltaTime;
            if (m_moveCurrentTime > m_minoMoveSpan)
            {
                m_moveCurrentTime = 0f;
                MoveMino(m_mino.GoToLeft, m_mino.GoToRight);
            }
        }

        if (m_isRightButtonPressed == PressType.HalfPressed)
        {
            m_moveCurrentTime += Time.deltaTime;
            if (m_moveCurrentTime > m_buttonPressedJudgeSpan)
            {
                m_isRightButtonPressed = PressType.StrongPressed;
                m_moveCurrentTime = 0f;
            }
        }

        if (m_isRightButtonPressed == PressType.StrongPressed)
        {
            m_moveCurrentTime += Time.deltaTime;
            if (m_moveCurrentTime > m_minoMoveSpan)
            {
                m_moveCurrentTime = 0f;
                MoveMino(m_mino.GoToRight, m_mino.GoToLeft);
            }
        }
    }

    private void PutGhost() // to do
    {
        //while (MoveMino(m_mino.Drop, m_mino.UnDrop,false,true));// to do
    }
    
    private int ClearLine()
    {
        int retClearLines = 0;
        for (int y = 0; y < STAGE_MAX_Y - 1; y++)
        {
            bool isFilledLine = true;
            for (int x = 1; x < STAGE_MAX_X - 1; x++)
            {
                if (m_stage[x, y].BlockType == BlockType.Empty)
                {
                    isFilledLine = false;
                }
            }
            if (isFilledLine)
            {
                retClearLines++;
                Debug.Log("y‚Ì’l‚Í"+y);
                for (int y2 = y; y2 > 0; y2--)
                {
                    for (int x2 = 1; x2 < STAGE_MAX_X - 1; x2++)
                    {
                        m_stage[x2, y2].BlockType = m_stage[x2, y2 - 1].BlockType;
                    }
                }
            }
        }
        return retClearLines;
    }
}
