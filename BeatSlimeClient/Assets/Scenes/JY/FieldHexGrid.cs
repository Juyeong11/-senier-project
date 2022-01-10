using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FieldHexGrid : MonoBehaviour
{
    public bool MakeMapWithoutMapPacket;

    private int xMaxLength;
    private int yMaxLength;
    private int zMaxLength;
    private int xMinLength;
    private int yMinLength;
    private int zMinLength;

    //Cell ����
    public List<GameObject> cellType;
    public CellMap cellMaps;

    //��ġ
    public HexCoordinates pPosition;
    public HexCoordinates ePosition;

    public List<List<HexCoordinates>> RedZones;

    public FieldHexGrid()
    { 
        // LintJson �ְ� Json���� �о�� ����

        cellMaps = new CellMap();
        xMaxLength = 3;
        yMaxLength = 3;
        zMaxLength = 3;
        xMinLength = -3;
        yMinLength = -3;
        zMinLength = -3;
    }

    public void Beat()
    {
        if (cellMaps.Get(pPosition).state == cellState.Damaged)
        {
            if(!FieldGameManager.Net.isOnline)
            Debug.Log("Player Damaged!");
        }
        foreach(var cell in cellMaps.cellMaps)
        {
            cell.Beat();
        }
    }

    public override string ToString()
    {
        return "override this!";
    }
    public void Start()
    {
        int color = 0;
        RedZones = new List<List<HexCoordinates>>();

        if (MakeMapWithoutMapPacket)
        {
            //�� ����
            for (int x = xMinLength; x <= xMaxLength; ++x)
            {
                for (int y = yMinLength; y <= yMaxLength; ++y)
                {
                    for (int z = zMinLength; z <= zMaxLength; ++z)
                    {
                        if (x + y + z == 0)
                        {
                            //print(cellType[0]);
                            GameObject tmpcell = Instantiate(cellType[color]); // <- ���߿� string name���� �ٲ��?
                            int w = Random.Range(0, 3);
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(x, z, w);
                            tmpcell.name = "cell" + FieldGameManager.data.mapCellid;
                            tmpcell.transform.parent = gameObject.transform;
                            cellMaps.Add(tmpcell, x, y, z, w);

                            Protocol.Map p_tempcell = new Protocol.Map();
                            p_tempcell.type = 0;
                            p_tempcell.x = x;
                            p_tempcell.y = y;
                            p_tempcell.z = z;
                            p_tempcell.w = w;
                            p_tempcell.color = 0;
                            p_tempcell.id = FieldGameManager.data.mapCellid++;

                            FieldGameManager.data.Mapdata.Add(p_tempcell);
                        }
                    }
                }
            }
        }
    }

    public void P_MakeHexMap(Protocol.Map map)
    {
        if (!MakeMapWithoutMapPacket)
        {
            if (map.x + map.y + map.z == 0)
            {
                //print(cellType[0]);
                GameObject tmpcell = Instantiate(cellType[map.color]); // <- ���߿� string name���� �ٲ��?
                tmpcell.GetComponent<HexCellPosition>().setInitPosition(map.x, map.z,map.w);
                tmpcell.name = "cell" + map.id;
                FieldGameManager.data.mapCellid = map.id+1;
                tmpcell.transform.parent = gameObject.transform;
                cellMaps.Add(tmpcell, map.x, map.y, map.z,map.w);
            }
            else
            {
                Debug.LogError(">>InValid HexCoordinate Error From MAP Packet<<");
            }
        }

        else
        {
            Debug.LogError(">>Flag_MakeMapWithoutMapPacket<<");
        }
    }



    //DEBUG----------------------------------------�� �Ʒ��� �� ���ľ���
    Dictionary<int, HexCoordinates> cellStoredPos = new Dictionary<int, HexCoordinates>();

    public void WarningCell(Pattern p)
    {
        //��ġ �����ϸ� �����ؾ���
        HexCoordinates RedZone = new HexCoordinates();

        //DEBUG - �̴�� �θ� �� ��
        if (p.noteType == 1)
        {
            RedZone = pPosition;
        }

        //foreach(var coord in RedZone)
        {
            RedZone.plus(p.pivot.X, p.pivot.Z);
            //Debug.Log(RedZone.ToString());
            cellMaps.Get(RedZone).Warning();
            cellStoredPos.Add(p.id,RedZone);
        }
    }

    public void EnemyAttack(Pattern p)
    {
        //����! ���õ� ��ġ�� �ƴ϶� �ٸ� ��ġ ���� �ȵ�

        Debug.Log(p.rhythmBeat.ToString() + ", " + cellStoredPos[p.id]);

        cellMaps.Get(cellStoredPos[p.id]).Damage(1);
    }
}
