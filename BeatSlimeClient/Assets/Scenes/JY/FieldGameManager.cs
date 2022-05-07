using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FieldGameManager : MonoBehaviour
{
    private static FieldGameManager instance;
    public static FieldGameManager data {get {if (instance == null){ Debug.LogError("Invalid");} return instance;} set {if (instance == null) instance = value;}}


    public int mapCellid = 0;

    public GameObject player;
    public Animator playerAnim;
    public Animator outlineAnim;
    public GameObject tilemap;
    public FieldHexGrid grid;

    public MapLoader loader;

    public SoundManager soundManager;
    public SoundEffectManager soundEffectManager;
    public ChattingManager chattingManager;
    public bool isGameStart;

    //
 
    static GameObject[] Objects = new GameObject[Protocol.CONSTANTS.MAX_OBJECT];

    public static int myPlayerID = -1;
    public ArrayList Mapdata = new ArrayList();

    int scene_num;

    public GameObject ResponseMenu;
    public MusicName MN;

    private bool isDebugCharacter = false;

    void Awake()
    {
        print("Start");
        data = this;
        isGameStart = false;

        if (grid.TMP)
        {
            loader.Match(grid);
            loader.LoadMap();
        }

        Network.SendChangeSceneDonePacket(0);
        if (myPlayerID != -1)
        {
            Objects[myPlayerID] = player;
        }
    }
    private void OnApplicationQuit()
    {
        Network.CloseSocket();
    }
    void Start()
    {
        if (false == Network.isOnline)
        {
            Network.CreateAndConnect();
           
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            soundManager.StopBGM();
        }


        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            if (isDebugCharacter)
            {
                scene_num = 2;
                StartCoroutine(ChangeScene());
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            var r = new System.Random();
            Network.SendLogIn("Happy" + r.Next(0, 128));
        }
        if (Input.GetKeyDown(KeyCode.F1))
        {
            MN.ChangeMusicName(soundManager.getSongName());
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Network.SendTeleportPacket(0); // 1�� ��Ż�� �̵�
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            if (isDebugCharacter)
            {
                Network.SendTeleportPacket(1); // 100��� ȹ��
                FieldPlayerManager.money += 100;
            }
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            //������           ����      damage    cooltime
            // 0 : AD0         0        5         10
            // 1 : AD1         50       10        8
            // 2 : AD2         150      20        7
            // 3 : AD3         500      40        6
            // 4 : Heal0       0        5         10
            // 5 : Heal1       50       5         8
            // 6 : Heal2       200      10        7
            // 7 : Heal3       800      20        6
            // 8 : Tank0       0        10        10
            // 9 : Tank1       50       20        8
            // 10: Tank2       150      40        7
            // 11: Tank3       500      80        6
            if (isDebugCharacter)
            Network.SendBuyPacket(0); // 0�� ������ ����
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            //scrool item�� ����ϰڴ�.
            //itemType�� ���ڷ� ���� 0~9���� ���� �� ��ũ�� 0~9���� ���ڷ� ��ũ���� ����� �ű� 
            if (isDebugCharacter)
            Network.SendUseItemPacket(1);
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            // ��ũ���� ��ڴ�. ��� ��ũ���� �������� �����ϰ� �ο�
            //if (isDebugCharacter)
            Network.SendTeleportPacket(3);
            PlayerPrefs.SetInt("inventory1", 1);
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (isDebugCharacter)
            PlayerPrefs.DeleteAll();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit(0);
        }

        if (Network.isOnline)
        {

            isGameStart = true;
            // ��Ʈ��ũ �޼��� ť
            if (Network.MessQueue.Count > 0)
            {
                byte[] data = Network.MessQueue.Dequeue();

                byte type = data[1];

                switch (type)
                {
                    case Protocol.CONSTANTS.SC_PACKET_LOGIN_OK:
                        {
                            Protocol.sc_packet_login_ok p = Protocol.sc_packet_login_ok.SetByteToVar(data);

                            soundManager.PlayBGM("riverside");
                            MN.ChangeMusicName(soundManager.getSongName());
                            playerAnim.SetFloat("Speed", 1);
                            outlineAnim.SetFloat("Speed", 1);
                            myPlayerID = p.id;
                            Objects[p.id] = player;

                            FieldPlayerManager.myName = System.Text.Encoding.UTF8.GetString(p.name).Split('\0')[0];
                            Debug.Log(FieldPlayerManager.myName.Length);
                            Debug.Log(FieldPlayerManager.myName);
                            if (FieldPlayerManager.myName == "ADMIN")
                            {
                                isDebugCharacter = true;
                            }
                            //PlayerPref.SetString("myName" + myPlayerID, System.Text.Encoding.UTF8.GetString(p.name));

                            //print(p.cur_skill_type);
                            FieldPlayerManager.instance.ChangeSkill(p.cur_skill_type, p.cur_skill_level);
                            FieldPlayerManager.instance.SetSkillLevelContainer(p.skill_progress[0], p.skill_progress[1], p.skill_progress[2]);

                            for (int i = 0; i < 20; ++i)
                            {
                                Debug.Log(p.inventory[i]);
                                PlayerPrefs.SetInt("inventory" + i, p.inventory[i]);
                            }
                            //PlayerPref
                            FieldPlayerManager.money = p.money;

                            //PutPlayerObject(p.type, p.id, p.x, p.y);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_LOGIN_FAIL:
                        {
                            Protocol.sc_packet_login_fail p = Protocol.sc_packet_login_fail.SetByteToVar(data);
                            if (p.reason == 0)
                            {
                                Debug.LogError("�ش� ���̵�� �̹� ��� �� �Դϴ�.");
                            }
                            else
                            {
                                Debug.LogError("�Ͽ�ư ���� �߸���");
                            }
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_CHANGE_SCENE:
                        {
                            Protocol.sc_packet_change_scene p = Protocol.sc_packet_change_scene.SetByteToVar(data);



                            scene_num = p.scene_num;
                            StartCoroutine(ChangeScene());

                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_MOVE:
                        {
                            Protocol.sc_packet_move p = Protocol.sc_packet_move.SetByteToVar(data);


                            //Debug.Log(p.id+"�̵�");
                            //Debug.Log((byte)p.dir);

                            //grid.cellMaps.Get(p.x, p.y, p.z).obejct.GetComponent<HexCellPosition>().enableToMove_ForField = false;
                            if (p.id < Protocol.CONSTANTS.MAX_USER)
                            {
                                if (p.id == myPlayerID)
                                {
                                    FieldPlayerManager.instance.PlayerSpinDirection(p.x, p.y, p.z);
                                    FieldPlayerManager.instance.JumpTrig();
                                    
                                    grid.cellMaps.Get(p.x, p.y, p.z).obejct.GetComponentInChildren<SpriteRenderer>().enabled = false;
                                }
                                else
                                {
                                    Objects[p.id].GetComponent<FieldOtherPlayerManager>().PlayerSpinDirection(p.x, p.y, p.z);
                                    Objects[p.id].GetComponent<FieldOtherPlayerManager>().JumpTrig();
                                }
                            }
                            Objects[p.id].GetComponent<FieldHexCellPosition>().SetPosition(p.x, p.y, p.z);
                            if (p.id == myPlayerID)
                            {
                                FieldPlayerManager.instance.MoveTag();
                            }
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PUT_OBJECT:
                        {
                            Protocol.sc_packet_put_object p = Protocol.sc_packet_put_object.SetByteToVar(data);

                            //PutObject(p.type, p.id, p.x, p.y);
                            switch (p.obj_type)
                            {
                                case (byte)Protocol.OBJECT_TYPE.PLAPER:
                                    {
                                        // Debug.Log(p.id + ", " + p.x + ", " + p.y + ", " + p.z + ", " + "�÷��̾� ����");
                                        Objects[p.id] = ObjectPool.instance.PlayerObjectQueue.Dequeue();
                                        Objects[p.id].SetActive(true);

                                        Objects[p.id].GetComponent<FieldHexCellPosition>().SetPosition(p.x, p.y, p.z);

                                        //print("myPlayerID : " + myPlayerID + " p.id :" + p.id + " p.skillType : " + p.skillType + " direction : " + p.direction);

                                        if (p.id == myPlayerID)
                                        {
                                            FieldPlayerManager.instance.selfDirection = (HexDirection)p.direction;
                                            FieldPlayerManager.self_skillnum = p.skillType;
                                            FieldPlayerManager.self_skillLevel = p.skillLevel;
                                        }   
                                        else
                                        {
                                            Objects[p.id].GetComponentInChildren<Animator>().SetFloat("Speed", PlayerPrefs.GetFloat("pAnimSpeed"));

                                            Objects[p.id].GetComponent<FieldOtherPlayerManager>().selfCoord.direction = (HexDirection)p.direction;
                                            Objects[p.id].GetComponent<FieldOtherPlayerManager>().other_playerName = System.Text.Encoding.UTF8.GetString(p.name).Split('\0')[0];
                                            Objects[p.id].GetComponent<FieldOtherPlayerManager>().other_skillnum = p.skillType;
                                            Objects[p.id].GetComponent<FieldOtherPlayerManager>().other_skillLevel = p.skillLevel;
                                            //Debug.Log(p.id + " �÷��̾� ����");
                                            Objects[p.id].GetComponent<FieldOtherPlayerManager>().pID = p.id;
                                        }

                                        //Objects[p.id].GetComponent<FieldOtherPlayerManager>().other_skillnum = p.skill_type;
                                        //grid.cellMaps.Get(p.x, p.y, p.z).obejct.GetComponent<HexCellPosition>().enableToMove_ForField = false;
                                        break;
                                    }
                            }

                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_REMOVE_OBJECT:
                        {
                            Protocol.sc_packet_remove_object p = Protocol.sc_packet_remove_object.SetByteToVar(data);
                            if (p.id < Protocol.CONSTANTS.MAX_USER)
                            {
                                ObjectPool.instance.PlayerObjectQueue.Enqueue(Objects[p.id]);
                                Objects[p.id].SetActive(false);
                            }
                            else
                            {
                                ObjectPool.instance.EnemyObjectQueue.Enqueue(Objects[p.id]);
                                Objects[p.id].SetActive(false);
                            }

                            //�ٸ� �÷��̾�� �ٸ��÷��̾� Ǯ��
                            //���̸� ��Ǯ�� ����
                            //ReMoveObject(p.id);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_CHANGE_SKILL:
                        {
                            Protocol.sc_packet_change_skill p = Protocol.sc_packet_change_skill.SetByteToVar(data);

                            if (p.id == myPlayerID)
                            {
                               FieldPlayerManager.instance.ChangeSkill(p.skill_type, p.skill_level);
                            }
                            else
                            {
                                Objects[p.id].GetComponent<FieldOtherPlayerManager>().ChangeSkill(p.skill_type, p.skill_level);
                                PartyManager.instance.PartyChangeClass(p.id, p.skill_type);
                            }
                            Debug.Log(p.id + "�� " + p.skill_type + "���� ��ų�� �ٲ�");

                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PARTY_REQUEST:
                        {
                            //��û�� ���� �� ����, �ź�
                            //�̹� ��Ƽ�� �ִٸ� �� ��Ŷ�� ���� ����
                            Protocol.sc_packet_party_request p = Protocol.sc_packet_party_request.SetByteToVar(data);


                            ResponseMenu.transform.position = player.transform.position;
                            ResponseMenu.GetComponent<ResponseBillboardUI>().GetOn(player.transform, p.requester_id, Objects[p.requester_id].GetComponent<FieldOtherPlayerManager>().other_playerName);

                            //����

                            //Net.SendPartyRequestAnwserPacket(1, p.requester_id);
                            //����
                            //Net.SendPartyRequestAnwserPacket(0, p.requester_id);

                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PARTY_REQUEST_ANWSER:
                        {
                            //���� ���� ��û�� ���� �ƴ��� ���� �ƴ��� �˷��ִ� ��Ŷ
                            Protocol.sc_packet_party_request_anwser p = Protocol.sc_packet_party_request_anwser.SetByteToVar(data);

                            Debug.Log(p.p_id + "�� " + p.anwser + " �̶�� ������");

                            /*
                             * 0 ��û ����
                             * 1 ����
                             * 2 ���� ������ ��Ƽ�� �ο��� �̹� ���� ã�� ��� -> ��Ƽ ��û�� �������� �������
                             * 3 �ٸ� ����� ��Ƽ���� Ż���Ѱ��
                             * 4 ��밡 ��Ƽ�� �̹� �ִ°��
                             * 5 ���� �̹� ��Ƽ�� �ִ´� ��Ƽ ��û�� �������
                             *  - CS_PACKET_PARTY_REQUEST_ANWSER��Ŷ�� ������ ���� ��  requester id�� -1�̸� ��Ƽ�� Ż���Ϸ��� �ϴ� �ɷ� �˰� �������� ó����
                             */

                            string reqName;
                            if (p.p_id == myPlayerID)
                            {
                                reqName = FieldPlayerManager.myName;
                            }
                            else
                            {
                                reqName = Objects[p.p_id].GetComponent<FieldOtherPlayerManager>().other_playerName;
                            }

                            switch (p.anwser)
                            {
                                case 0:
                                    chattingManager.SetMess("<color=red>��밡 ��Ƽ ��û�� �����߽��ϴ�!</color>");
                                    break;

                                case 1:
                                    chattingManager.SetMess("<color=red>" + reqName + "���� ���ο� ��Ƽ���� �Ǿ����ϴ�!</color>");
                                    foreach (var i in p.ids)
                                    {
                                        if (i == myPlayerID || i == -1)
                                            continue;
                                        //print(i + " of skill : " + Objects[i].GetComponent<FieldOtherPlayerManager>().other_skillnum);
                                        PartyManager.instance.SetParty(i, Objects[i].GetComponent<FieldOtherPlayerManager>().other_skillnum,  Objects[i].GetComponent<FieldOtherPlayerManager>().other_playerName);
                                    }
                                    break;

                                case 2:
                                    chattingManager.SetMess("<color=red>����� ��Ƽ�� ���� �ڸ��� �����ϴ�!</color>");
                                    break;  

                                case 3:
                                    chattingManager.SetMess("<color=red>" + reqName + "���� ��Ƽ���� Ż���ϼ̽��ϴ�.</color>");
                                    PartyManager.instance.DelParty(p.p_id);
                                    if (p.p_id == myPlayerID)
                                    {
                                        PartyManager.instance.DelParty();
                                    }
                                    break;

                                case 4:
                                    chattingManager.SetMess("<color=red>���� �̹� ��Ƽ�� �ֽ��ϴ�!</color>");
                                    break;
                                case 5:
                                    chattingManager.SetMess("<color=red>�̹� ���Ե� ��Ƽ�� �ֽ��ϴ�!</color>");

                                    break;
                            }
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_CHAT:
                        {
                            Protocol.sc_packet_chat p = Protocol.sc_packet_chat.SetByteToVar(data);

                            string mess = System.Text.Encoding.UTF8.GetString(p.mess).Split('\0')[0];
                            //Debug.Log(System.Text.Encoding.UTF8.GetString(p.mess));
                            chattingManager.SetMess(p.p_id + ": " + mess);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_BUY_RESULT:
                        {
                            Protocol.sc_packet_buy_result p = Protocol.sc_packet_buy_result.SetByteToVar(data);

                            if (p.result == 0)
                            {
                                chattingManager.SetMess("�������� ������ �������� ���߽��ϴ�!");
                            }
                            else
                            {
                                FieldPlayerManager.instance.SetSkillLevelContainer(p.itemType);
                            }
                            // p->itemType  ���� �õ��� ������
                            // p->result    0 ���� ���� 1 ���� ����
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_USE_ITEM:
                        // user�� itemType�� ����߽��ϴ�.
                        // �ʵ忡 �ִ� ��� �÷��̾�� ���۵Ǵ� ��Ŷ
                        {
                            Protocol.sc_packet_use_item p = Protocol.sc_packet_use_item.SetByteToVar(data);

                            Debug.Log(p.user + "�� " + p.itemType + "�� ����߽��ϴ�.");
                            Orgel.instance.Touch(p.itemType);
                        }
                        break;
                    default:
                        Debug.Log("�̻��� Ÿ���̳�");
                        break;
                }
            }
        }
    }

    IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(0.5f);
        FieldPlayerManager.instance.PortalPlane.transform.SetParent(null);
        FieldPlayerManager.instance.PortalPlane.transform.eulerAngles = new Vector3(0, 0, 0);
        //player.GetComponent<FieldPlayerManager>().PortalPlane.transform.localRotation = Quaternion.Euler(90,0,0);
        FieldPlayerManager.instance.PortalPlane.SetActive(true);

        yield return new WaitForSeconds(1.5f);
        FieldPlayerManager.instance.EnterPortal();
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(scene_num);
    }
}
