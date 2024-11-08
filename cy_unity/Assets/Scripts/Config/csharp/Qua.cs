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
	public partial class QuaCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "itemBg")]
		public string ItemBg { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "itemBg1")]
		public string ItemBg1 { get; private set; }

		public QuaCfg()
		{
		}

		[SerializationConstructor]
		public QuaCfg(int Id, string Name, string ItemBg, string ItemBg1)
		{
			this.Id = Id;
			this.Name = Name;
			this.ItemBg = ItemBg;
			this.ItemBg1 = ItemBg1;
		}
	}
	public partial class QuaCfgCollection : CfgCollectionBase<QuaCfg>
	{
		private const string dataPath = "Qua";
		private Dictionary<int, QuaCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<QuaCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, QuaCfg>();
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

			var config = await DataTableManager.LoadTableAsync<QuaCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, QuaCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public QuaCfg GetDataById(int id)
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