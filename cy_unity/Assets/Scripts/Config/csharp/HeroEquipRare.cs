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
	public partial class HeroEquipRareCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "quaId")]
		public int QuaId { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "value")]
		public int Value { get; private set; }

		public HeroEquipRareCfg()
		{
		}

		[SerializationConstructor]
		public HeroEquipRareCfg(int Id, int QuaId, string Icon, int Value)
		{
			this.Id = Id;
			this.QuaId = QuaId;
			this.Icon = Icon;
			this.Value = Value;
		}
	}
	public partial class HeroEquipRareCfgCollection : CfgCollectionBase<HeroEquipRareCfg>
	{
		private const string dataPath = "HeroEquipRare";
		private Dictionary<int, HeroEquipRareCfg> idMapItems;
		private Dictionary<int, List<HeroEquipRareCfg>> quaIdMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<HeroEquipRareCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroEquipRareCfg>();
			quaIdMapItems = new Dictionary<int, List<HeroEquipRareCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(quaIdMapItems, item.QuaId, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<HeroEquipRareCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroEquipRareCfg>();
			quaIdMapItems = new Dictionary<int, List<HeroEquipRareCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(quaIdMapItems, item.QuaId, item);
			}

			loaded = true;
		}

		public HeroEquipRareCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<HeroEquipRareCfg> GetDataByQuaId(int quaId)
		{
			Load();

			if(!quaIdMapItems.TryGetValue(quaId, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}