using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


public class FieldPlayerManager : MonoBehaviour
{
    public Animator JumpTrigger;
    public UnityEvent onPlayerStand;
    public UnityEvent onPlayerFly;

    private bool isFly = false;
    bool isReady = false;

    public playerState state;
    public PlayerKeyHandler handle;     //Invalid

    public FieldHexCellPosition selfCoord;
    public HexDirection selfDirection;
    public FieldHexGrid grid;

    public Transform PlayerTransform;

    public GameObject PortalPlane;

    public ChattingManager CM;

    public int self_skillnum;
    

    //[System.NonSerialized]
    //public HexCoordinates Destination;

    //List<HexDirection> path = new List<HexDirection>();


    public void Start()
    {
        self_skillnum = 0;
        grid = FieldGameManager.data.grid;
        state = playerState.Idle;
        selfDirection = HexDirection.Up;
        //onPlayerFly.Invoke();
        isFly = true;

    }
    public void LoginOk()
    {
        grid = FieldGameManager.data.grid;
        state = playerState.Idle;
        selfDirection = HexDirection.Up;
        //onPlayerFly.Invoke();
        isFly = true;
    }

    public void JumpTrig()
    {
        JumpTrigger.SetTrigger("Jump");
        //JumpTrigger.ResetTrigger("Jump");
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

        if (FieldGameManager.data.isGameStart)
        {
            if (!CM.isActive())
                KeyHandler();
            PlayerRotateToLookAt();
            PlayerWCheck();
            PlayerPortalCheck();
            gameObject.transform.position = new Vector3(PlayerTransform.position.x, HexCellPosition.calculateWPosition(selfCoord.preCoordinates.W), PlayerTransform.position.z);
        }
    }
    public void PlayerPortalCheck()
    {
        // Ŭ�󿡼� �÷��̾ ��Ż�� �ִ��� ��ġ �˻� �� �´ٸ� -> ������ ��Ŷ ���� -> �������� �ش� ��ǥ�� ��Ż�� �´��� Ȯ��
        // -> �´ٸ� ready���� ����Ʈ ��Ŷ ���� -> 3���� �� �غ�Ǹ� �� ��ȯ

        // ��Ż Ÿ������ Ȯ�� ������ 0,0,0     1,0,-1     0,1,-1�� �� -> �� ��ȯ �ߵǸ� Cell�� type �߰��� ���ϴ� ������� �ٲ���
        
        if(selfCoord.coordinates.X == 17 && selfCoord.coordinates.Z == -21 ||
            selfCoord.coordinates.X == 16 && selfCoord.coordinates.Z == -20 ||
            selfCoord.coordinates.X == 17 && selfCoord.coordinates.Z == -20)
        {
            if (isReady) return;
            FieldGameManager.Net.SendChangeSceneReadyPacket(1);
            isReady = true;
            return;
        }
        else if (isReady)
        {
            FieldGameManager.Net.SendChangeSceneReadyPacket(0);
        }
        isReady = false;
    }

    public void PlayerWCheck()
    {
        if (selfCoord.coordinates.W != grid.cellMaps.Get(selfCoord.coordinates).w + 1)
        {
            //Debug.LogError(">Player W Coordinate Error!<");
            //print("Self W : " + selfCoord.coordinates.W);
            //print("Self W : " + grid.cellMaps.Get(selfCoord.coordinates).w);
            //Debug.Log(grid.cellMaps.Get(selfCoord.coordinates).x.ToString() + grid.cellMaps.Get(selfCoord.coordinates).y.ToString() + grid.cellMaps.Get(selfCoord.coordinates).z.ToString());
            selfCoord.coordinates.W = grid.cellMaps.Get(selfCoord.coordinates).w + 1;
        }
        //Debug.Log("P1 : " + PlayerTransform.position.x + " " + selfCoord.coordinates.X);
        PlayerTransform.position = calculatePlayerPosition();
        //Debug.Log("P2 :" + PlayerTransform.position.x + " " + selfCoord.coordinates.X);
        //Debug.Log("z : " + gameObject.transform.position.z);

    }

      public Vector3 calculatePlayerPosition()
    {
        int beatTime = 500;

        float tick = LerpSquare((Time.time - selfCoord.preBeatedTime) * 1000f / beatTime);
        float newX;
        float newY;
        float newZ;

        newX = Mathf.Lerp(selfCoord.preCoordinates.X * 0.866f, selfCoord.coordinates.X * 0.866f, tick);
        newZ = Mathf.Lerp(selfCoord.preCoordinates.X * 0.5f + selfCoord.preCoordinates.Z, selfCoord.coordinates.X * 0.5f + selfCoord.coordinates.Z, tick);
        newY = SlimeWLerp(HexCellPosition.calculateWPosition(selfCoord.preCoordinates.W), HexCellPosition.calculateWPosition(selfCoord.coordinates.W), tick);

        if (tick >= 1f)
        {
            selfCoord.preCoordinates = selfCoord.coordinates;
        }

        return new Vector3(newX, newY, newZ);
    }

