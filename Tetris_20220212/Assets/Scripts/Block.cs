using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    Empty,
    Wall,
    MinoT,
    MinoS,
    MinoZ,
    MinoL,
    MinoJ,
    MinoO,
    MinoI,
    BlockTypeMax
}

public class Block : MonoBehaviour
{
    public BlockType BlockType { get; set; } = BlockType.Empty;

    [SerializeField] private Color32 m_minoColorWall    = Color.white;
    [SerializeField] private Color32 m_minoColorT       = Color.white;
    [SerializeField] private Color32 m_minoColorS       = Color.white;
    [SerializeField] private Color32 m_minoColorZ       = Color.white;
    [SerializeField] private Color32 m_minoColorL       = Color.white;
    [SerializeField] private Color32 m_minoColorJ       = Color.white;
    [SerializeField] private Color32 m_minoColorO       = Color.white;
    [SerializeField] private Color32 m_minoColorI       = Color.white;

    private MeshRenderer m_meshRenderer = null;
    // Start is called before the first frame update
    void Start()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void DyeBlock(bool isGohst =false)
    {
        if (BlockType == BlockType.Empty)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }
        switch (BlockType)
        {
            case BlockType.Wall:
                m_meshRenderer.material.color = m_minoColorWall;
                break;
            case BlockType.MinoT:
                m_meshRenderer.material.color = m_minoColorT;
                break;
            case BlockType.MinoS:
                m_meshRenderer.material.color = m_minoColorS;
                break;
            case BlockType.MinoZ:
                m_meshRenderer.material.color = m_minoColorZ;
                break;
            case BlockType.MinoL:
                m_meshRenderer.material.color = m_minoColorL;
                break;
            case BlockType.MinoJ:
                m_meshRenderer.material.color = m_minoColorJ;
                break;
            case BlockType.MinoO:
                m_meshRenderer.material.color = m_minoColorO;
                break;
            case BlockType.MinoI:
                m_meshRenderer.material.color = m_minoColorI;
                break;
            default:
                Debug.Log("ミノのタイプが予想される範囲内にないなんてびっくりしたよね。");
                break;
        }
        if (isGohst)
        {
            var color = m_meshRenderer.material.color;
            color.a = 100;
            m_meshRenderer.material.color = color;
        }
    }

}
