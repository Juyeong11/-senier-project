using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


public class InGameOtherPlayerManager : MonoBehaviour
{
    public Animator JumpTrigger;
    public UnityEvent onPlayerStand;
    public UnityEvent onPlayerFly;

    private bool isFly = false;
    bool isReady = false;

    public playerState state;
    public PlayerKeyHandler handle;     //Invalid

    public HexCellPosition selfCoord;
    public HexDirection selfDirection;
    public HexGrid grid;

    public Transform PlayerTransform;

    public void Start()
    {
        grid = GameManager.data.grid;
        state = playerState.Idle;
        selfDirection = HexDirection.Up;
        //onPlayerFly.Invoke();
        isFly = true;
    }
    public void LoginOk()
    {
        grid = GameManager.data.grid;
        state = playerState.Idle;
        selfDirection = HexDirection.Up;
        //onPlayerFly.Invoke();
        isFly = true;
    }

    public void JumpTrig()
    {
        JumpTrigger.SetTrigger("Jump");
    }

    void Update()
    {
        //DEBUG
        //if (GameManager.data.isGameStart)
        //    resetPosition();
        //else
        //   gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y -2.f*Time.deltaTime, gameObject.transform.position.z);

        /*        if (gameObject.transform.position.y < -0.1f)
                {
                    if (isFly)
                    {
                        onPlayerStand.Invoke();
                        isFly = false;
                    }
                }*/

        //if (GameManager.data.Net.isOnline)
        //{
        //    GameManager.data.PlaySound();
        //}

        if (GameManager.data.isGameStart)
        {
            PlayerWCheck();
            //PlayerPortalCheck();
        }
    }
    public void PlayerPortalCheck()
    {
        // Ŭ�󿡼� �÷��̾ ��Ż�� �ִ��� ��ġ �˻� �� �´ٸ� -> ������ ��Ŷ ���� -> �������� �ش� ��ǥ�� ��Ż�� �´��� Ȯ��
        // -> �´ٸ� ready���� ����Ʈ ��Ŷ ���� -> 3���� �� �غ�Ǹ� �� ��ȯ

        // ��Ż Ÿ������ Ȯ�� ������ 0,0,0     1,0,-1     0,1,-1�� �� -> �� ��ȯ �ߵǸ� Cell�� type �߰��� ���ϴ� ������� �ٲ���
        if(selfCoord.coordinates.X == 3 && selfCoord.coordinates.Z == -3 ||
            selfCoord.coordinates.X == 3 && selfCoord.coordinates.Z == -2 ||
            selfCoord.coordinates.X == 2 && selfCoord.coordinates.Z == -2)
        {
            if (isReady) return;
            //FieldGameManager.Net.SendChangeSceneReadyPacket(1);
            isReady = true;
            return;
        }
        else if (isReady)
        {
            //FieldGameManager.Net.SendChangeSceneReadyPacket(0);
        }
        isReady = false;
    }

    public void PlayerWCheck()
    {
        if (selfCoord.coordinates.W != grid.cellMaps.Get(selfCoord.coordinates).w + 1)
        {
            Debug.LogError(">Player W Coordinate Error!<");
            print("Self W : " + selfCoord.coordinates.W);
            print("Self W : " + grid.cellMaps.Get(selfCoord.coordinates).w);
            selfCoord.coordinates.W = grid.cellMaps.Get(selfCoord.coordinates).w + 1;
        }
        PlayerTransform.position = selfCoord.calculatePlayerPosition();

        //Debug.Log("z : " + gameObject.transform.position.z);

    }

    public void PlayerRotateToLookAt()
    {
        switch(selfDirection)
        {
            case HexDirection.LeftUp:
                //�þ� ���͸� �����ؾ��� ������ �����ϸ� �翬�� Ʋ����
                //Vector3 c = Vector3.Cross(transform.rotation.eulerAngles, new Vector3(0, -120, 0));
                //transform.Rotate(0, c.x * 3f, 0);

                transform.rotation = Quaternion.Euler(new Vector3(0, -120, 0));
                break;
            case HexDirection.Up:
                transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
                break;
            case HexDirection.RightUp:
                transform.rotation = Quaternion.Euler(new Vector3(0, -30, 0));
                break;
            case HexDirection.Down:
                transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                break;
            case HexDirection.LeftDown:
                transform.rotation = Quaternion.Euler(new Vector3(0, 120, 0));
                break;
            case HexDirection.RightDown:
                transform.rotation = Quaternion.Euler(new Vector3(0, 30, 0));
                break;
        }
    }

