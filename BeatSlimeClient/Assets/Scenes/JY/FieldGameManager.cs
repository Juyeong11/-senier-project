using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FieldGameManager : MonoBehaviour
{
    public static FieldGameManager data;

    public int mapCellid = 0;

    public GameObject player;
    public GameObject tilemap;
    public FieldHexGrid grid;

    public MapLoader loader;

    public SoundManager soundManager;
    public SoundEffectManager soundEffectManager;
    public ChattingManager chattingManager;
    public bool isGameStart;

    //
    public static Network Net = new Network();
    static GameObject[] Objects = new GameObject[Protocol.CONSTANTS.MAX_OBJECT];

    public static int myPlayerID = -1;
    public ArrayList Mapdata = new ArrayList();

    int scene_num;

    public GameObject ResponseMenu;
    public MusicName MN;

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

        Net.SendChangeSceneDonePacket(0);
        if (myPlayerID != -1)
        {
            Objects[myPlayerID] = player;
        }
    }
    private void OnApplicationQuit()
    {
        Net.CloseSocket();
    }
    void Start()
    {
        if (false == Net.isOnline)
            Net.CreateAndConnect();
        PlayerPrefs.DeleteKey("myName");
        PlayerPrefs.DeleteKey("mySkill");
        PlayerPrefs.DeleteKey("mySkillLevel");
        PlayerPrefs.SetInt("mySkill", 1);
        PlayerPrefs.SetString("myName", "soHappy");

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            soundManager.StopBGM();
        }


        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            scene_num = 2;
            StartCoroutine(ChangeScene());
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            MN.ChangeMusicName("flower load - zeroste.");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Net.SendTeleportPacket(0); // 1�� ��Ż�� �̵�
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            Net.SendTeleportPacket(1); // 100��� ȹ��
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            //������ ���    ����
            //0 : ADLev1    	50
            //1 : ADLev2    	150
            //2 : ADLev3    	500
            //3 : TankLev1  	50
            //4 : TankLev2  	150
            //5 : TankLev3  	500
            //6 : SupLev1   	50
            //7 : SupLev2   	200
            //8 : SupLev3   	800

            Net.SendBuyPacket(0); // 0�� ������ ����
        }
        if (Net.isOnline)
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
                            MN.ChangeMusicName("riverside - zeroste.");
                            player.GetComponentInChildren<Animator>().SetFloat("Speed", PlayerPrefs.GetFloat("pAnimSpeed"));
                            myPlayerID = p.id;
                            Objects[p.id] = player;
                            //PutPlayerObject(p.type, p.id, p.x, p.y);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_LOGIN_FAIL:
                        {
                            Protocol.sc_packet_login_fail p = Protocol.sc_packet_login_fail.SetByteToVar(data);
                            if (p.reason == 0)
                            {
                                Debug.Log("�ش� ���̵�� �̹� ��� �� �Դϴ�.");
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
                            grid.cellMaps.Get(p.x, p.y, p.z).obejct.GetComponentInChildren<SpriteRenderer>().enabled = false;
                            if (p.id < Protocol.CONSTANTS.MAX_USER)
                            {
                                if (p.id == myPlayerID)
                                {
                                    Objects[p.id].GetComponentInParent<FieldPlayerManager>().PlayerSpinDirection(p.x, p.y, p.z);
                                    Objects[p.id].GetComponentInParent<FieldPlayerManager>().JumpTrig();
                                }
                                else
                                {
                                    Objects[p.id].GetComponent<FieldOtherPlayerManager>().PlayerSpinDirection(p.x, p.y, p.z);
                                    Objects[p.id].GetComponent<FieldOtherPlayerManager>().JumpTrig();
                                }
                            }
                            Objects[p.id].GetComponent<FieldHexCellPosition>().SetPosition(p.x, p.y, p.z);
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
                                        Objects[p.id].GetComponentInChildren<Animator>().SetFloat("Speed", 120 / 45.0f);

                                        Objects[p.id].GetComponent<FieldHexCellPosition>().SetPosition(p.x, p.y, p.z);

                                        //print("myPlayerID : " + myPlayerID + " p.id :" + p.id + " p.skillType : " + p.skillType + " direction : " + p.direction);

                                        if (p.id == myPlayerID)
                                        {
                                            Objects[p.id].GetComponentInParent<FieldPlayerManager>().selfDirection = (HexDirection)p.direction;
                                            Objects[p.id].GetComponentInParent<FieldPlayerManager>().self_skillnum = p.skillType;
                                            Objects[p.id].GetComponentInParent<FieldPlayerManager>().self_skillLevel = p.skillLevel;
                                        }
                                        else
                                        {
                                            Objects[p.id].GetComponent<FieldOtherPlayerManager>().selfCoord.direction = (HexDirection)p.direction;
                                            Objects[p.id].GetComponent<FieldOtherPlayerManager>().other_playerName = System.Text.Encoding.UTF8.GetString(p.name);
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
                                Objects[p.id].GetComponentInParent<FieldPlayerManager>().ChangeSkill(p.skill_type);
                                PlayerPrefs.SetInt("mySkill", player.GetComponentInParent<FieldPlayerManager>().self_skillnum);
                                PlayerPrefs.SetInt("mySkillLevel", player.GetComponentInParent<FieldPlayerManager>().self_skillLevel);
                            }
                            else
                            {
                                Objects[p.id].GetComponent<FieldOtherPlayerManager>().ChangeSkill(p.skill_type);
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
                            ResponseMenu.GetComponent<ResponseBillboardUI>().GetOn(player.transform, p.requester_id);

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
                            switch (p.anwser)
                            {
                                case 0:
                                    chattingManager.SetMess("<color=red>��밡 ��Ƽ ��û�� �����߽��ϴ�!</color>");
                                    break;

                                case 1:
                                    chattingManager.SetMess("<color=red>" + p.p_id + "���� ���ο� ��Ƽ���� �Ǿ����ϴ�!</color>");
                                    foreach (var i in p.ids)
                                    {
                                        if (i == myPlayerID || i == -1)
                                            continue;
                                        print(i + " of skill : " + Objects[i].GetComponent<FieldOtherPlayerManager>().other_skillnum);
                                        PartyManager.instance.SetParty(i, Objects[i].GetComponent<FieldOtherPlayerManager>().other_skillnum);
                                    }
                                    break;

                                case 2:
                                    chattingManager.SetMess("<color=red>����� ��Ƽ�� ���� �ڸ��� �����ϴ�!</color>");
                                    break;

                                case 3:
                                    chattingManager.SetMess("<color=red>" + p.p_id + "���� ��Ƽ���� Ż���ϼ̽��ϴ�.</color>");
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
                            // p->itemType  ���� �õ��� ������
                            // p->result    0 ���� ���� 1 ���� ����
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
        player.GetComponentInParent<FieldPlayerManager>().PortalPlane.transform.SetParent(null);
        //player.GetComponent<FieldPlayerManager>().PortalPlane.transform.localRotation = Quaternion.Euler(90,0,0);
        player.GetComponentInParent<FieldPlayerManager>().PortalPlane.SetActive(true);




        yield return new WaitForSeconds(2.0f);
        player.GetComponentInParent<FieldPlayerManager>().EnterPortal();
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(scene_num);
    }

    public void ShopOpen()
    {
        Debug.Log("SHOP!");
    }
}
