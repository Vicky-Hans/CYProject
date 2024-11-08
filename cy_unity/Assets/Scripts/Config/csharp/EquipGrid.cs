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
	public partial class EquipGridCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "equipGridIcon")]
		public string EquipGridIcon { get; private set; }

		public EquipGridCfg()
		{
		}

		[SerializationConstructor]
		public EquipGridCfg(int Id, string EquipGridIcon)
		{
			this.Id = Id;
			this.EquipGridIcon = EquipGridIcon;
		}
	}
	public partial class EquipGridCfgCollection : CfgCollectionBase<EquipGridCfg>
	{
		private const string dataPath = "EquipGrid";
		private Dictionary<int, EquipGridCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<EquipGridCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipGridCfg>();
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

			var config = await DataTableManager.LoadTableAsync<EquipGridCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipGridCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public EquipGridCfg GetDataById(int id)
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