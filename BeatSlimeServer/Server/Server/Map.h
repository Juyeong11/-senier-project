#pragma once


struct PatternInfo {

	int type;
	int pivotType;
	int time;
	int dir;
	int speed;
	int x, y, z;
	PatternInfo(int Type, int PivotType, int Time, int Dir, int Speed, int px, int py, int pz)
		:type(Type), pivotType(PivotType), time(Time), dir(Dir), speed(Speed), x(px), y(py), z(pz)
	{}

	bool operator <(const PatternInfo& rhs) const {
		return time < rhs.time;
	}
	void operator =(const PatternInfo& rhs) {
		type = rhs.type;
		pivotType = rhs.pivotType;
		time = rhs.time;
		dir = rhs.dir;
		speed = rhs.speed;
		x = rhs.x;
		y = rhs.y;
		z = rhs.z;
	}
	static constexpr int HexCellAround[6][3] = {
	{0, -1, 1 },{ 1, -1, 0 }, { 1, 0, -1 },
	{ 0, 1, -1 },{ -1, 1, 0 }, { -1, 0, 1 }
	};

	static constexpr int HexPattern3[10][3] = {
	{ 0, -1, 1 },{ 1, -1, 0 }, { 1, 0, -1 },
	{ 0, 1, -1 },{ -1, 1, 0 }, { -1, 0, 1 },
		{+1,+1,-2},{-1,-1,+2},{+2,-2,0},{-2,+2,0}
	};

	static constexpr int HexPattern4[8][3] = {
		{0,-1,+1},{0,-2,+2},
		{-1,0,+1},{-2,0,+2},
		{+1,0,-1},{+2,0,-2},
		{0,+1,-1},{0,+2,-2}
	};

};
class MapInfo {
public:
	int offsetX;
	int offsetZ;

	int LengthX;
	int LengthZ;

	std::vector<std::string> menu;
	std::map<std::string, std::vector<std::string>> pattern;


	std::vector<PatternInfo> pattern_time;
	//typedef int Time;
	//std::unordered_map<Time,int> parrying_pattern;

public:

	int* map;

	int timeByBar;
	int timeByBeat;
	int timeBy16Beat;
	int timeBy24Beat;
	int bpm;
	int totalSongTime;
	int nowSongTime;
	int barCounts;

	int num_totalPattern;

	void SetMap(std::string map_name, std::string music_name);
	int GetTileType(int x, int z);
	void SetTileType(int x, int z,int pre_x,int pre_z);
	const std::vector<PatternInfo>& GetPatternTime() { return pattern_time; }
	~MapInfo()
	{
		delete[] map;
	}
	/*
	int half = 19 / 2;
	for (int x = -half; x <= half; ++x)
	{
		for (int z = -half; z <= half; ++z)
		{
			std::cout << maps[FIELD_MAP].GetTileType(x, z) << std::endl;
		}
	}
	*/
};

class GameObject;

class Portal
{
public:
	int x, y, z;
	const int range = 1;
	MAP_TYPE map_type;
	std::unordered_set<int> player_ids;
	std::mutex id_lock;
	std::atomic_int ready_player_cnt;
	Portal(int _x, int _z, MAP_TYPE _map_type) :x(_x), z(_z) {
		y = -z - x; map_type = _map_type;
		ready_player_cnt = 0;
	};

	bool isPortal(int _x, int _z)
	{
		if (range < abs(_x - x)) return false;
		if (range < abs(_z - z)) return false;

		return true;
	}

	bool findPlayer(int id) {
		id_lock.lock();
		if (player_ids.contains(id)) {
			id_lock.unlock();
			return true;
		}
		id_lock.unlock();
		return false;
	}
};

class GameRoom
{
public:
	int game_room_id;
	int map_type;
	float bpm;
	std::chrono::system_clock::time_point start_time{};
	// atomic���δ� ���ÿ� MAX_IN_GAME_PLAYER���̼� ���� ���� �뿡�� ���� �����ϴ°� ���� ���� ���� ��
	std::mutex state_lock;
	bool isGaming;
	std::atomic_int pattern_progress;
	std::atomic_int Score[MAX_IN_GAME_PLAYER];
	std::atomic_int Money[MAX_IN_GAME_PLAYER];

	GameObject* player_ids[MAX_IN_GAME_PLAYER];
	GameObject* boss_id;
	Portal*		portal;// ������ ������ ��� ���� ��Ż ��ġ�� ���ƿ��� ����

	std::mutex ready_lock;
	int ready_player_cnt;
	//std::mutex id_insert_lock;

	GameRoom();
	GameRoom(int id);

	void GameRoomInit(int mapType, float BPM, GameObject* Boss, GameObject* Players[MAX_IN_GAME_PLAYER],Portal* p);
	int FindPlayer(int id) const;
	int FindPlayerID_by_GameRoom(int id) const;

	int find_online_player() const;

	int find_max_hp_player() const;
	int find_min_hp_player() const;
	int find_max_distance_player() const;
	int find_min_distance_player() const;

	int get_item_result() const;
	void set_player_portal_pos(int c_id);
	void game_end();
};