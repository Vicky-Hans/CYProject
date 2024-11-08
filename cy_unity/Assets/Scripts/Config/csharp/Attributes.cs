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
	public partial class AttributesCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "desc")]
		public string Desc { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "show")]
		public int Show { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "showType")]
		public int ShowType { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "unit")]
		public string Unit { get; private set; }

		public AttributesCfg()
		{
		}

		[SerializationConstructor]
		public AttributesCfg(int Id, int Type, string Icon, string Name, string Desc, int Show, int ShowType, string Unit)
		{
			this.Id = Id;
			this.Type = Type;
			this.Icon = Icon;
			this.Name = Name;
			this.Desc = Desc;
			this.Show = Show;
			this.ShowType = ShowType;
			this.Unit = Unit;
		}
	}
	public partial class AttributesCfgCollection : CfgCollectionBase<AttributesCfg>
	{
		private const string dataPath = "Attributes";
		private Dictionary<int, AttributesCfg> idMapItems;
		private Dictionary<string, AttributesCfg> nameMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<AttributesCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, AttributesCfg>();
			nameMapItems = new Dictionary<string, AttributesCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(nameMapItems, item.Name, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<AttributesCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, AttributesCfg>();
			nameMapItems = new Dictionary<string, AttributesCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(nameMapItems, item.Name, item);
			}

			loaded = true;
		}

		public AttributesCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public AttributesCfg GetDataByName(string name)
		{
			Load();

			if(!nameMapItems.TryGetValue(name, out var item))
			{
				return null;
			}
			return item;
		}

	}
}