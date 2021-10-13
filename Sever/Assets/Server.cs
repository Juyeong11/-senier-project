using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;

public class Server : MonoBehaviour
{
    // Start is called before the first frame update
    Socket c_Socket;
    void sendStr(System.IAsyncResult ar)
    {
        Socket c_s = (Socket)ar.AsyncState;
        int strLength = c_s.EndSend(ar);
    }
    void receiveStr(System.IAsyncResult ar)
    {
        Socket c_Socket = (Socket)ar.AsyncState;
        int strLength = c_Socket.EndReceive(ar);
        Debug.Log(System.Text.Encoding.Default.GetString(receiveBytes));
        is_not_receive = false;
    }
    void Start()
    {
        {


            const int SEVER_PORT = 4000;
            //bind�� Listen
            Socket ServerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            ServerSock.Bind(new IPEndPoint(IPAddress.Any, SEVER_PORT));
            ServerSock.Listen(100);
            Debug.Log("Listen��");

            c_Socket = ServerSock.Accept();
            Debug.Log("Accept");

            c_Socket.BeginReceive(receiveBytes, 0, BUFSIZE, SocketFlags.None, new System.AsyncCallback(receiveStr), c_Socket);

            c_Socket.BeginSend(receiveBytes, 0, BUFSIZE, SocketFlags.None, new System.AsyncCallback(sendStr), c_Socket);// �̰� overlapped �ݹ����ϴ� �Լ��ΰ�?

            is_not_receive = false;
        }
    }

    const int BUFSIZE = 256;
    byte[] receiveBytes = new byte[BUFSIZE];
    bool is_not_receive = true;

    // Update is called once per frame
    void Update()
    {
        
        //recive�ϰ� �װ� �ٽ� send�������
        if (Input.GetKeyDown(KeyCode.K))
        {

        }


    }
}
