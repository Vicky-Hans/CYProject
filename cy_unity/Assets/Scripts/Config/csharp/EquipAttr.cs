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
	public partial class EquipAttrCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "equipAttrIcon")]
		public string EquipAttrIcon { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "equipAttrName")]
		public string EquipAttrName { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "equipAttrColor")]
		public string EquipAttrColor { get; private set; }

		public EquipAttrCfg()
		{
		}

		[SerializationConstructor]
		public EquipAttrCfg(int Id, string EquipAttrIcon, string EquipAttrName, string EquipAttrColor)
		{
			this.Id = Id;
			this.EquipAttrIcon = EquipAttrIcon;
			this.EquipAttrName = EquipAttrName;
			this.EquipAttrColor = EquipAttrColor;
		}
	}
	public partial class EquipAttrCfgCollection : CfgCollectionBase<EquipAttrCfg>
	{
		private const string dataPath = "EquipAttr";
		private Dictionary<int, EquipAttrCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<EquipAttrCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipAttrCfg>();
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

			var config = await DataTableManager.LoadTableAsync<EquipAttrCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipAttrCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public EquipAttrCfg GetDataById(int id)
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