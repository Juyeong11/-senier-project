#include"stdfx.h"
#include"Client.h"
#include "DataBase.h"



void HandleDiagnosticRecord(SQLHANDLE hHandle, SQLSMALLINT hType, RETCODE RetCode)
{

	SQLSMALLINT iRec = 0;
	SQLINTEGER iError;
	WCHAR wszMessage[1000];
	WCHAR wszState[SQL_SQLSTATE_SIZE + 1];
	if (RetCode == SQL_INVALID_HANDLE) {
		fwprintf(stderr, L"Invalid handle!\n");
		return;
	}
	while (SQLGetDiagRec(hType, hHandle, ++iRec, wszState, &iError, wszMessage,
		(SQLSMALLINT)(sizeof(wszMessage) / sizeof(WCHAR)), (SQLSMALLINT*)NULL) == SQL_SUCCESS) {
		// Hide data truncated..
		if (wcsncmp(wszState, L"01004", 5)) {
			fwprintf(stderr, L"[%5.5s] %s (%d)\n", wszState, wszMessage, iError);
		}
	}
}

DataBase::DataBase() {
	SQLRETURN ret = SQLAllocHandle(SQL_HANDLE_ENV, SQL_NULL_HANDLE, &henv);
	isConnect = false;
	if (false == (ret == SQL_SUCCESS || ret == SQL_SUCCESS_WITH_INFO)) {
		std::cout << "SQLHandle ���� ����\n";
		return;
	}

	ret = SQLSetEnvAttr(henv, SQL_ATTR_ODBC_VERSION, (SQLPOINTER*)SQL_OV_ODBC3, 0);
	if (false == (ret == SQL_SUCCESS || ret == SQL_SUCCESS_WITH_INFO)) {
		std::cout << "ODBC���� ���� ����\n";
		return;
	}
	ret = SQLAllocHandle(SQL_HANDLE_DBC, henv, &hdbc);

	SQLSetConnectAttr(hdbc, SQL_LOGIN_TIMEOUT, (SQLPOINTER)5, 0);
	ret = SQLConnect(hdbc, (SQLWCHAR*)L"BeatSlime", SQL_NTS, (SQLWCHAR*)NULL, 0, NULL, 0);
	if (false == (ret == SQL_SUCCESS || ret == SQL_SUCCESS_WITH_INFO)) {
		std::cout << "ODBC ���� ����\n";

		return;
	}
	isConnect = true;
}

DataBase::~DataBase() {
	SQLDisconnect(hdbc);
	SQLFreeHandle(SQL_HANDLE_DBC, hdbc);
	SQLFreeHandle(SQL_HANDLE_ENV, henv);
}

