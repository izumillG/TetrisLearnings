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
    private const int STAGE_MAX_X = 12;
    private const int STAGE_MAX_Y = 21;
    private bool m_isGameOver = false;
    private bool m_isDownButtonPressed = false;
    private int m_totalClearedLines = 0;

    private PressType m_isLeftButtonPressed     = PressType.NonPressed;
    private PressType m_isRightButtonPressed    = PressType.NonPressed;

    [SerializeField] private float m_buttonPressedJudgeSpan = 0.3f;
    [SerializeField] private float m_minoMoveSpan = 0.05f;
    [SerializeField] private int m_speedMaximizeLineNum = 40;
    [SerializeField] private float m_minDropSpeed = 1f;
    [SerializeField] private float m_maxDropSpeed = 0.2f;
    private float m_dropSpeedCoefficient = 0;

    private float m_moveCurrentTime = 0f;

    private MinoQueue m_queue = new MinoQueue();

    [SerializeField] private Block m_blockObject = null;
    private Block[,] m_stage = new Block[STAGE_MAX_X, STAGE_MAX_Y];

    void Start()
    {
        Initialize();
    }


    #region Initialize

    private void Initialize()
    {
        GenerateStage();

        CreateWall();
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

    private void CreateWall()
    {
        for (int x = 0; x < STAGE_MAX_X; x++)
        {
            for (int y = 0; y < STAGE_MAX_Y; y++)
            {
                if (x == 0 || x == STAGE_MAX_X - 1 || y == STAGE_MAX_Y - 1)
                {
                    m_stage[x, y].BlockType = BlockType.Wall;
                }
            }
        }
    }

    #endregion


    private Mino m_mino = null;

    void Update()
    {
        if (m_isGameOver)
        {
            Debug.Log("�����܂��I�@" + m_totalClearedLines + "���C�������܂����B�܂��V��łˁI");
            return;
        }

        CreateMino();

        DropSpeedManager(m_totalClearedLines);

        ControlMino();
        
        //WaitControl();

        PrintStatus();
    }

    private void CreateMino()
    {
        if(m_mino != null)
        {
            // 操作中のminoが存在するので終了
            return;
        }

        m_mino = new Mino();
        m_mino.GenerateMino(m_queue.GetNextMino());
        if (IsCanPut())
        {
            PutMino();
            PutGhost();
        }
        else
        {
            m_isGameOver = true;
            Debug.Log("�~�m���o���Ȃ�");
            return;
        }

        PrintStatus();
        m_moveCurrentTime = 0f;
    }

    private void LateUpdate()
    {
        UpdateDropMino();
    }

    private float m_refreshSpan = 1f;
    private float m_dropCurrentTime = 0f;
    private void UpdateDropMino()
    {
        if (m_mino == null)
        {
            // 操作中のminoが存在しない
            return;
        }

        if (m_isDownButtonPressed)
        {
            // 下入力が入っている場合は処理しない
            return;
        }

        m_dropCurrentTime += Time.deltaTime;
        if (m_dropCurrentTime > m_refreshSpan)
        {
            MoveMino(m_mino, Mino.MoveType.Drop);
        }
    }

    private void DropSpeedManager(int totalClearedLines)
    {
        m_dropSpeedCoefficient = (m_minDropSpeed - m_maxDropSpeed) / m_speedMaximizeLineNum;
        m_refreshSpan = m_totalClearedLines > m_speedMaximizeLineNum ? m_maxDropSpeed : m_minDropSpeed - m_dropSpeedCoefficient * totalClearedLines;
    }


    #region ControlMino

    private float HoldKeyTime = 0;
    private Mino.MoveType HoldKeyType = Mino.MoveType.None;
    private bool _isHoldKey = false;
    private bool IsHodlKey
    {
        get
        {
            return _isHoldKey;
        }

        set 
        {
            if(value == false)
            {
                HoldKeyTime = 0;
            }
            _isHoldKey = value;
        }
    }

    private bool IsMoveHoldKey
    {
        get
        {
            return HoldKeyTime > m_buttonPressedJudgeSpan;
        }
    }

    private void ControlMino()
    {
        OnPressedLeft();
        OnPressedRight();
        OnPressedDown();
        OnPressedUp();
        OnPressedAButton();
        OnPressedBButton();

        OnReleasedLeft();
        OnReleasedRight();
        OnReleasedDown();

        OnPressHold();
    }

    #region InputSystem

    private Gamepad CurrentGamePad
    {
        get
        {
            return Gamepad.current;
        }
    }

    private Keyboard CurrentKeyboard
    {
        get
        {
            return Keyboard.current;
        }
    }

    #region InputLeft

    private bool InputPressedLeft
    {
        get
        {
            if (CurrentGamePad != null)
            {
                return CurrentGamePad.dpad.left.wasPressedThisFrame;
            }

            if(CurrentKeyboard != null)
            {
                return Keyboard.current.leftArrowKey.wasPressedThisFrame;
            }

            return false;
        }
    }

    private bool InputReleasedLeft
    {
        get
        {
            if (CurrentGamePad != null)
            {
                return CurrentGamePad.dpad.left.wasReleasedThisFrame;
            }

            if (CurrentKeyboard != null)
            {
                return Keyboard.current.leftArrowKey.wasReleasedThisFrame;
            }

            return false;
        }
    }

    #endregion

    #region InputRight

    private bool InputPressedRight
    {
        get
        {
            if (CurrentGamePad != null)
            {
                return CurrentGamePad.dpad.right.wasPressedThisFrame;
            }

            if (CurrentKeyboard != null)
            {
                return Keyboard.current.rightArrowKey.wasPressedThisFrame;
            }

            return false;
        }
    }

    private bool InputReleasedRight
    {
        get
        {
            if (CurrentGamePad != null)
            {
                return CurrentGamePad.dpad.right.wasReleasedThisFrame;
            }

            if (CurrentKeyboard != null)
            {
                return Keyboard.current.rightArrowKey.wasReleasedThisFrame;
            }

            return false;
        }
    }

    #endregion

    #region InputDown

    private bool InputPressedDown
    {
        get
        {
            if (CurrentGamePad != null)
            {
                return CurrentGamePad.dpad.down.wasPressedThisFrame;
            }

            if (CurrentKeyboard != null)
            {
                return Keyboard.current.downArrowKey.wasPressedThisFrame;
            }

            return false;
        }
    }

    private bool InputReleasedDown
    {
        get
        {
            if (CurrentGamePad != null)
            {
                return CurrentGamePad.dpad.down.wasReleasedThisFrame;
            }

            if (CurrentKeyboard != null)
            {
                return Keyboard.current.downArrowKey.wasReleasedThisFrame;
            }

            return false;
        }
    }

    #endregion

    #region InputUp

    private bool InputPressedUp
    {
        get
        {
            if (CurrentGamePad != null)
            {
                return CurrentGamePad.dpad.up.wasPressedThisFrame;
            }

            if (CurrentKeyboard != null)
            {
                return Keyboard.current.upArrowKey.wasPressedThisFrame;
            }

            return false;
        }
    }

    #endregion

    #region InputButton

    private bool InputPressedAButton
    {
        get
        {
            if (CurrentGamePad != null)
            {
                return CurrentGamePad.aButton.wasPressedThisFrame;
            }

            if (CurrentKeyboard != null)
            {
                return Keyboard.current.spaceKey.wasPressedThisFrame;
            }

            return false;
        }
    }

    private bool InputPressedBButton
    {
        get
        {
            if (CurrentGamePad != null)
            {
                return CurrentGamePad.bButton.wasPressedThisFrame;
            }

            if (CurrentKeyboard != null)
            {
                return Keyboard.current.leftAltKey.wasPressedThisFrame;
            }

            return false;
        }
    }

    #endregion

    #endregion

    #region InputEvent

    private void OnPressedLeft()
    {
        if (IsHodlKey)
        {
            return;
        }

        if (!InputPressedLeft || InputPressedRight)
        {
            return;
        }

        m_moveCurrentTime = 0f;
        MoveMino(m_mino, Mino.MoveType.Left);
        m_isLeftButtonPressed = PressType.HalfPressed;


        IsHodlKey = true;
        HoldKeyType = Mino.MoveType.Left;
    }

    private void OnReleasedLeft()
    {
        if (HoldKeyType != Mino.MoveType.Left)
        {
            return;
        }

        if (InputReleasedLeft)
        {
            m_isLeftButtonPressed = PressType.NonPressed;

            IsHodlKey = false;
            HoldKeyType = Mino.MoveType.None;
        }
    }

    private void OnPressedRight()
    {
        if (IsHodlKey)
        {
            return;
        }

        if (!InputPressedRight || InputPressedLeft)
        {
            return;
        }

        m_moveCurrentTime = 0f;
        MoveMino(m_mino, Mino.MoveType.Right);
        m_isRightButtonPressed = PressType.HalfPressed;

        IsHodlKey = true;
        HoldKeyType = Mino.MoveType.Right;
    }

    private void OnReleasedRight()
    {
        if(HoldKeyType != Mino.MoveType.Right)
        {
            return;
        }

        if (InputReleasedRight)
        {
            m_isRightButtonPressed = PressType.NonPressed;

            IsHodlKey = false;
            HoldKeyType = Mino.MoveType.None;
        }
    }

    private void OnPressedDown()
    {
        if(IsHodlKey)
        {
            return;
        }

        if (InputPressedDown)
        {
            m_dropCurrentTime = 0f;
            MoveMino(m_mino, Mino.MoveType.Drop);
            m_isDownButtonPressed = true;

            IsHodlKey = true;
            HoldKeyType = Mino.MoveType.Drop;
        }
    }

    private void OnReleasedDown()
    {
        if (HoldKeyType != Mino.MoveType.Drop)
        {
            return;
        }

        if (InputReleasedDown)
        {
            m_isDownButtonPressed = false;

            IsHodlKey = false;
            HoldKeyType = Mino.MoveType.None;
        }
    }

    private void OnPressedUp()
    {
        if (InputPressedUp)
        {
            while (MoveMino(m_mino.Drop, m_mino.UnDrop, true)) ;
        }
    }

    private void OnPressedAButton()
    {
        if (InputPressedAButton)
        {
            MoveMino(m_mino.RightRot, m_mino.LeftRot);
        }
    }

    private void OnPressedBButton()
    {
        if (InputPressedBButton)
        {
            MoveMino(m_mino.LeftRot, m_mino.RightRot);
        }
    }


    private const float resetHoldKeyTime = 0.2f;
    private void OnPressHold()
    {
        if (IsHodlKey)
        {
            HoldKeyTime += Time.deltaTime;
        }

        if(IsMoveHoldKey)
        {
            switch (HoldKeyType)
            {
                case Mino.MoveType.Left:
                    MoveMino(m_mino, Mino.MoveType.Left);
                    break;
                case Mino.MoveType.Right:
                    MoveMino(m_mino, Mino.MoveType.Right);
                    break;
                case Mino.MoveType.Drop:
                    MoveMino(m_mino, Mino.MoveType.Drop);
                    break;
            }

            HoldKeyTime = resetHoldKeyTime;
        }
    }

    #endregion

    #endregion


    private void MoveMino(Mino mino, Mino.MoveType moveType)
    {
        if (mino == null)
        {
            return;
        }

        switch (moveType)
        {
            case Mino.MoveType.Left:
                MoveTypeLeft(mino);
                break;
            case Mino.MoveType.Right:
                MoveTypeRight(mino);
                break;
            case Mino.MoveType.Drop:
                MoveTypeDrop(mino);
                break;
        }
    }

    private void MoveTypeLeft(Mino mino)
    {
        PutMino(true, false);
        if (CanMoveMino(mino, Mino.MoveType.Left))
        {
            mino.GoToLeft();
        }
        PutMino(false, false);
    }

    private void MoveTypeRight(Mino mino)
    {
        PutMino(true, false);
        if (CanMoveMino(mino, Mino.MoveType.Right))
        {
            mino.GoToRight();
        }
        PutMino(false, false);
    }

    private void MoveTypeDrop(Mino mino)
    {
        PutMino(true, false);
        if (CanMoveMino(mino, Mino.MoveType.Drop))
        {
            mino.Drop();
            m_dropCurrentTime = 0f;
            PutMino(false, false);
            return;
        }

        // minoを置いてライン成立判定
        PutMino(false, false);
        m_totalClearedLines += ClearLine();

        // 操作中のminoを破棄
        m_mino = null;
    }

    /// <summary>
    /// Minoと動く方向を引数に渡すと、その方向に動けるかを返す
    /// </summary>
    /// <param name="mino"></param>
    /// <param name="moveType"></param>
    /// <returns></returns>
    private bool CanMoveMino(Mino mino, Mino.MoveType moveType)
    {
        // 計算用Minoクラスを複製
        var calcMino = new Mino();
        calcMino.PosX = mino.PosX;
        calcMino.PosY = mino.PosY;
        calcMino.Size = mino.Size;
        calcMino.Shape = mino.Shape;

        // 計算用Minoを指定方向に動かす
        switch (moveType)
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

        // 計算用Minoが置ける状態か検証
        for (int x = 0; x < calcMino.Size; x++)
        {
            for (int y = 0; y < calcMino.Size; y++)
            {
                if (calcMino.Shape[x, y])
                {
                    if (m_stage[calcMino.PosX + x, calcMino.PosY + y].BlockType != BlockType.Empty)
                    {
                        Debug.Log((calcMino.PosX + x) + "," + (calcMino.PosY + y) + "�ɂ͒u���Ȃ���");
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
            PutMino(false, isGhost);
            result = true;
        }
        else
        {
            Undoing();
            PutMino(false, isGhost);
            if (isLanding)
            {
                m_totalClearedLines += ClearLine();

                // 操作中のminoを破棄
                m_mino = null;
            }
        }
        PrintStatus();
        return result;
    }



    private void PrintStatus()
    {
        for (int x = 0; x < STAGE_MAX_X; x++)
        {
            for (int y = 0; y < STAGE_MAX_Y; y++)
            {
                m_stage[x, y].DyeBlock();
            }
        }
    }

    private void PutMino(bool isErase = false, bool isGhost = false)
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
                }
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
                        Debug.Log((m_mino.PosX + x) + ","+ (m_mino.PosY + y) + "�ɂ͒u���Ȃ���");
                        return false;
                    }
                }
            }
        }
        return true;
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
                Debug.Log("y�̒l��"+y);
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
