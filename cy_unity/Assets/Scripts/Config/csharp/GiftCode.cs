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
	public partial class GiftCodeCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public string Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "unlock")]
		public int Unlock { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "openTime")]
		public string OpenTime { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "endTime")]
		public string EndTime { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "reward")]
		public List<Reward> Reward { get; private set; }

		public GiftCodeCfg()
		{
		}

		[SerializationConstructor]
		public GiftCodeCfg(string Id, int Unlock, string OpenTime, string EndTime, List<Reward> Reward)
		{
			this.Id = Id;
			this.Unlock = Unlock;
			this.OpenTime = OpenTime;
			this.EndTime = EndTime;
			this.Reward = Reward;
		}
	}
	public partial class GiftCodeCfgCollection : CfgCollectionBase<GiftCodeCfg>
	{
		private const string dataPath = "GiftCode";
		private Dictionary<string, GiftCodeCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<GiftCodeCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<string, GiftCodeCfg>();
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

			var config = await DataTableManager.LoadTableAsync<GiftCodeCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<string, GiftCodeCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public GiftCodeCfg GetDataById(string id)
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