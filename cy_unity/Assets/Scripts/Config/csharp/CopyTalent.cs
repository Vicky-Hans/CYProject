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
	public partial class CopyTalentCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "quality")]
		public int Quality { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "weight")]
		public int Weight { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "pickQuantity")]
		public int PickQuantity { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "equipSkillId")]
		public int EquipSkillId { get; private set; }

		public CopyTalentCfg()
		{
		}

		[SerializationConstructor]
		public CopyTalentCfg(int Id, int Quality, int Weight, int PickQuantity, int EquipSkillId)
		{
			this.Id = Id;
			this.Quality = Quality;
			this.Weight = Weight;
			this.PickQuantity = PickQuantity;
			this.EquipSkillId = EquipSkillId;
		}
	}
	public partial class CopyTalentCfgCollection : CfgCollectionBase<CopyTalentCfg>
	{
		private const string dataPath = "CopyTalent";
		private Dictionary<int, CopyTalentCfg> idMapItems;
		private Dictionary<int, List<CopyTalentCfg>> qualityMapItems;
		private Dictionary<int, List<CopyTalentCfg>> equipSkillIdMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<CopyTalentCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, CopyTalentCfg>();
			qualityMapItems = new Dictionary<int, List<CopyTalentCfg>>();
			equipSkillIdMapItems = new Dictionary<int, List<CopyTalentCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(qualityMapItems, item.Quality, item);
				AddItem(equipSkillIdMapItems, item.EquipSkillId, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<CopyTalentCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, CopyTalentCfg>();
			qualityMapItems = new Dictionary<int, List<CopyTalentCfg>>();
			equipSkillIdMapItems = new Dictionary<int, List<CopyTalentCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(qualityMapItems, item.Quality, item);
				AddItem(equipSkillIdMapItems, item.EquipSkillId, item);
			}

			loaded = true;
		}

		public CopyTalentCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<CopyTalentCfg> GetDataByQuality(int quality)
		{
			Load();

			if(!qualityMapItems.TryGetValue(quality, out var itemList))
			{
				return null;
			}
			return itemList;
		}

		public List<CopyTalentCfg> GetDataByEquipSkillId(int equipSkillId)
		{
			Load();

			if(!equipSkillIdMapItems.TryGetValue(equipSkillId, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}