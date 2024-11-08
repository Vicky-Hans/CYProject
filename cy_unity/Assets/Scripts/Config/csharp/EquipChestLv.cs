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
	public partial class EquipChestLvCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "exp")]
		public int Exp { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "total_exp_1")]
		public int Total_exp_1 { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "rewardAdd1")]
		public List<Reward> RewardAdd1 { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "rewardAdd2")]
		public List<Reward> RewardAdd2 { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "rewardAdd3")]
		public List<Reward> RewardAdd3 { get; private set; }

		public EquipChestLvCfg()
		{
		}

		[SerializationConstructor]
		public EquipChestLvCfg(int Id, int Exp, int Total_exp_1, List<Reward> RewardAdd1, List<Reward> RewardAdd2, List<Reward> RewardAdd3)
		{
			this.Id = Id;
			this.Exp = Exp;
			this.Total_exp_1 = Total_exp_1;
			this.RewardAdd1 = RewardAdd1;
			this.RewardAdd2 = RewardAdd2;
			this.RewardAdd3 = RewardAdd3;
		}
	}
	public partial class EquipChestLvCfgCollection : CfgCollectionBase<EquipChestLvCfg>
	{
		private const string dataPath = "EquipChestLv";
		private Dictionary<int, EquipChestLvCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<EquipChestLvCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipChestLvCfg>();
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

			var config = await DataTableManager.LoadTableAsync<EquipChestLvCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipChestLvCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public EquipChestLvCfg GetDataById(int id)
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