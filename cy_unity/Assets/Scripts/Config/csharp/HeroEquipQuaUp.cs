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
	public partial class HeroEquipQuaUpCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "nextId")]
		public int NextId { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "quaId")]
		public int QuaId { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "quaStage")]
		public int QuaStage { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "maxLv")]
		public int MaxLv { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "num")]
		public int Num { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "value")]
		public int Value { get; private set; }

		public HeroEquipQuaUpCfg()
		{
		}

		[SerializationConstructor]
		public HeroEquipQuaUpCfg(int Id, int NextId, int QuaId, int QuaStage, int MaxLv, int Type, int Num, int Value)
		{
			this.Id = Id;
			this.NextId = NextId;
			this.QuaId = QuaId;
			this.QuaStage = QuaStage;
			this.MaxLv = MaxLv;
			this.Type = Type;
			this.Num = Num;
			this.Value = Value;
		}
	}
	public partial class HeroEquipQuaUpCfgCollection : CfgCollectionBase<HeroEquipQuaUpCfg>
	{
		private const string dataPath = "HeroEquipQuaUp";
		private Dictionary<int, HeroEquipQuaUpCfg> idMapItems;
		private Dictionary<int, List<HeroEquipQuaUpCfg>> quaIdMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<HeroEquipQuaUpCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroEquipQuaUpCfg>();
			quaIdMapItems = new Dictionary<int, List<HeroEquipQuaUpCfg>>();
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

			var config = await DataTableManager.LoadTableAsync<HeroEquipQuaUpCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroEquipQuaUpCfg>();
			quaIdMapItems = new Dictionary<int, List<HeroEquipQuaUpCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(quaIdMapItems, item.QuaId, item);
			}

			loaded = true;
		}

		public HeroEquipQuaUpCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<HeroEquipQuaUpCfg> GetDataByQuaId(int quaId)
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