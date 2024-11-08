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
	public partial class PrayJackpotCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "weight")]
		public int Weight { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "Reward")]
		public List<Reward> Reward { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "Frequency")]
		public int Frequency { get; private set; }

		public PrayJackpotCfg()
		{
		}

		[SerializationConstructor]
		public PrayJackpotCfg(int Id, int Type, int Weight, List<Reward> Reward, int Frequency)
		{
			this.Id = Id;
			this.Type = Type;
			this.Weight = Weight;
			this.Reward = Reward;
			this.Frequency = Frequency;
		}
	}
	public partial class PrayJackpotCfgCollection : CfgCollectionBase<PrayJackpotCfg>
	{
		private const string dataPath = "PrayJackpot";
		private Dictionary<int, PrayJackpotCfg> idMapItems;
		private Dictionary<int, List<PrayJackpotCfg>> typeMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<PrayJackpotCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, PrayJackpotCfg>();
			typeMapItems = new Dictionary<int, List<PrayJackpotCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(typeMapItems, item.Type, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<PrayJackpotCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, PrayJackpotCfg>();
			typeMapItems = new Dictionary<int, List<PrayJackpotCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(typeMapItems, item.Type, item);
			}

			loaded = true;
		}

		public PrayJackpotCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<PrayJackpotCfg> GetDataByType(int type)
		{
			Load();

			if(!typeMapItems.TryGetValue(type, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}