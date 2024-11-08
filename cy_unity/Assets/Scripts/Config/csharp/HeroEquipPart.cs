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
	public partial class HeroEquipPartCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "seqId")]
		public int SeqId { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "heroEquipIcon")]
		public string HeroEquipIcon { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "heroEquipIconMin")]
		public string HeroEquipIconMin { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "lvItemId")]
		public int LvItemId { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "quaItemId")]
		public List<int> QuaItemId { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "quaItemId_S")]
		public List<int> QuaItemId_S { get; private set; }

		public HeroEquipPartCfg()
		{
		}

		[SerializationConstructor]
		public HeroEquipPartCfg(int Id, string Name, int SeqId, string HeroEquipIcon, string HeroEquipIconMin, int LvItemId, List<int> QuaItemId, List<int> QuaItemId_S)
		{
			this.Id = Id;
			this.Name = Name;
			this.SeqId = SeqId;
			this.HeroEquipIcon = HeroEquipIcon;
			this.HeroEquipIconMin = HeroEquipIconMin;
			this.LvItemId = LvItemId;
			this.QuaItemId = QuaItemId;
			this.QuaItemId_S = QuaItemId_S;
		}
	}
	public partial class HeroEquipPartCfgCollection : CfgCollectionBase<HeroEquipPartCfg>
	{
		private const string dataPath = "HeroEquipPart";
		private Dictionary<int, HeroEquipPartCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<HeroEquipPartCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroEquipPartCfg>();
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

			var config = await DataTableManager.LoadTableAsync<HeroEquipPartCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroEquipPartCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public HeroEquipPartCfg GetDataById(int id)
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