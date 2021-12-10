using System;
using System.Collections.Generic;

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
			public string label;			//< ラベル
			public string name;				//< 名前
			public int hp;					//< 体力
			public int ap;					//< 攻撃力
			public int dp;					//< 防御力
			public Data.Hand hand;          //< じゃんけん属性

			public MonsterData(string label, string name, int hp, int ap, int dp, Data.Hand hand = Data.Hand.stone)
			{
				this.label = label;
				this.name = name;
				this.hp = hp;
				this.ap = ap;
				this.dp = dp;
				this.hand = hand;
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
			int ap = offense.ap;
			if ((offense.hand + 3 - defense.hand) % 3 == 1) ap = ap * 3 / 2;
			int damage = ap * (100 - defense.dp) / 100;
			defense.hp -= damage;
		}

		static int IsStronger(MonsterData offense, MonsterData defense)
		{
			while (offense.hp > 0 && defense.hp > 0)
			{
				Attack(offense, defense);
				Attack(defense, offense);
			}

			return offense.hp <= 0 && defense.hp <= 0 ? 0 : offense.hp - defense.hp;
		}

		static int Battle(in MonsterData a, in MonsterData b)
		{
			var aa = new MonsterData(a.label, a.name, a.hp, a.ap, a.dp, a.hand);
			var bb = new MonsterData(b.label, b.name, b.hp, b.ap, b.dp, b.hand);
			return IsStronger(aa, bb);
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
					var monster = new MonsterData(fbMonsterData.Label, fbMonsterData.Name,
						fbMonsterData.Hp, fbMonsterData.Ap, fbMonsterData.Dp,
						fbMonsterData.Hand);
					monsterList.Add(monster);
				}
			}
			else
			{
				// FlatBuffersを使わない段階では、ココで monsterList が構築される。
				monsterList = new List<MonsterData>(s_monsterList);
			}

			// じゃんけん属性も含めて表示元の状態を表示
			foreach (var monster in monsterList)
			{
				Console.WriteLine("  {0, -10} : HP:{1, 3}, ATK:{2, 2}, DEF:{3, 2} {4}", monster.name, monster.hp, monster.ap, monster.dp, monster.hand.ToString());
			}
			Console.WriteLine();

			// 総当たり戦の結果を格納する二次元配列を用意
			var size = monsterList.Count;
			var winMatrix = new int[size, size];
			// 総当たり戦
			for (int offense = 0; offense < (int)size; ++offense)
			{
				for (int defense = 0; defense < (int)size; ++defense)
				{
					winMatrix[offense, defense] = Battle(monsterList[offense], monsterList[defense]);
				}
			}
			// 結果表を表示
			// と同時に優勝者もココで調べておく
			var winners = new List<int>();
			int high = 0;
			for (int offense = 0; offense < size; ++offense)
			{
				string str = "";
				int score = 0;
				for (int defense = 0; defense < size; ++defense)
				{
					var win = winMatrix[offense, defense];
					score += win == 0 ? 1 : win > 0 ? 3 : 0;
					str += win == 0 ? offense == defense ? "＼" : "・" : win > 0 ? "〇" : "×";
				}
				Console.WriteLine("　{0, -10} : {1} : {2, 3}点", monsterList[offense].name, str, score);
				// 暫定優勝者の更新処理
				if (score > high)
				{
					winners.Clear();
					high = score;
				}
				if (score >= high) winners.Add(offense);
			}
			// 優勝者を表示
			Console.Write("\n優勝は {0}点の", high);
			foreach (var winner in winners)
			{
				Console.Write(" {0} ", monsterList[winner].name);
			}
			Console.WriteLine();
		}
	}
};
