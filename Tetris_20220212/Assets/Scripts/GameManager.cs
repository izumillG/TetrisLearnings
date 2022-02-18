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
    private const int STAGE_MAX_X = 12; // ステージの横幅は10行に壁ブロック2つ。
    private const int STAGE_MAX_Y = 21; // ステージの縦幅20行に床ブロック1つ。
    private bool m_isGameOver = false;
    private bool m_isDownButtonPressed = false;
    private int m_totalClearedLines = 0;

    private PressType m_isLeftButtonPressed     = PressType.NonPressed;
    private PressType m_isRightButtonPressed    = PressType.NonPressed;

    [SerializeField] private float m_buttonPressedJudgeSpan = 0.3f; // 長押し判定の時間
    [SerializeField] private float m_minoMoveSpan = 0.05f; // ブロック移動の間隔
    [SerializeField] private int m_speedMaximizeLineNum = 40; // 落下速度が最大になるライン数
    [SerializeField] private float m_minDropSpeed = 1f; // 自動落下の基本速度
    [SerializeField] private float m_maxDropSpeed = 0.2f; // 自動落下の最大速度
    private float m_refreshSpan = 1f; // 自動落下の基本速度
    private float m_dropSpeedCoefficient = 0; // 落下速度係数

    private float m_dropCurrentTime = 0f; // 落下についてのフレームごとの秒数のカウンター
    private float m_moveCurrentTime = 0f; // 左右移動についてのフレームごとの秒数のカウンター
    private bool m_isFirstUpdate = true;

    private Mino m_mino = new Mino(); // 落下中のミノのインスタンス
    private MinoQueue m_queue = new MinoQueue();

    [SerializeField] private Block m_blockObject = null; // prefabを読み込むUnityの処理をする。
    private Block[,] m_stage = new Block[STAGE_MAX_X, STAGE_MAX_Y];
    // ブロックの配列によるフィールドの生成。モニターのイメージ。
    void Start()
    {
        GenerateStage();
        CreateWall();
    }
    void Update()
    {
        if (m_isGameOver)
        {
            Debug.Log("おしまい！　" + m_totalClearedLines +"ライン消しました。また遊んでね！");
            return;
        }

        if (m_isFirstUpdate) // 現在の処理では1秒目が空白になるため別途1秒目専用の処理を走らせる。
        {
            CreateMino();
            m_isFirstUpdate = false;
        }

        DropSpeedManager(m_totalClearedLines);
        ControlMino();
        WaitControl();

        if (!m_isDownButtonPressed)
        {
            m_dropCurrentTime += Time.deltaTime; // 1秒ごとに処理を走らせるためのギミック。
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
                    // 壁に該当する箇所だけブロックタイプを壁に変更
                }
            }
        }
    }
    private void CreateMino()
    {
        m_mino.GenerateMino(m_queue.GetNextMino()); // 初期位置にミノを生成する。
        if (IsCanPut())
        {
            PutMino();
            PutGhost();
        }
        else
        {
            m_isGameOver = true;
            Debug.Log("ミノが出せない");
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
                // ステージのブロック状態に合わせて描写する。
            }
        }
    }

    private void PutMino(bool isErase = false, bool isGhost = false) // アクティヴミノの移動の後画像をクリアする処理を入れる。
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
                } // アクティヴミノの現在位置を始動に該当範囲内のブロックのステータスを変更する。
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
                        Debug.Log((m_mino.PosX + x) + ","+ (m_mino.PosY + y) + "には置けないよ");
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
        while (MoveMino(m_mino.Drop, m_mino.UnDrop,false,true));// to do
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
                Debug.Log("yの値は"+y);
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