    public float LerpSquare(float tick)
    {
        if (tick < 0.3f)
            return 0f;
        else if (tick < 0.7f)
        {
            return (tick - 0.3f) * 2.5f;
        }
        else
            return 1f;
    }

    public float SlimeWLerp(float a, float b, float t)
    {
        float skyHigh = (a + b) * 0.5f + 2f;

        if (t < 0.5f)
        {
            return Mathf.Lerp(a, skyHigh, t);
        }
        else
        {
            return Mathf.Lerp(skyHigh, b, t);
        }
    }

    public void PlayerRotateToLookAt()
    {
        switch(selfDirection)
        {
            case HexDirection.LeftUp:
                //�þ� ���͸� �����ؾ��� ������ �����ϸ� �翬�� Ʋ����
                //Vector3 c = Vector3.Cross(transform.rotation.eulerAngles, new Vector3(0, -120, 0));
                //transform.Rotate(0, c.x * 3f, 0);

                transform.rotation = Quaternion.Euler(new Vector3(0, -120 - 90, 0));
                break;
            case HexDirection.Up:
                transform.rotation = Quaternion.Euler(new Vector3(0, -90 - 90, 0));
                break;
            case HexDirection.RightUp:
                transform.rotation = Quaternion.Euler(new Vector3(0, -30 - 90, 0));
                break;
            case HexDirection.Down:
                transform.rotation = Quaternion.Euler(new Vector3(0, 90 - 90, 0));
                break;
            case HexDirection.LeftDown:
                transform.rotation = Quaternion.Euler(new Vector3(0, 120 - 90, 0));
                break;
            case HexDirection.RightDown:
                transform.rotation = Quaternion.Euler(new Vector3(0, 30 - 90, 0));
                break;
        }
    }

    public void PlayerSpinDirection(int x, int y, int z)
    {
        if (selfCoord.coordinates.X - 1 == x && selfCoord.coordinates.Z + 1 == z)
        {
            selfDirection = HexDirection.LeftUp;
        }
        else if (selfCoord.coordinates.X == x && selfCoord.coordinates.Z + 1 == z)
        {
            selfDirection = HexDirection.Up;
        }
        else if (selfCoord.coordinates.X + 1 == x && selfCoord.coordinates.Z == z)
        {
            selfDirection = HexDirection.RightUp;
        }
        else if (selfCoord.coordinates.X - 1 == x && selfCoord.coordinates.Z == z)
        {
            selfDirection = HexDirection.LeftDown;
        }
        else if (selfCoord.coordinates.X == x && selfCoord.coordinates.Z - 1 == z)
        {
            selfDirection = HexDirection.Down;
        }
        else if (selfCoord.coordinates.X + 1 == x && selfCoord.coordinates.Z - 1 == z)
        {
            selfDirection = HexDirection.RightDown;
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
            case KeyCode.W:
                selfDirection = HexDirection.LeftUp;
                break;
            case KeyCode.E:
                selfDirection = HexDirection.Up;
                break;
            case KeyCode.R:
                selfDirection = HexDirection.RightUp;
                break;
            case KeyCode.S:
                selfDirection = HexDirection.LeftDown;
                break;
            case KeyCode.D:
                selfDirection = HexDirection.Down;
                break;
            case KeyCode.F:
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
        

        if (Input.GetKeyDown(KeyCode.W) && KeyCheck(KeyCode.W))
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
        else if (Input.GetKeyDown(KeyCode.E) && KeyCheck(KeyCode.E))
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
        else if (Input.GetKeyDown(KeyCode.R) && KeyCheck(KeyCode.R))
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
        else if (Input.GetKeyDown(KeyCode.S) && KeyCheck(KeyCode.S))
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
        else if (Input.GetKeyDown(KeyCode.D) && KeyCheck(KeyCode.D))
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
        else if (Input.GetKeyDown(KeyCode.F) && KeyCheck(KeyCode.F))
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
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("skill change 1");
            FieldGameManager.Net.SendChangeSkillPacket(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            FieldGameManager.Net.SendChangeSkillPacket(2);
            Debug.Log("skill change 2");

        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            FieldGameManager.Net.SendChangeSkillPacket(3);
            Debug.Log("skill change 3");

        }

    }

    public void EnterPortal()
    {
        selfDirection = HexDirection.Down;
        selfCoord.plus(0, 0, 0, grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y, selfCoord.coordinates.Z).w - selfCoord.coordinates.W + 1);
        JumpTrig();
    }

    public void ChangeSkill(int skillNum)
    {
        self_skillnum = skillNum;
    }

}
