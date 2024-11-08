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
	public partial class MonthlyVipEffectCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "dec")]
		public string Dec { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "value")]
		public List<int> Value { get; private set; }

		public MonthlyVipEffectCfg()
		{
		}

		[SerializationConstructor]
		public MonthlyVipEffectCfg(int Id, string Dec, List<int> Value)
		{
			this.Id = Id;
			this.Dec = Dec;
			this.Value = Value;
		}
	}
	public partial class MonthlyVipEffectCfgCollection : CfgCollectionBase<MonthlyVipEffectCfg>
	{
		private const string dataPath = "MonthlyVipEffect";
		private Dictionary<int, MonthlyVipEffectCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<MonthlyVipEffectCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MonthlyVipEffectCfg>();
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

			var config = await DataTableManager.LoadTableAsync<MonthlyVipEffectCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MonthlyVipEffectCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public MonthlyVipEffectCfg GetDataById(int id)
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