using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public enum enemyState
{
    Idle,
    move,
    guard,
}

public class EnemyManager : MonoBehaviour
{
    private enemyState state;
    public int enemyHp;
    public int enemyMaxHp;

    public HexCellPosition selfCoord;
    public HexGrid grid;

    //��� int�� �ƴ϶� �ٸ��ſ��� �� �ʿ䰡 ��������
    public Dictionary<Beat, int> enemyMovingList;
    public Dictionary<Beat, int> enemyAttackList;
    public Dictionary<Beat, int> enemyNoteList;

    void Start()
    {
        grid = GameManager.data.grid;
        state = enemyState.Idle;
    }

    void Update()
    {
        
    }
}
