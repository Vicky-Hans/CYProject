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
	public partial class ShopCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "content")]
		public List<ShopContent> Content { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "functionId")]
		public int FunctionId { get; private set; }

		public ShopCfg()
		{
		}

		[SerializationConstructor]
		public ShopCfg(int Id, string Name, List<ShopContent> Content, int FunctionId)
		{
			this.Id = Id;
			this.Name = Name;
			this.Content = Content;
			this.FunctionId = FunctionId;
		}
	}
	public partial class ShopCfgCollection : CfgCollectionBase<ShopCfg>
	{
		private const string dataPath = "Shop";
		private Dictionary<int, ShopCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<ShopCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ShopCfg>();
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

			var config = await DataTableManager.LoadTableAsync<ShopCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ShopCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public ShopCfg GetDataById(int id)
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