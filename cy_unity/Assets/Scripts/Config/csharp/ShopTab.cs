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
	public partial class ShopTabCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "content")]
		public List<Element> Content { get; private set; }

		public ShopTabCfg()
		{
		}

		[SerializationConstructor]
		public ShopTabCfg(int Id, string Name, List<Element> Content)
		{
			this.Id = Id;
			this.Name = Name;
			this.Content = Content;
		}
	}
	public partial class ShopTabCfgCollection : CfgCollectionBase<ShopTabCfg>
	{
		private const string dataPath = "ShopTab";
		private Dictionary<int, ShopTabCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<ShopTabCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ShopTabCfg>();
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

			var config = await DataTableManager.LoadTableAsync<ShopTabCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ShopTabCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public ShopTabCfg GetDataById(int id)
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