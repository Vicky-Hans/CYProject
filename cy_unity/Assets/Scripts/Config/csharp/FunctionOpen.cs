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
	public partial class FunctionOpenCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "level")]
		public int Level { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "stage")]
		public int Stage { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "star")]
		public int Star { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "seq")]
		public int Seq { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "page")]
		public int Page { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "obtainId")]
		public int ObtainId { get; private set; }

		public FunctionOpenCfg()
		{
		}

		[SerializationConstructor]
		public FunctionOpenCfg(int Id, int Level, int Stage, int Star, int Seq, int Page, string Icon, int ObtainId)
		{
			this.Id = Id;
			this.Level = Level;
			this.Stage = Stage;
			this.Star = Star;
			this.Seq = Seq;
			this.Page = Page;
			this.Icon = Icon;
			this.ObtainId = ObtainId;
		}
	}
	public partial class FunctionOpenCfgCollection : CfgCollectionBase<FunctionOpenCfg>
	{
		private const string dataPath = "FunctionOpen";
		private Dictionary<int, FunctionOpenCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<FunctionOpenCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, FunctionOpenCfg>();
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

			var config = await DataTableManager.LoadTableAsync<FunctionOpenCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, FunctionOpenCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public FunctionOpenCfg GetDataById(int id)
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