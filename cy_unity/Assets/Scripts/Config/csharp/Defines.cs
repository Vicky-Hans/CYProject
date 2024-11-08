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
	public partial class DefinesCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "attrib")]
		public string Attrib { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "content")]
		public List<int> Content { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "reward")]
		public List<Reward> Reward { get; private set; }

		public DefinesCfg()
		{
		}

		[SerializationConstructor]
		public DefinesCfg(int Id, string Attrib, List<int> Content, List<Reward> Reward)
		{
			this.Id = Id;
			this.Attrib = Attrib;
			this.Content = Content;
			this.Reward = Reward;
		}
	}
	public partial class DefinesCfgCollection : CfgCollectionBase<DefinesCfg>
	{
		private const string dataPath = "Defines";
		private Dictionary<int, DefinesCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<DefinesCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DefinesCfg>();
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

			var config = await DataTableManager.LoadTableAsync<DefinesCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DefinesCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public DefinesCfg GetDataById(int id)
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