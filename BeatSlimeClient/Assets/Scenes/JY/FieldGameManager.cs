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

    public bool isGameStart;

    //
    public static Network Net = new Network();
    static GameObject[] Objects = new GameObject[Protocol.CONSTANTS.MAX_OBJECT];

    static int myPlayerID = -1;
    public ArrayList Mapdata = new ArrayList();

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
        else if (Input.GetKeyDown(KeyCode.O))
        {
            //id�� ���ؼ� SendPartyRequestPacket�� ���ڷ� ����
            //��Ƽ�� �ϰ� ���� �÷��̾ ���ؼ� id�� ���ڷ� �ϰ� �Ʒ� �Լ��� ȣ���ϸ��(Ŭ��� ������ ���� id�� ���� ����)
            Net.SendPartyRequestPacket(0);


            //��Ƽ ��û�� ���� ���� ���θ� �˷��� (0 ����, 1 ����)
            // �� ��°���ڴ� myPlayerID�� �־��ָ�� 
            Net.SendPartyRequestAnwserPacket(0, myPlayerID);

            //Protocol.CONSTANTS.SC_PACKET_PARTY_REQUEST: -> ��Ƽ ��û�� �Դ�.
            //Protocol.CONSTANTS.SC_PACKET_PARTY_REQUEST_ANWSER: -> ��Ƽ ��û�� ���� ���� ���ΰ� �Դ�.
        }
        // ������ ��û ���� ���ΰ� SC��Ŷ���� ��

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
                            player.GetComponentInChildren<Animator>().SetFloat("Speed", PlayerPrefs.GetFloat("pAnimSpeed"));
                            myPlayerID = p.id;
                            Objects[p.id] = player;
                            //PutPlayerObject(p.type, p.id, p.x, p.y);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_CHANGE_SCENE:
                        {
                            Protocol.sc_packet_change_scene p = Protocol.sc_packet_change_scene.SetByteToVar(data);
                            SceneManager.LoadScene(p.scene_num);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_MOVE:
                        {
                            Protocol.sc_packet_move p = Protocol.sc_packet_move.SetByteToVar(data);

                            //Debug.Log(p.id+"�̵�");
                            //Debug.Log((byte)p.dir);
                            Objects[p.id].GetComponent<FieldHexCellPosition>().setDirection((byte)p.dir);
                            Objects[p.id].GetComponent<FieldHexCellPosition>().SetPosition(p.x, p.y, p.z);
                            if (p.id < Protocol.CONSTANTS.MAX_USER)
                                Objects[p.id].GetComponent<FieldPlayerManager>().JumpTrig();
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
                            Debug.Log(p.id + "�� " + p.skill_type + "���� ��ų�� �ٲ�");
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PARTY_REQUEST:
                        {
                            //��û�� ���� �� ����, �ź�
                            //�̹� ��Ƽ�� �ִٸ� �� ��Ŷ�� ���� ����

                            //����
                            Net.SendPartyRequestAnwserPacket(1, myPlayerID);
                            //����
                            Net.SendPartyRequestAnwserPacket(0, myPlayerID);

                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PARTY_REQUEST_ANWSER:
                        {
                            //���� ���� ��û�� ���� �ƴ��� ���� �ƴ��� �˷��ִ� ��Ŷ
                            Protocol.sc_packet_party_request_anwser p = Protocol.sc_packet_party_request_anwser.SetByteToVar(data);

                            Debug.Log(p.p_id + "�� " + p.anwser + " �̶�� ������");

                        }
                        break;
                    default:
                        Debug.Log("�̻��� Ÿ���̳�");
                        break;
                }
            }
        }
    }
}
