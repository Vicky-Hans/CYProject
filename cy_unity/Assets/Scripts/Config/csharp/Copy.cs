using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using MessagePack;
using DH.Config;
using System;

using UnityEngine.Scripting;

namespace DH.Config 
{
	[Preserve]
	[DHDataTableObject]
	[MessagePackObject]
	public partial class CopyCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "nextPass")]
		public int NextPass { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "stageType")]
		public int StageType { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "hangup")]
		public List<Reward> Hangup { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "special")]
		public List<Reward> Special { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "specialLimit")]
		public List<int> SpecialLimit { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "heroEquip")]
		public List<Reward> HeroEquip { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "heroEquipLimit")]
		public List<int> HeroEquipLimit { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "battle_mapy")]
		public string Battle_mapy { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "levelWaves")]
		public int LevelWaves { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "passReward")]
		public List<Reward> PassReward { get; private set; }

		[Key(11)]
		[JsonProperty(PropertyName = "numReward")]
		public List<Reward> NumReward { get; private set; }

		[Key(12)]
		[JsonProperty(PropertyName = "spcRewar")]
		public List<WaveReward> SpcRewar { get; private set; }

		[Key(13)]
		[JsonProperty(PropertyName = "copyReward1")]
		public List<Reward> CopyReward1 { get; private set; }

		[Key(14)]
		[JsonProperty(PropertyName = "copyReward2")]
		public List<Reward> CopyReward2 { get; private set; }

		[Key(15)]
		[JsonProperty(PropertyName = "copyReward3")]
		public List<Reward> CopyReward3 { get; private set; }

		[Key(16)]
		[JsonProperty(PropertyName = "condition")]
		public List<Condition> Condition { get; private set; }

		[Key(17)]
		[JsonProperty(PropertyName = "expend")]
		public int Expend { get; private set; }

		[Key(18)]
		[JsonProperty(PropertyName = "waveStartId")]
		public List<int> WaveStartId { get; private set; }

		[Key(19)]
		[JsonProperty(PropertyName = "reward")]
		public List<Reward> Reward { get; private set; }

		[Key(20)]
		[JsonProperty(PropertyName = "MonPreview")]
		public List<string> MonPreview { get; private set; }

		[Key(21)]
		[JsonProperty(PropertyName = "BossPreview")]
		public List<string> BossPreview { get; private set; }

		[Key(22)]
		[JsonProperty(PropertyName = "preview")]
		public string Preview { get; private set; }

		[Key(23)]
		[JsonProperty(PropertyName = "coin")]
		public List<int> Coin { get; private set; }

		public CopyCfg()
		{
		}

		[SerializationConstructor]
		public CopyCfg(int Id, int NextPass, int StageType, List<Reward> Hangup, List<Reward> Special, List<int> SpecialLimit, List<Reward> HeroEquip, List<int> HeroEquipLimit, string Battle_mapy, int LevelWaves, List<Reward> PassReward, List<Reward> NumReward, List<WaveReward> SpcRewar, List<Reward> CopyReward1, List<Reward> CopyReward2, List<Reward> CopyReward3, List<Condition> Condition, int Expend, List<int> WaveStartId, List<Reward> Reward, List<string> MonPreview, List<string> BossPreview, string Preview, List<int> Coin)
		{
			this.Id = Id;
			this.NextPass = NextPass;
			this.StageType = StageType;
			this.Hangup = Hangup;
			this.Special = Special;
			this.SpecialLimit = SpecialLimit;
			this.HeroEquip = HeroEquip;
			this.HeroEquipLimit = HeroEquipLimit;
			this.Battle_mapy = Battle_mapy;
			this.LevelWaves = LevelWaves;
			this.PassReward = PassReward;
			this.NumReward = NumReward;
			this.SpcRewar = SpcRewar;
			this.CopyReward1 = CopyReward1;
			this.CopyReward2 = CopyReward2;
			this.CopyReward3 = CopyReward3;
			this.Condition = Condition;
			this.Expend = Expend;
			this.WaveStartId = WaveStartId;
			this.Reward = Reward;
			this.MonPreview = MonPreview;
			this.BossPreview = BossPreview;
			this.Preview = Preview;
			this.Coin = Coin;
		}
	}
	public partial class CopyCfgCollection : CfgCollectionBase<CopyCfg>
	{
		private const string dataPath = "Copy";
		private Dictionary<int, CopyCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<CopyCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, CopyCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<CopyCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, CopyCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public CopyCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

	}
}