    public void Beat()
    {
        //Debug.Log("BEAT");
        grid.pPosition = selfCoord.coordinates;
        selfCoord.beat();
    }

    bool KeyCheck(KeyCode k)
    {
        switch (k)
        {
            case KeyCode.Q:
                selfDirection = HexDirection.LeftUp;
                break;
            case KeyCode.W:
                selfDirection = HexDirection.Up;
                break;
            case KeyCode.E:
                selfDirection = HexDirection.RightUp;
                break;
            case KeyCode.A:
                selfDirection = HexDirection.LeftDown;
                break;
            case KeyCode.S:
                selfDirection = HexDirection.Down;
                break;
            case KeyCode.D:
                selfDirection = HexDirection.RightDown;
                break;
        }

            return true;
    }
    public void PlayerMove(int x, int y, int z)
    {
        //GameManager.data.setMoved();
        // if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).state != cellState.None)
        selfCoord.SetPosition(x, y, z);
        //selfDirection = HexDirection.LeftUp;
    }
    void KeyHandler()
    {
        if (Input.GetKeyDown(KeyCode.Q) && KeyCheck(KeyCode.Q))
        {
            if (FieldGameManager.Net.isOnline)
            {
                //Debug.Log("Ű ����");
                // ������ �̵� ����
                //FieldGameManager.data.setMoved();

                FieldGameManager.Net.SendMovePacket((byte)Protocol.DIR.LEFTUP);
            }
            else
            {
                //FieldGameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(-1, 0, 1, grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.W) && KeyCheck(KeyCode.W))
        {
            if (FieldGameManager.Net.isOnline)
            {
                //GameManager.data.setMoved();
                // ������ �̵� ����
                FieldGameManager.Net.SendMovePacket((byte)Protocol.DIR.UP);
            }
            else
            {
                //GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z + 1).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z + 1).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(0, -1, 1, grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y -1, selfCoord.coordinates.Z + 1).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.E) && KeyCheck(KeyCode.E))
        {
            if (FieldGameManager.Net.isOnline)
            {
                //GameManager.data.setMoved();
                // ������ �̵� ����
                FieldGameManager.Net.SendMovePacket((byte)Protocol.DIR.RIGHTUP);
            }
            else
            {
                //FieldGameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(1, -1, 0, grid.cellMaps.Get(selfCoord.coordinates.X+1, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                    
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.A) && KeyCheck(KeyCode.A))
        {
            if (FieldGameManager.Net.isOnline)
            {
                //GameManager.data.setMoved();
                // ������ �̵� ����
                FieldGameManager.Net.SendMovePacket((byte)Protocol.DIR.LEFTDOWN);
            }
            else
            {
                //FieldGameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(-1, 1, 0, grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.S) && KeyCheck(KeyCode.S))
        {
            if (FieldGameManager.Net.isOnline)
            {
                //GameManager.data.setMoved();
                // ������ �̵� ����
                FieldGameManager.Net.SendMovePacket((byte)Protocol.DIR.DOWN);
            }
            else
            {
                //GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z - 1).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z -1).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(0, 1, -1, grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z-1).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.D) && KeyCheck(KeyCode.D))
        {
            if (FieldGameManager.Net.isOnline)
            {
                //GameManager.data.setMoved();
                // ������ �̵� ����
                FieldGameManager.Net.SendMovePacket((byte)Protocol.DIR.RIGHTDOWN);
            }
            else
            {
                //GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z - 1).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z - 1).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(1, 0, -1, grid.cellMaps.Get(selfCoord.coordinates.X+1, selfCoord.coordinates.Y, selfCoord.coordinates.Z - 1).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                }
            }
        }

    }
}
