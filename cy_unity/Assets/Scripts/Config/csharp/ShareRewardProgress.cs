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
	public partial class ShareRewardProgressCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "value1")]
		public int Value1 { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "reward")]
		public List<Reward> Reward { get; private set; }

		public ShareRewardProgressCfg()
		{
		}

		[SerializationConstructor]
		public ShareRewardProgressCfg(int Id, int Value1, List<Reward> Reward)
		{
			this.Id = Id;
			this.Value1 = Value1;
			this.Reward = Reward;
		}
	}
	public partial class ShareRewardProgressCfgCollection : CfgCollectionBase<ShareRewardProgressCfg>
	{
		private const string dataPath = "ShareRewardProgress";
		private Dictionary<int, ShareRewardProgressCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<ShareRewardProgressCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ShareRewardProgressCfg>();
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

			var config = await DataTableManager.LoadTableAsync<ShareRewardProgressCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ShareRewardProgressCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public ShareRewardProgressCfg GetDataById(int id)
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