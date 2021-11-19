#pragma once

#include <WS2tcpip.h>
#include<MSWSock.h>

#include"protocol.h"
#include"Client.h"
#include"Enemy.h"
#pragma comment (lib, "WS2_32.LIB")
#pragma comment (lib, "MSWSock.LIB")

void error_display(int err_no);


class Network
{
private:
	static Network* instance;
public:
	static Network* GetInstance();
	HANDLE g_h_iocp;
	SOCKET g_s_socket;
	Network() {
		//�ν��Ͻ��� �� ����!!
		assert(instance == nullptr);
		instance = this;
		for (int i = 0; i < MAX_USER; ++i) {
			clients[i].id = i;
		}
		Enemys.reserve(4);
		for (int i = 0; i < 4; ++i)
		{
			Enemys.emplace_back(i);
		}

	}
	~Network() {
		//�����尡 ����� �� �̱� ������ ���� �� �ʿ䰡 ����
//accpet������ �� ������ �����
		for (auto& cl : clients) {
			if (ST_INGAME == cl.state)
				disconnect_client(cl.id);
		}
	}

	void start_accept() {
		SOCKET c_socket = WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, 0, 0, WSA_FLAG_OVERLAPPED);

		//char* ���۸� socket*�� �ٲ� ������ ����ų �� �ֵ��� �Ѵ�. ���ϵ� �������ε�?
		*(reinterpret_cast<SOCKET*>(accept_ex._net_buf)) = c_socket;

		ZeroMemory(&accept_ex._wsa_over, sizeof(accept_ex._wsa_over));
		accept_ex._comp_op = OP_ACCEPT;

		AcceptEx(g_s_socket, c_socket, accept_ex._net_buf + sizeof(SOCKET), 0, sizeof(SOCKADDR_IN) + 16,
			sizeof(SOCKADDR_IN) + 16, NULL, &accept_ex._wsa_over);
	}

	void send_login_ok(int client_id);
	void send_game_start(int client_id);
	void send_move_object(int client_id, int mover_id);
	void send_put_object(int client_id, int target_id, int obj_type);
	void send_remove_object(int client_id, int victim_id);
	void disconnect_client(int client_id);

	int get_new_id();

	void process_packet(int client_id, unsigned char* p);

	void worker();

	std::array<Client, MAX_USER> clients;//��..
	EXP_OVER accept_ex;
	std::vector <Enemy> Enemys;
	std::atomic<int> nPlayer = 0;
	bool isGameStart = false;
};

