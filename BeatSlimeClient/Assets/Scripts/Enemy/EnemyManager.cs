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
    public Animator selfAnim;

    public enemyState state;
    public int enemyHp;
    public int enemyMaxHp;

    public HexCellPosition selfCoord;
    public HexGrid grid;

    //Invalid
    public List<EMoving> enemyMovingList;
    public List<EAttack> enemyAttackList;

    //
    public List<BeatBall> enemyNoteList;


    void Start()
    {
        grid = GameManager.data.grid;
        state = enemyState.Idle;
    }

    void Update()
    {
        reflectPosition();
        EnemyWCheck();
    }

    public void EnemyWCheck()
    {
        //Debug.LogError(">Player W Coordinate Error!< [ Self W : " + selfCoord.coordinates.W + ", Cell W : " + grid.cellMaps.Get(selfCoord.coordinates).w + 1 + " ]");
        //print("Self W : " + selfCoord.coordinates.W);
        selfCoord.coordinates.W = grid.cellMaps.Get(selfCoord.coordinates).w + 1;
        //PlayerTransform.position = calculatePlayerPosition();

        //Debug.Log("z : " + gameObject.transform.position.z);

    }

    public void Beat()
    {
        grid.ePosition = selfCoord.coordinates;
    }

    public void PatternSet()
    {
        //���⼭ �ִϸ��̼� ó��
        grid.WarningCell(PatternManager.data.SettedPattern);

        //��Ʈ Ÿ�Ե��� �ٸ� �ִϸ��̼� ���� �� ���� ������ �� ��� �� (id ���)
    }

    public void PatternServe()
    {
        //���⼭ ���ǵ����� ó��
        grid.EnemyAttack(PatternManager.data.CastedPattern);
    }

    public void BeatPatternServe(Beat NowBeat,Beat offset,GameObject destination)
    {
        selfAnim.SetTrigger("Attack");

        //������Ʈ Ǯ ���� Ǯ���ϱ�
        enemyNoteList[0].Init(NowBeat, offset, gameObject, destination);
    }

    public void reflectPosition()
    {
        selfCoord.reflectPosition();
    }
}