bool DataBase::checkPlayer(PlayerData& data)
{
	if (false == isConnect) return false;

	SQLHSTMT hstmt = 0;
	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);


	std::wstring command = std::format(L"EXEC playerLogin '{}'", data.name);
	retcode = SQLExecDirect(hstmt, (SQLWCHAR*)command.c_str(), SQL_NTS);
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {

		//	// Bind columns 1, 2, and 3  
		//	// �̸� �о�� ������ bind���ش�.
		retcode = SQLBindCol(hstmt, 1, SQL_C_WCHAR, &PlayerDataSchema.p_name, MAX_NAME_SIZE, &PlayerDataSchema.cb_name);
		retcode = SQLBindCol(hstmt, 2, SQL_C_LONG, &PlayerDataSchema.p_x, 4, &PlayerDataSchema.cb_x);
		retcode = SQLBindCol(hstmt, 3, SQL_C_LONG, &PlayerDataSchema.p_z, 4, &PlayerDataSchema.cb_z);
		retcode = SQLBindCol(hstmt, 4, SQL_C_LONG, &PlayerDataSchema.p_Money, 4, &PlayerDataSchema.cb_Money);
		retcode = SQLBindCol(hstmt, 5, SQL_C_LONG, &PlayerDataSchema.p_UsingSkill, 4, &PlayerDataSchema.cb_UsingSkill);
		retcode = SQLBindCol(hstmt, 6, SQL_C_TINYINT, &PlayerDataSchema.p_isUsing, 1, &PlayerDataSchema.cb_isUsing);

		retcode = SQLBindCol(hstmt, 7, SQL_C_WCHAR, &PlayerDataSchema.p_name, MAX_NAME_SIZE, &PlayerDataSchema.cb_name);
		retcode = SQLBindCol(hstmt, 8, SQL_C_TINYINT, &PlayerDataSchema.p_SkillAD, 1, &PlayerDataSchema.cb_SkillAD);
		retcode = SQLBindCol(hstmt, 9, SQL_C_TINYINT, &PlayerDataSchema.p_SkillTa, 1, &PlayerDataSchema.cb_SKillTa);
		retcode = SQLBindCol(hstmt, 10, SQL_C_TINYINT, &PlayerDataSchema.p_SkillHeal, 1, &PlayerDataSchema.cb_SkillHeal);

		//retcode = SQLBindCol(hstmt, 10, SQL_C_WCHAR, &PlayerDataSchema.p_name, MAX_NAME_SIZE, &PlayerDataSchema.cb_name);
		//retcode = SQLBindCol(hstmt, 11, SQL_C_LONG, &PlayerDataSchema.p_MapName, 4, &PlayerDataSchema.cb_MapName);
		//retcode = SQLBindCol(hstmt, 12, SQL_C_LONG, &PlayerDataSchema.p_ClearScore, 4, &PlayerDataSchema.cb_ClearScore);

		retcode = SQLBindCol(hstmt, 11, SQL_C_WCHAR, &PlayerDataSchema.p_name, MAX_NAME_SIZE, &PlayerDataSchema.cb_name);
		retcode = SQLBindCol(hstmt, 12, SQL_C_LONG, &PlayerDataSchema.p_MMR, 4, &PlayerDataSchema.cb_MMR);

		//retcode = SQLBindCol(hstmt, 13, SQL_C_WCHAR, &PlayerDataSchema.p_name, MAX_NAME_SIZE, &PlayerDataSchema.cb_name);
		//retcode = SQLBindCol(hstmt, 14, SQL_C_LONG, &PlayerDataSchema.p_MusicScroll, 4, &PlayerDataSchema.p_MusicScroll);
		//retcode = SQLBindCol(hstmt, 15, SQL_C_LONG, &PlayerDataSchema.p_MusicScrollCount, 4, &PlayerDataSchema.p_MusicScrollCount);

		// Fetch and print each row of data. On an error, display a message and exit.  
		for (int i = 0; ; i++) {
			retcode = SQLFetch(hstmt);
			if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO)
			{
				data.name = PlayerDataSchema.p_name;

				data.x = PlayerDataSchema.p_x;
				data.z = PlayerDataSchema.p_z;
				data.curSkill = PlayerDataSchema.p_UsingSkill;

				data.SkillAD = PlayerDataSchema.p_SkillAD;
				data.SkillTa = PlayerDataSchema.p_SkillTa;
				data.SkillHeal = PlayerDataSchema.p_SkillHeal;

				data.money = PlayerDataSchema.p_Money;

				int is_using = PlayerDataSchema.p_isUsing;
				SQLCancel(hstmt);
				SQLFreeHandle(SQL_HANDLE_STMT, hstmt);

				if (is_using)
					return false;
				return true;

			}
			else {
				break;
			}
		}
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);

		//data.name;
		data.money = 0;
		data.x = 13;
		data.z = -25;
		insertPlayer(data);
		return true;

	}
	else {

		HandleDiagnosticRecord(hstmt, SQL_HANDLE_STMT, retcode);
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);

		return false;
	}



}

void DataBase::insertPlayer(PlayerData& name)
{
	if (false == isConnect) return;

	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);


	std::wstring command = std::format(L"EXEC insertPlayer '{}'", name.name);

	retcode = SQLExecDirect(hstmt, (SQLWCHAR*)command.c_str(), command.length());
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
	}
	else
		HandleDiagnosticRecord(hstmt, SQL_HANDLE_STMT, retcode);

	// Process data  
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);
	}


}

void DataBase::updatePlayer(const Client* const pl, bool isend)
{
	if (false == isConnect) return;

	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);

	// ���⼭���� �����Ұ�

	std::wstring name = std::wstring(pl->name, &pl->name[strlen(pl->name)]);
	if (name == L"Happy") {
		return;
	}
	int x = pl->x;
	int z = pl->z;
	int usingSkill = pl->curSkill;
	int money = pl->money;
	int isEnd = isend;
	
	int unlockSkillAD = pl->SkillAD;
	int unlockSkillTa = pl->SkillTa;
	int unlockSkillHe = pl->SkillHeal;

	int mmr = pl->MMR;

	std::wstring command = std::format(L"EXEC updatePlayerData '{}',{},{},{},{}, {},{},{}, {}",
		name, x, z, usingSkill, money, isEnd,
		unlockSkillAD, unlockSkillTa, unlockSkillHe,
		mmr
	);


	retcode = SQLExecDirect(hstmt, (SQLWCHAR*)command.c_str(), command.length());
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
	}
	else
		HandleDiagnosticRecord(hstmt, SQL_HANDLE_STMT, retcode);

	// Process data  
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);
	}


}

