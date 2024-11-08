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
	public partial class JackpotCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "itemTypeName")]
		public string ItemTypeName { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "jackpotQua")]
		public int JackpotQua { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "weight")]
		public int Weight { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "randomReward")]
		public List<RandomReward> RandomReward { get; private set; }

		public JackpotCfg()
		{
		}

		[SerializationConstructor]
		public JackpotCfg(int Id, string ItemTypeName, int Type, int JackpotQua, int Weight, List<RandomReward> RandomReward)
		{
			this.Id = Id;
			this.ItemTypeName = ItemTypeName;
			this.Type = Type;
			this.JackpotQua = JackpotQua;
			this.Weight = Weight;
			this.RandomReward = RandomReward;
		}
	}
	public partial class JackpotCfgCollection : CfgCollectionBase<JackpotCfg>
	{
		private const string dataPath = "Jackpot";
		private Dictionary<int, JackpotCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<JackpotCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, JackpotCfg>();
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

			var config = await DataTableManager.LoadTableAsync<JackpotCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, JackpotCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public JackpotCfg GetDataById(int id)
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