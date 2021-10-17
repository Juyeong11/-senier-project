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
    public enemyState state;
    public int enemyHp;
    public int enemyMaxHp;

    public HexCellPosition selfCoord;
    public HexGrid grid;

    public List<EMoving> enemyMovingList;
    public List<EAttack> enemyAttackList;
    public List<ENote> enemyNoteList;


    void Start()
    {
        grid = GameManager.data.grid;
        state = enemyState.Idle;
    }

    void Update()
    {
        
    }

    public void PatternServe()
    {
        //���� ��ȣ���� �ִϸ��̼� ��
        grid.EnemyAttack(PatternManager.data.CastedPattern);
    }
}
