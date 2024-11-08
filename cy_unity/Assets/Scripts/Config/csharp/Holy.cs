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
	public partial class HolyCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "holyName")]
		public string HolyName { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "holyIcon")]
		public string HolyIcon { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "quality")]
		public int Quality { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "maxLv")]
		public int MaxLv { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "holyModel")]
		public List<int> HolyModel { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "holySkillId")]
		public List<List<int>> HolySkillId { get; private set; }

		public HolyCfg()
		{
		}

		[SerializationConstructor]
		public HolyCfg(int Id, string HolyName, string HolyIcon, int Quality, int MaxLv, List<int> HolyModel, List<List<int>> HolySkillId)
		{
			this.Id = Id;
			this.HolyName = HolyName;
			this.HolyIcon = HolyIcon;
			this.Quality = Quality;
			this.MaxLv = MaxLv;
			this.HolyModel = HolyModel;
			this.HolySkillId = HolySkillId;
		}
	}
	public partial class HolyCfgCollection : CfgCollectionBase<HolyCfg>
	{
		private const string dataPath = "Holy";
		private Dictionary<int, HolyCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<HolyCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HolyCfg>();
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

			var config = await DataTableManager.LoadTableAsync<HolyCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HolyCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public HolyCfg GetDataById(int id)
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