void DataBase::updateClearInfo(const Client* const pl)
{
	if (false == isConnect) return;

	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);


	std::wstring name = std::wstring(pl->name, &pl->name[strlen(pl->name)]);

	int index = 0;
	for (auto i : pl->ClearMap) {
		std::wstring command = std::format(L"EXEC updateClearScore '{}',{},{}",
			name, index, i
		);


		retcode = SQLExecDirect(hstmt, (SQLWCHAR*)command.c_str(), command.length());
		if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
		}
		else {
			HandleDiagnosticRecord(hstmt, SQL_HANDLE_STMT, retcode);
			break;
		}
	
		index++;
	}
	// Process data  
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);
	}
}

void DataBase::updateInventory(const Client* const pl)
{
	if (false == isConnect) return;

	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);


	std::wstring name = std::wstring(pl->name, &pl->name[strlen(pl->name)]);

	for (const Item& it : pl->inventory->items) {
		int musicScroll = it.itemType;
		int musicScrollCount = it.cnt;
		if (musicScroll == -1)continue;
		std::wstring command = std::format(L"EXEC updateInventory '{}',{},{}",
			name, musicScroll, musicScrollCount
		);


		retcode = SQLExecDirect(hstmt, (SQLWCHAR*)command.c_str(), command.length());
		if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
		}
		else {
			HandleDiagnosticRecord(hstmt, SQL_HANDLE_STMT, retcode);
			break;
		}

		
	}
	// Process data  
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);
	}
	
}

void DataBase::readSkills(std::array<Skill*, SKILL_CNT>& items)
{
	if (false == isConnect) {
		items[0]->CoolTime = 10;
		items[1]->CoolTime = 8;
		items[2]->CoolTime = 7;
		items[3]->CoolTime = 6;
		items[0]->SkillLevel = 0;
		items[1]->SkillLevel = 1;
		items[2]->SkillLevel = 2;
		items[3]->SkillLevel = 3;
		items[0]->SkillPrice = 0;
		items[1]->SkillPrice = 50;
		items[2]->SkillPrice = 150;
		items[3]->SkillPrice = 500;
		items[0]->Damage = 5;
		items[1]->Damage = 10;
		items[2]->Damage = 20;
		items[3]->Damage = 40;

		items[4]->CoolTime = 10;
		items[5]->CoolTime = 8;
		items[6]->CoolTime = 7;
		items[7]->CoolTime = 6;
		items[4]->SkillLevel = 0;
		items[5]->SkillLevel = 1;
		items[6]->SkillLevel = 2;
		items[7]->SkillLevel = 3;
		items[4]->SkillPrice = 0;
		items[5]->SkillPrice = 50;
		items[6]->SkillPrice = 150;
		items[7]->SkillPrice = 500;
		items[4]->Damage = 10;
		items[5]->Damage = 20;
		items[6]->Damage = 40;
		items[7]->Damage = 80;

		items[8]->CoolTime = 10;
		items[9]->CoolTime = 8;
		items[10]->CoolTime = 7;
		items[11]->CoolTime = 6;
		items[8]->SkillLevel = 0;
		items[9]->SkillLevel = 1;
		items[10]->SkillLevel = 2;
		items[11]->SkillLevel = 3;
		items[8]->SkillPrice = 0;
		items[9]->SkillPrice = 50;
		items[10]->SkillPrice = 200;
		items[11]->SkillPrice = 800;
		items[8]->Damage = 5;
		items[9]->Damage = 5;
		items[10]->Damage = 10;
		items[11]->Damage = 20;

		return;
	}
	SQLHSTMT hstmt = 0;
	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);


	std::wstring command = std::format(L"EXEC readSkill");
	retcode = SQLExecDirect(hstmt, (SQLWCHAR*)command.c_str(), SQL_NTS);
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {

		//	// Bind columns 1, 2, and 3  
		//	// �̸� �о�� ������ bind���ش�.
		retcode = SQLBindCol(hstmt, 1, SQL_C_WCHAR, &ItemDataSchema.p_name, MAX_NAME_SIZE, &ItemDataSchema.cb_name);
		retcode = SQLBindCol(hstmt, 2, SQL_C_LONG, &ItemDataSchema.p_price, 4, &ItemDataSchema.cb_price);
		retcode = SQLBindCol(hstmt, 3, SQL_C_LONG, &ItemDataSchema.p_damage, 4, &ItemDataSchema.cb_damage);
		retcode = SQLBindCol(hstmt, 4, SQL_C_LONG, &ItemDataSchema.p_CoolTime, 4, &ItemDataSchema.cb_coolTime);

		// Fetch and print each row of data. On an error, display a message and exit.  
		for (int i = 0; items.size(); i++) {
			retcode = SQLFetch(hstmt);
			if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO)
			{
				std::wstring itemType = ItemDataSchema.p_name;
				while (itemType.back() == ' ') itemType.pop_back();
				int index;
				if (itemType == L"AD0") {
					index = 0;
				}
				else if (itemType == L"AD1") {
					index = 1;
				}
				else if (itemType == L"AD2") {
					index = 2;
				}
				else if (itemType == L"AD3") {
					index = 3;
				}
				else if (itemType == L"Tank0") {
					index = 4;
				}
				else if (itemType == L"Tank1") {
					index = 5;
				}
				else if (itemType == L"Tank2") {
					index = 6;
				}
				else if (itemType == L"Tank3") {
					index = 7;
				}
				else if (itemType == L"Heal0") {
					index = 8;
				}
				else if (itemType == L"Heal1") {
					index = 9;
				}
				else if (itemType == L"Heal2") {
					index = 10;
				}
				else if (itemType == L"Heal3") {
					index = 11;
				}
				else {
					std::cout << "check DB : SkillTable\n";
					break;
				}

				items[index]->CoolTime = ItemDataSchema.p_CoolTime;
				items[index]->Damage = ItemDataSchema.p_damage;
				items[index]->SkillPrice = ItemDataSchema.p_price;
			}
			else {
				break;
			}
		}


		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);



	}
	else {

		HandleDiagnosticRecord(hstmt, SQL_HANDLE_STMT, retcode);
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);
	}


}

