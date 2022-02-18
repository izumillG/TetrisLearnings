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
    private const int STAGE_MAX_X = 12; // �X�e�[�W�̉�����10�s�ɕǃu���b�N2�B
    private const int STAGE_MAX_Y = 21; // �X�e�[�W�̏c��20�s�ɏ��u���b�N1�B
    private bool m_isGameOver = false;
    private bool m_isDownButtonPressed = false;
    private int m_totalClearedLines = 0;

    private PressType m_isLeftButtonPressed     = PressType.NonPressed;
    private PressType m_isRightButtonPressed    = PressType.NonPressed;

    [SerializeField] private float m_buttonPressedJudgeSpan = 0.3f; // ����������̎���
    [SerializeField] private float m_minoMoveSpan = 0.05f; // �u���b�N�ړ��̊Ԋu
    [SerializeField] private int m_speedMaximizeLineNum = 40; // �������x���ő�ɂȂ郉�C����
    [SerializeField] private float m_minDropSpeed = 1f; // ���������̊�{���x
    [SerializeField] private float m_maxDropSpeed = 0.2f; // ���������̍ő呬�x
    private float m_refreshSpan = 1f; // ���������̊�{���x
    private float m_dropSpeedCoefficient = 0; // �������x�W��

    private float m_dropCurrentTime = 0f; // �����ɂ��Ẵt���[�����Ƃ̕b���̃J�E���^�[
    private float m_moveCurrentTime = 0f; // ���E�ړ��ɂ��Ẵt���[�����Ƃ̕b���̃J�E���^�[
    private bool m_isFirstUpdate = true;

    private Mino m_mino = new Mino(); // �������̃~�m�̃C���X�^���X
    private MinoQueue m_queue = new MinoQueue();

    [SerializeField] private Block m_blockObject = null; // prefab��ǂݍ���Unity�̏���������B
    private Block[,] m_stage = new Block[STAGE_MAX_X, STAGE_MAX_Y];
    // �u���b�N�̔z��ɂ��t�B�[���h�̐����B���j�^�[�̃C���[�W�B
    void Start()
    {
        GenerateStage();
        CreateWall();
    }
    void Update()
    {
        if (m_isGameOver)
        {
            Debug.Log("�����܂��I�@" + m_totalClearedLines +"���C�������܂����B�܂��V��łˁI");
            return;
        }

        if (m_isFirstUpdate) // ���݂̏����ł�1�b�ڂ��󔒂ɂȂ邽�ߕʓr1�b�ڐ�p�̏����𑖂点��B
        {
            CreateMino();
            m_isFirstUpdate = false;
        }

        DropSpeedManager(m_totalClearedLines);
        ControlMino();
        WaitControl();

        if (!m_isDownButtonPressed)
        {
            m_dropCurrentTime += Time.deltaTime; // 1�b���Ƃɏ����𑖂点�邽�߂̃M�~�b�N�B
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
                    // �ǂɊY������ӏ������u���b�N�^�C�v��ǂɕύX
                }
            }
        }
    }
    private void CreateMino()
    {
        m_mino.GenerateMino(m_queue.GetNextMino()); // �����ʒu�Ƀ~�m�𐶐�����B
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
                // �X�e�[�W�̃u���b�N��Ԃɍ��킹�ĕ`�ʂ���B
            }
        }
    }

    private void PutMino(bool isErase = false, bool isGhost = false) // �A�N�e�B���~�m�̈ړ��̌�摜���N���A���鏈��������B
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
                } // �A�N�e�B���~�m�̌��݈ʒu���n���ɊY���͈͓��̃u���b�N�̃X�e�[�^�X��ύX����B
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
