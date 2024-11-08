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
	public partial class EquipSlotsCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "unlock")]
		public int Unlock { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "equip")]
		public int Equip { get; private set; }

		public EquipSlotsCfg()
		{
		}

		[SerializationConstructor]
		public EquipSlotsCfg(int Id, int Unlock, int Equip)
		{
			this.Id = Id;
			this.Unlock = Unlock;
			this.Equip = Equip;
		}
	}
	public partial class EquipSlotsCfgCollection : CfgCollectionBase<EquipSlotsCfg>
	{
		private const string dataPath = "EquipSlots";
		private Dictionary<int, EquipSlotsCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<EquipSlotsCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipSlotsCfg>();
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

			var config = await DataTableManager.LoadTableAsync<EquipSlotsCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipSlotsCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public EquipSlotsCfg GetDataById(int id)
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