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

                                        Objects[p.id].GetComponentInChildren<FieldHexCellPosition>().SetPosition(p.x, p.y, p.z);
                                        Objects[p.id].GetComponentInChildren<FieldHexCellPosition>().direction = (HexDirection)p.direction;
                                        //Debug.Log(p.id + " �÷��̾� ����");
                                        Objects[p.id].GetComponent<FieldOtherPlayerManager>().pID = p.id;

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

                            if(p.id == myPlayerID)
                                Objects[p.id].GetComponentInParent<FieldPlayerManager>().ChangeSkill(p.skill_type);
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
                             *  - CS_PACKET_PARTY_REQUEST_ANWSER��Ŷ�� ������ ���� ��  requester id�� -1�̸� ��Ƽ�� Ż���Ϸ��� �ϴ� �ɷ� �˰� �������� ó����
                             */
                            switch (p.anwser)
                            {
                                case 0:
                                chattingManager.SetMess("<color=red>��밡 ��Ƽ ��û�� �����߽��ϴ�!</color>");
                                break;

                                case 1:
                                chattingManager.SetMess("<color=red>" + p.p_id + "���� ���ο� ��Ƽ���� �Ǿ����ϴ�!</color>");
                                PartyManager.instance.SetParty(p.p_id, Objects[p.p_id].GetComponent<FieldOtherPlayerManager>().other_skillnum);
                                break;

                                case 2:
                                chattingManager.SetMess("<color=red>����� ��Ƽ�� ���� �ڸ��� �����ϴ�!</color>");
                                break;

                                case 3:
                                chattingManager.SetMess("<color=red>" + p.p_id + "���� ��Ƽ���� Ż���ϼ̽��ϴ�.</color>");
                                PartyManager.instance.DelParty(p.p_id);
                                break;

                                case 4:
                                    chattingManager.SetMess("<color=red>���� �̹� ��Ƽ�� �ֽ��ϴ�!</color>");
                                    break;
                            }
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_CHAT:
                        {
                            Protocol.sc_packet_chat p = Protocol.sc_packet_chat.SetByteToVar(data);

                            string mess = System.Text.Encoding.UTF8.GetString(p.mess).Split('\0')[0];
                            //Debug.Log(System.Text.Encoding.UTF8.GetString(p.mess));
                            chattingManager.SetMess(p.p_id+ ": " + mess);
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
}
