#include <vector>
#include <algorithm>
#include <iostream>
#include <Windows.h>

#include "MonsterData_generated.h"
#include "flatbuffers/util.h"


#define USE_FLATBUFFERS 0 


/// モンスターデータ
struct MonsterData
{
	const char* label;      //< ラベル
	const char* name;       //< 名前
	int hp;                 //< 体力
	int ap;                 //< 攻撃力
	int dp;                 //< 防御力
};

/// モンスターリスト
static constexpr MonsterData s_monsterList[] =
{
	{ "demon",      "デーモン　", 	800,    40, 	25  },
	{ "dragon", 	"ドラゴン　", 	900,	45, 	10  },
	{ "ghost",  	"ゴースト　", 	500,    20, 	25  },
	{ "hapry",  	"ハーピィ　", 	600,	30, 	20  },
	{ "ninja",	    "ニンジャ　",	400,	85, 	20  },
	{ "slime",	    "スライム　",	200,	10,	    90  },
	{ "vampire",	"バンパイア",	700,	15,	    60  },
	{ "zombie",	    "ゾンビ　　",   300,	20,	    30  }
};

// モンスターを強い順に並べなさい
// 
// どのモンスターが強いかは、実際に戦わせて決めます。
// 戦闘は 1 vs 1 で行い、先に力尽きた方が負けです。（力尽きる＝ヒットポイントが0以下になる）
// ステータスの意味は以下の通りです。
// ・hp  残体力
// ・ap  攻撃力（この値がそのまま基礎タメージ値になります）
// ・dp  防御力（この値の分だけ、受ける基礎ダメージの割合を減らします）
// 例）攻撃側の ap が 20 で、防御側の dp が 30 の時は、防御側は 20 の威力の攻撃を 30% 吸収するんで、 14 ダメージを受けます。
// ダメージ値に小数が出た場合は切り捨てられます。（例：1.5 ⇒ 1）
// 2体は同時に戦います。同時に力尽きた時は、力尽きた時の体力が大きい方が勝ちです。（例：残り体力が -20 と -5になった場合は、-5の方が勝ち）


void setCursorPos(int x, int y)
{
	HANDLE hCons = GetStdHandle(STD_OUTPUT_HANDLE);
	COORD pos{ pos.X = x, pos.Y = y };
	SetConsoleCursorPosition(hCons, pos);
}

void Attack(MonsterData& offense, MonsterData& defense, int y, int turn)
{
	int damage = offense.ap * (100 - defense.dp) / 100;
	defense.hp -= damage;

	//setCursorPos(0, 1);
	//printf("TURN %d", turn);
	//setCursorPos(0, y);
	//const char* text = "  %10s の攻撃！ %10s に %3d のダメージ！ 残りHP(%4d)\n";
	//printf(text, offense.name, defense.name, damage, defense.hp);
	//printf("  %10s の攻撃！ %10s に %3d のダメージ！ 残りHP(%4d)\n", offense.name, defense.name, damage, defense.hp);
}

bool IsStronger(MonsterData& a, MonsterData& b)
{
	int turn = 1;
	while (a.hp > 0 && b.hp > 0)
	{
		Attack(a, b, 2, turn);
		Attack(b, a, 3, turn);
		++turn;
	}
	//(void)getchar();

	return a.hp >= b.hp;
}

bool Battle(const MonsterData& a, const MonsterData& b)
{
	//setCursorPos(0, 0);
	//printf("%s vs %s\n", a.name, b.name);
	//std::cout << a.name << " vs " << b.name << std::endl;

	auto aa = a;
	auto bb = b;
	return IsStronger(aa, bb);
}

int main()
{
#if defined(USE_FLATBUFFERS) && USE_FLATBUFFERS
	// flatbuffersでバイナリにシリアライズされたデータを読み込んで、monsterListを構築する
	std::vector<MonsterData> monsterList;
	std::string binaryData;
	if (flatbuffers::LoadFile("ToolsAndData/MonsterData.mdfb", true, &binaryData))
	{
		auto monsters = Data::GetMonsterList(binaryData.data());
		MonsterData monster;
		for (const auto& it : *monsters->monster_list())
		{
			monster.label = it->label()->c_str();
			monster.name = it->name()->c_str();
			monster.hp = it->hp();
			monster.ap = it->ap();
			monster.dp = it->dp();
			monsterList.push_back(monster);
		}
	}
#else
	std::vector<MonsterData> monsterList(std::begin(s_monsterList), std::end(s_monsterList));
#endif

	// コードページ(文字コード)を UTF-8 にするおまじない
	//SetConsoleOutputCP(65001);


	// ソート前の状態を表示
	//setCursorPos(0, 5);
	for (const auto& monster : monsterList)
	{
		printf("%-10s : HP:%3d, ATK:%2d, DEF:%2d\n", monster.name, monster.hp, monster.ap, monster.dp);
	}
	printf("\n");

	// ソート
	std::sort(monsterList.begin(), monsterList.end(), [](const auto& a, const auto& b) { return Battle(a, b); });


	// ソート後の状態を表示
	//setCursorPos(0, 14);
	for (const auto& monster : monsterList)
	{
		printf("%-10s : HP:%3d, ATK:%2d, DEF:%2d\n", monster.name, monster.hp, monster.ap, monster.dp);
	}
	printf("\n");
}

