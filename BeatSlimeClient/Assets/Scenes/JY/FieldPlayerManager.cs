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
    public SkillConveter SC;

    public static int self_skillnum = 1;   //1~3
    public static int self_skillLevel = 1; //0~3
    public static int[] skillLevelsContainer = new int[3]; // only for shop

    public ShopManager SM;
    bool shopOpened = false;
    bool scrollOpened = false;

    public ScrollManager scrollManager;

    public ShopPrices shopPrices;

    Cell nowOnCellTag;
    
    public GameObject AlertPressKey;
    public GameObject AlertPanel;
    public UnityEngine.UI.Text AlertText;
    public AlertOBJ alertOBJ;

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
        nowOnCellTag = null;
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
        JumpTrigger.SetTrigger("IJump");
        OutLineTrigger.SetTrigger("IJump");
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
            PlayerOnCellCheck();
            gameObject.transform.position = new Vector3(PlayerTransform.position.x, HexCellPosition.calculateWPosition(selfCoord.preCoordinates.W), PlayerTransform.position.z);

        }
    }
    public void PlayerOnCellCheck()
    {
        // 클라에서 플레이어가 포탈에 있는지 위치 검사 후 맞다면 -> 서버에 패킷 전송 -> 서버에서 해당 좌표가 포탈이 맞는지 확인
        // -> 맞다면 ready상태 이펙트 패킷 전송 -> 3명이 다 준비되면 씬 전환

        // 포탈 타일인지 확인 지금은 0,0,0     1,0,-1     0,1,-1로 함 -> 씬 전환 잘되면 Cell에 type 추가해 비교하는 방법으로 바꾸자
        
        if (nowOnCellTag != null)
        {
            if(nowOnCellTag.state == cellState.Stage1Portal)
            {
                //Debug.Log("ready");
                if (isReady) return;
                Network.SendChangeSceneReadyPacket(1);
                isReady = true;
                return;
            }
            else if(nowOnCellTag.state == cellState.Stage2Portal)
            {
                //Debug.Log("ready");
                if (isReady) return;
                Network.SendChangeSceneReadyPacket(1);
                isReady = true;
                return;
            }
            else if(nowOnCellTag.state == cellState.Stage1_2Portal)
            {
                //Debug.Log("ready");
                if (isReady) return;
                Network.SendChangeSceneReadyPacket(1);
                isReady = true;
                return;
            }
            else if(nowOnCellTag.state == cellState.Stage2_2Portal)
            {
                //Debug.Log("ready");
                if (isReady) return;
                Network.SendChangeSceneReadyPacket(1);
                isReady = true;
                return;
            }
            else if (isReady)
            {
                Network.SendChangeSceneReadyPacket(0);
            }

            if (nowOnCellTag.state != cellState.Shop)
            {
                SM.ShopClose();
                shopOpened = false;
            }

            if (nowOnCellTag.state == cellState.Shop)
            {
                AlertPressKey.GetComponent<UnityEngine.UI.Text>().text = "스페이스 바를 눌러서 상점 열기";
                AlertPressKey.SetActive(true);
            }
            else if (nowOnCellTag.state >= cellState.Panel11 && nowOnCellTag.state <= cellState.Panel19)
            {
                AlertPressKey.GetComponent<UnityEngine.UI.Text>().text = "스페이스 바를 눌러서 표지판 내용 확인";
                AlertPressKey.SetActive(true);
            }
            else
            {
                AlertPressKey.SetActive(false);
            }


            isReady = false;
        }
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
        float skyHigh = (a + b) * 0.5f + 1.7f;

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
                //시야 벡터를 외적해야지 각도를 외적하면 당연히 틀리지
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
            if (Network.isOnline)
            {
                //Debug.Log("키 전송");
                // 서버에 이동 전송
                //FieldGameManager.data.setMoved();

                Network.SendMovePacket((byte)Protocol.DIR.LEFTUP);
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
            if (Network.isOnline)
            {
                //GameManager.data.setMoved();
                // 서버에 이동 전송
                Network.SendMovePacket((byte)Protocol.DIR.UP);
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
            if (Network.isOnline)
            {
                //GameManager.data.setMoved();
                // 서버에 이동 전송
                Network.SendMovePacket((byte)Protocol.DIR.RIGHTUP);
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
            if (Network.isOnline)
            {
                //GameManager.data.setMoved();
                // 서버에 이동 전송
                Network.SendMovePacket((byte)Protocol.DIR.LEFTDOWN);
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
            if (Network.isOnline)
            {
                //GameManager.data.setMoved();
                // 서버에 이동 전송
                Network.SendMovePacket((byte)Protocol.DIR.DOWN);
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
            if (Network.isOnline)
            {
                //GameManager.data.setMoved();
                // 서버에 이동 전송
                Network.SendMovePacket((byte)Protocol.DIR.RIGHTDOWN);
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
            if (nowOnCellTag.obejct)
            {
                switch (nowOnCellTag.state)
                {
                    case cellState.Shop:
                        if (shopOpened)
                        {
                            SM.ShopClose();
                            shopOpened = false;
                            ExtraSoundManager.instance.SFX(ESound.Popdown);
                        }
                        else
                        {
                            SM.ShopOpen();
                            shopOpened = true;
                            ExtraSoundManager.instance.SFX(ESound.Popup);
                        }
                    break;

                    case cellState.Orgel:
                    // 오르골 처리

                        if (Orgel.instance.isOrgelPlaying)
                        {
                            Network.SendUseItemPacket(99);   
                        }
                        else if (scrollOpened == false)
                        {
                            scrollManager.hasOpen();
                            scrollOpened = true;
                        }
                        else
                        {
                            scrollManager.hasClose();
                            scrollOpened = false;
                        }
                    
                    break;
                }
                if (nowOnCellTag.state >= cellState.Panel11 && nowOnCellTag.state <= cellState.Panel19)
                {
                    AlertText.text = alertOBJ.Data[nowOnCellTag.state - cellState.Panel11];
                    AlertPanel.SetActive(true);
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (nowOnCellTag.obejct)
            {
                if (AlertPanel.activeSelf)
                {
                    AlertPanel.SetActive(false);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            SC.SkillConvet();
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

    public void MoveTag()
    {
        nowOnCellTag = FieldGameManager.data.grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y, selfCoord.coordinates.Z);

        // if (nowOnCellTag.state != cellState.None)
        //     Debug.Log("nowOnCellTag : " + nowOnCellTag);
        if (nowOnCellTag.obejct)
        {
            if (nowOnCellTag.state != cellState.Orgel)
            {
                if (scrollManager.gameObject.activeSelf)
                {
                    scrollManager.hasClose();
                    scrollOpened = false;
                }
            }
        }
    }
}