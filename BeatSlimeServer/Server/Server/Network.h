#pragma once



#include"protocol.h"
#include"Client.h"


enum EVENT_TYPE { EVENT_BOSS_MOVE, EVENT_BOSS_TARGETING_ATTACK, EVENT_BOSS_TILE_ATTACK, EVENT_PLAYER_PARRYING};
struct timer_event {
	int obj_id;
	int target_id;
	std::chrono::system_clock::time_point start_time;
	EVENT_TYPE ev;
	//int target_id;

	constexpr bool operator <(const timer_event& left)const
	{
		return (start_time > left.start_time);
	}
};


class DataBase;
class GameRoom;
class MapInfo;
class Portal;
class Network
{
private:
	static Network* instance;
public:
	static Network* GetInstance();
	HANDLE g_h_iocp;
	SOCKET g_s_socket;
	DataBase* DB;
	Network();
	~Network();


	void start_accept() {
		SOCKET c_socket = WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, 0, 0, WSA_FLAG_OVERLAPPED);

		*(reinterpret_cast<SOCKET*>(accept_ex._net_buf)) = c_socket;

		ZeroMemory(&accept_ex._wsa_over, sizeof(accept_ex._wsa_over));
		accept_ex._comp_op = OP_ACCEPT;

		AcceptEx(g_s_socket, c_socket, accept_ex._net_buf + sizeof(SOCKET), 0, sizeof(SOCKADDR_IN) + 16,
			sizeof(SOCKADDR_IN) + 16, NULL, &accept_ex._wsa_over);
	}

	void send_login_ok(int client_id);
	void send_move_object(int client_id, int mover_id);
	void send_attack_player(int client_id, int target_id, int receiver);
	void send_put_object(int client_id, int target_id);
	void send_remove_object(int client_id, int victim_id);
	void send_map_data(int client_id, char* data, int nShell);
	void send_change_scene(int client_id, int map_type);
	void send_game_start(int client_id, int ids[3]);
	void disconnect_client(int client_id);

	bool is_near(int a, int b)
	{
		if (VIEW_RANGE < abs(clients[a]->x - clients[b]->x)) return false;
		if (VIEW_RANGE < abs(clients[a]->z - clients[b]->z)) return false;

		return true;
	}
	bool can_attack(int a, int b)
	{
		if (ATTACK_RANGE < abs(clients[a]->x - clients[b]->x)) return false;
		if (ATTACK_RANGE < abs(clients[a]->z - clients[b]->z)) return false;

		return true;
	}
	bool is_npc(int id)
	{
		return (id >= NPC_ID_START) && (id <= NPC_ID_END);
	}
	bool is_player(int id)
	{
		return (id >= 0) && (id < MAX_USER);
	}
	int get_new_id();
	int get_npc_id(int monsterType);

	int get_game_room_id();
	void Initialize_NPC() {
		for (int i = NPC_ID_START; i < NPC_ID_END; ++i) {
			sprintf_s(clients[i]->name, "NPC%d", i);
			clients[i]->x = 0;
			clients[i]->z = 0;
			clients[i]->id = i;
			clients[i]->state = ST_ACCEPT;
			
		}
	}
	void do_npc_move(int npc_id);
	void do_npc_attack(int npc_id, int target_id, int receiver);
	void do_npc_tile_attack();

	void do_timer() {
		using namespace std;
		using namespace chrono;
		while (true) {

			timer_event ev;
			while (!timer_queue.empty()) {

				timer_queue.try_pop(ev);

				if (ev.start_time <= system_clock::now()) {
					//�̺�Ʈ ����
					EXP_OVER* ex_over;// = new EXP_OVER;
					//ex_over->_comp_op = OP_NPC_MOVE;
					while (!exp_over_pool.try_pop(ex_over));
					switch (ev.ev)
					{

					case EVENT_BOSS_MOVE:
						ex_over->_comp_op = OP_BOSS_MOVE;
						break;
					case EVENT_BOSS_TARGETING_ATTACK:
						ex_over->_comp_op = OP_BOSS_TARGETING_ATTACK;
						*reinterpret_cast<int*>(ex_over->_net_buf) = ev.target_id;
						break;
					case EVENT_BOSS_TILE_ATTACK:
						ex_over->_comp_op = OP_BOSS_TILE_ATTACK;
						*reinterpret_cast<int*>(ex_over->_net_buf) = ev.target_id;
						break;
					case EVENT_PLAYER_PARRYING:
						ex_over->_comp_op = OP_PLAYER_PARRYING;
						*reinterpret_cast<int*>(ex_over->_net_buf) = ev.target_id;
						break;
					default:
						break;
					}

					PostQueuedCompletionStatus(g_h_iocp, 1, ev.obj_id, &ex_over->_wsa_over);// �ι�° ���ڰ� 0�� �Ǹ� ��Ĺ ����� ����� �ȴ�. 1��������
				}
				else {
					//�ⲯ ���µ� �ٽ� �ִ°� �� ��ȿ�� ���̴�.
					//�ٽ� ���� �ʴ� ������� ����ȭ �ʿ�
					timer_queue.push(ev);
					break;
				}
			}

			//ť�� ����ų�
			this_thread::sleep_for(10ms);
		}
	}
	void process_packet(int client_id, unsigned char* p);

	void worker();
private:
	concurrency::concurrent_priority_queue<timer_event> timer_queue;
	concurrency::concurrent_queue<EXP_OVER*> exp_over_pool;
	std::array<Gameobject*, MAX_OBJECT> clients;// 200, 200 ���� ������ ������ �� ����Ʈ ������ ��
	EXP_OVER accept_ex;

private:
	std::array<GameRoom*, MAX_GAME_ROOM_NUM> game_room;
	std::array<MapInfo*, MAP_NUM> maps;
	std::array<Portal*, PORTAL_NUM> portals;
};

