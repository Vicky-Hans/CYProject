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
	public partial class ProPictureCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "showType")]
		public int ShowType { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "quality")]
		public int Quality { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "Basemap")]
		public string Basemap { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "spEffect")]
		public string SpEffect { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "convert")]
		public List<Reward> Convert { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "description")]
		public string Description { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "seasonOr")]
		public int SeasonOr { get; private set; }

		public ProPictureCfg()
		{
		}

		[SerializationConstructor]
		public ProPictureCfg(int Id, int Type, int ShowType, int Quality, string Icon, string Basemap, string SpEffect, List<Reward> Convert, string Description, int SeasonOr)
		{
			this.Id = Id;
			this.Type = Type;
			this.ShowType = ShowType;
			this.Quality = Quality;
			this.Icon = Icon;
			this.Basemap = Basemap;
			this.SpEffect = SpEffect;
			this.Convert = Convert;
			this.Description = Description;
			this.SeasonOr = SeasonOr;
		}
	}
	public partial class ProPictureCfgCollection : CfgCollectionBase<ProPictureCfg>
	{
		private const string dataPath = "ProPicture";
		private Dictionary<int, ProPictureCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<ProPictureCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ProPictureCfg>();
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

			var config = await DataTableManager.LoadTableAsync<ProPictureCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ProPictureCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public ProPictureCfg GetDataById(int id)
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