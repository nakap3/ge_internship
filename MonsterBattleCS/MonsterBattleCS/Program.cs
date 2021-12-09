using System;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using FlatBuffers;


namespace MonsterBattleCS
{
	class Program
	{
		// FlatBuffersを使う段階になったら、この値を true にしよう！
		static readonly bool USE_FLATBUFFERS = true;


		/// モンスターデータ
		public class MonsterData
		{
			public string label;           //< ラベル
			public string name;            //< 名前
			public int hp;                 //< 体力
			public int ap;                 //< 攻撃力
			public int dp;                 //< 防御力

			public MonsterData(string label, string name, int hp, int ap, int dp)
			{
				this.label = label;
				this.name = name;
				this.hp = hp;
				this.ap = ap;
				this.dp = dp;
			}
		};

		/// モンスターリスト
		static readonly List<MonsterData> s_monsterList = new List<MonsterData>()
		{
			//               label        name           hp      ap      dp
			new MonsterData("demon",    "デーモン　",    800,    40,     25),
			new MonsterData("dragon",   "ドラゴン　",    900,    45,     10),
			new MonsterData("ghost",    "ゴースト　",    500,    20,     25),
			new MonsterData( "hapry",   "ハーピィ　",    600,    30,     20),
			new MonsterData("ninja",    "ニンジャ　",    400,    85,     20),
			new MonsterData("slime",    "スライム　",    200,    10,     90),
			new MonsterData("vampire",  "バンパイア",    700,    15,     60),
			new MonsterData("zombie",   "ゾンビ　　",    300,    20,     30)
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


		static void Attack(MonsterData offense, MonsterData defense)
		{
			int damage = offense.ap * (100 - defense.dp) / 100;
			defense.hp -= damage;
		}

		static int IsStronger(MonsterData offense, MonsterData defense)
		{
			while (offense.hp > 0 && defense.hp > 0)
			{
				Attack(offense, defense);
				Attack(defense, offense);
			}

			return defense.hp - offense.hp;
		}

		// Sort を使う場合はこんな感じ
		static int Battle2(in MonsterData a, in MonsterData b)
		{
			var aa = new MonsterData(a.label, a.name, a.hp, a.ap, a.dp);
			var bb = new MonsterData(b.label, b.name, b.hp, b.ap, b.dp);
			return IsStronger(aa, bb);
		}

		public class Battle : IComparer<MonsterData>
		{
			public int Compare(MonsterData a, MonsterData b)
			{
				var aa = new MonsterData(a.label, a.name, a.hp, a.ap, a.dp);
				var bb = new MonsterData(b.label, b.name, b.hp, b.ap, b.dp);
				return IsStronger(aa, bb);
			}
		}


		/// メイン処理
		static void Main(string[] args)
		{
			var monsterList = new List<MonsterData>();

			if (USE_FLATBUFFERS)
			{
				// FlatBuffersを使う段階になったら、ここにバイナリにシリアライズされたデータを読み込んで、 monsterList を構築する処理を実装しよう！

				// ここでは、引数でカレントディレクトリを貰うようにしている
				// 引数の設定は、プロジェクトのプロパティのデバッグのアプリケーション引数に $(ProjectDir) を設定すればよい
				// バイナリファイルの読み込み方はこれ以外にもいろんな方法があるので好きな方法を使えばよい
				// もちろん、実際のゲームアプリでは引数を指定するようなやり方は使えないので注意！
				System.IO.Directory.SetCurrentDirectory(args[0]);
				byte[] data = File.ReadAllBytes("MonsterData.mdfb");

				var bb = new ByteBuffer(data);
				var fbMonsterList = Data.FbMonsterList.GetRootAsFbMonsterList(bb);
				for (int i = 0; i < fbMonsterList.MonsterListLength; ++i)
				{
					var fbMonsterData = fbMonsterList.MonsterList(i).Value;
					var monster = new MonsterData(fbMonsterData.Label, fbMonsterData.Name, fbMonsterData.Hp, fbMonsterData.Ap, fbMonsterData.Dp);
					monsterList.Add(monster);
				}
			}
			else
			{
				// FlatBuffersを使わない段階では、ココで monsterList が構築される。
				monsterList = new List<MonsterData>(s_monsterList);
			}

			// ソート前の状態を表示
			Console.WriteLine("ソート前");
			foreach (var monster in monsterList)
			{
				Console.WriteLine("  {0, -10} : HP:{1, 3}, ATK:{2, 2}, DEF:{3, 2}", monster.name, monster.hp, monster.ap, monster.dp);
			}
			Console.WriteLine("");

			// monsterList を強い順に並べ替える処理をココに実装しよう！
			{
				// Sort を使う場合はこんな感じ
				// でもSortは非安定ソートなので、今回は利用しない
				//monsterList.Sort((a, b) => { return Battle2(a, b); });

				// 安定ソートは Linq の OrderBy で実現できる
				var battle = new Battle();
				monsterList = monsterList.OrderBy(e => e, battle).ToList();
			}

			// ソート後の状態を表示
			Console.WriteLine("ソート後");
			foreach (var monster in monsterList)
			{
				Console.WriteLine("  {0, -10} : HP:{1, 3}, ATK:{2, 2}, DEF:{3, 2}", monster.name, monster.hp, monster.ap, monster.dp);
			}
			Console.WriteLine("");
		}
	}
};
