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
	public partial class CopyEquipCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "atkType")]
		public int AtkType { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "weight")]
		public int Weight { get; private set; }

		public CopyEquipCfg()
		{
		}

		[SerializationConstructor]
		public CopyEquipCfg(int Id, int Type, int AtkType, int Weight)
		{
			this.Id = Id;
			this.Type = Type;
			this.AtkType = AtkType;
			this.Weight = Weight;
		}
	}
	public partial class CopyEquipCfgCollection : CfgCollectionBase<CopyEquipCfg>
	{
		private const string dataPath = "CopyEquip";
		private Dictionary<int, CopyEquipCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<CopyEquipCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, CopyEquipCfg>();
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

			var config = await DataTableManager.LoadTableAsync<CopyEquipCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, CopyEquipCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public CopyEquipCfg GetDataById(int id)
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