void DataBase::readInventory(Client* pl)
{
	//�κ��丮�� ������ �޶�� Ŭ�󿡼� ��û�� ���� �����ش�.
	//��û�� ���� ��� -> �Ҹ�ǰ�� ������� ��
	//������ �κ��丮�� ���� �����ϰ� ���� ������ ��� ����

	//�κ��丮 ũ�⸸ŭ �̸� �����ΰ� �����صδ� ����..
	//�������� �߿��ϴϱ� �ٷιٷ� �����ؾߵ� �׷��� ���� ������ ���ص�...
	//�޸𸮴� �����ϱ� �ӵ��� �켱���� ����

	if (false == isConnect) return;

	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);

	std::wstring name = std::wstring(pl->name, &pl->name[strlen(pl->name)]);

	std::wstring command = std::format(L"EXEC readInventory '{}'", name);

	retcode = SQLExecDirect(hstmt, (SQLWCHAR*)command.c_str(), command.length());
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {

		retcode = SQLBindCol(hstmt, 1, SQL_C_WCHAR, &PlayerDataSchema.p_name, MAX_NAME_SIZE, &PlayerDataSchema.cb_name);
		retcode = SQLBindCol(hstmt, 2, SQL_C_LONG, &PlayerDataSchema.p_MusicScroll, 4, &PlayerDataSchema.cb_MusicScroll);
		retcode = SQLBindCol(hstmt, 3, SQL_C_LONG, &PlayerDataSchema.p_MusicScrollCount, 4, &PlayerDataSchema.cb_MusicScrollCount);

		for (int i = 0; i < 15; i++) {
			retcode = SQLFetch(hstmt);
			if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO)
			{
				pl->inventory->items[i].itemType = PlayerDataSchema.p_MusicScroll;
				pl->inventory->items[i].cnt = PlayerDataSchema.p_MusicScrollCount;

			}
			else {
				break;
			}
		}


	}
	else
		HandleDiagnosticRecord(hstmt, SQL_HANDLE_STMT, retcode);

	// Process data  
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);
	}


}

void DataBase::readClearMap(Client* pl)
{
	if (false == isConnect) return;

	SQLRETURN retcode = SQLAllocHandle(SQL_HANDLE_STMT, hdbc, &hstmt);

	std::wstring name = std::wstring(pl->name, &pl->name[strlen(pl->name)]);

	std::wstring command = std::format(L"EXEC readClearMap '{}'", name);

	retcode = SQLExecDirect(hstmt, (SQLWCHAR*)command.c_str(), command.length());
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {

		retcode = SQLBindCol(hstmt, 1, SQL_C_WCHAR, &PlayerDataSchema.p_name, MAX_NAME_SIZE, &PlayerDataSchema.cb_name);
		retcode = SQLBindCol(hstmt, 2, SQL_C_LONG, &PlayerDataSchema.p_MapName, 4, &PlayerDataSchema.cb_MapName);
		retcode = SQLBindCol(hstmt, 3, SQL_C_LONG, &PlayerDataSchema.p_ClearScore, 4, &PlayerDataSchema.cb_ClearScore);

		for (int i = 0; i < 15; i++) {
			retcode = SQLFetch(hstmt);
			if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO)
			{
				int Mapnum = PlayerDataSchema.p_MapName;
				pl->ClearMap[Mapnum] = PlayerDataSchema.p_ClearScore;

			}
			else {
				break;
			}
		}


	}
	else
		HandleDiagnosticRecord(hstmt, SQL_HANDLE_STMT, retcode);

	// Process data  
	if (retcode == SQL_SUCCESS || retcode == SQL_SUCCESS_WITH_INFO) {
		SQLCancel(hstmt);
		SQLFreeHandle(SQL_HANDLE_STMT, hstmt);
	}
}
