using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


public class FieldPlayerManager : MonoBehaviour
{
    public static FieldPlayerManager instance;
    public static int money;
    public static string myName;

    public Animator JumpTrigger;
    public GameObject OutLineObject;
    public Animator OutLineTrigger;

    bool isReady = false;

    public playerState state;
    public PlayerKeyHandler handle;     //Invalid

    public FieldHexCellPosition selfCoord;
    public HexDirection selfDirection;
    public FieldHexGrid grid;

    public Transform PlayerTransform;

    public GameObject PortalPlane;

    public ChattingManager CM;

    public static int self_skillnum = 1;   //1~3
    public static int self_skillLevel = 1; //0~3
    public static int[] skillLevelsContainer = new int[3]; // only for shop

    public ShopManager SM;
    bool shopOpened = false;

    public ShopPrices shopPrices;
    

    //[System.NonSerialized]
    //public HexCoordinates Destination;

    //List<HexDirection> path = new List<HexDirection>();

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        grid = FieldGameManager.data.grid;
        state = playerState.Idle;
        //selfDirection = HexDirection.Up;
        //onPlayerFly.Invoke();

    }
    public void LoginOk()
    {
        grid = FieldGameManager.data.grid;
        state = playerState.Idle;
        //selfDirection = HexDirection.Up;
        //onPlayerFly.Invoke();
    }

    public void JumpTrig()
    {
        JumpTrigger.SetTrigger("Jump");
        OutLineTrigger.SetTrigger("Jump");
        OutLineObject.layer = LayerMask.NameToLayer("RenderOut");
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
            OutLineObject.layer = LayerMask.NameToLayer("OutLiner");
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var t = FieldGameManager.data.grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y, selfCoord.coordinates.Z);
            if (t.obejct)
            {
                switch (t.state)
                {
                    case cellState.Shop:
                        if (shopOpened)
                        {
                            SM.ShopClose();
                            shopOpened = false;
                        }
                        else
                        {
                            SM.ShopOpen();
                            shopOpened = true;
                        }
                    break;

                    case cellState.Orgel:
                    // ������ ó��
                    if (Orgel.instance.isOrgelPlaying)
                    {
                        FieldGameManager.Net.SendUseItemPacket(99);
                    }
                    else
                    {
                        FieldGameManager.Net.SendUseItemPacket(1);
                    }
                    
                    break;
                }
            }
        }

    }

    public void EnterPortal()
    {
        selfDirection = HexDirection.Down;
        selfCoord.plus(0, 0, 0, grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y, selfCoord.coordinates.Z).w - selfCoord.coordinates.W + 1);
        JumpTrig();
    }

    public void ChangeSkill(int skillNum, int skillLevel)
    {
        self_skillnum = skillNum;
        self_skillLevel = skillLevel;

        if (skillNum == 0)
            self_skillnum = 1;
        
        //PlayerPref.SetInt("mySkill"+ FieldGameManager.myPlayerID, self_skillnum);
        //PlayerPref.SetInt("mySkillLevel"+ FieldGameManager.myPlayerID, self_skillLevel);
    }

    public void SetSkillLevelContainer(int a, int b, int c)
    {
        skillLevelsContainer[0] = a;
        skillLevelsContainer[1] = b;
        skillLevelsContainer[2] = c;
    }

    public void SetSkillLevelContainer(int itemType)
    {
        int index = itemType / 4;
        int level = itemType % 4;
        skillLevelsContainer[index] = level;

        switch (index)
        {
            case 0:
                money -= shopPrices.Skill1Prices[level];
            break;
            case 1:
                money -= shopPrices.Skill2Prices[level];
            break;
            case 2:
                money -= shopPrices.Skill3Prices[level];
            break;
        }
    }

}
