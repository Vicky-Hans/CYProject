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
	public partial class SecretCopyTalentCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "quality")]
		public int Quality { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "weight")]
		public int Weight { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "pickQuantity")]
		public int PickQuantity { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "equipSkillId")]
		public int EquipSkillId { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "season")]
		public List<int> Season { get; private set; }

		public SecretCopyTalentCfg()
		{
		}

		[SerializationConstructor]
		public SecretCopyTalentCfg(int Id, int Quality, int Type, int Weight, int PickQuantity, int EquipSkillId, List<int> Season)
		{
			this.Id = Id;
			this.Quality = Quality;
			this.Type = Type;
			this.Weight = Weight;
			this.PickQuantity = PickQuantity;
			this.EquipSkillId = EquipSkillId;
			this.Season = Season;
		}
	}
	public partial class SecretCopyTalentCfgCollection : CfgCollectionBase<SecretCopyTalentCfg>
	{
		private const string dataPath = "SecretCopyTalent";
		private Dictionary<int, SecretCopyTalentCfg> idMapItems;
		private Dictionary<int, List<SecretCopyTalentCfg>> qualityMapItems;
		private Dictionary<int, List<SecretCopyTalentCfg>> typeMapItems;
		private Dictionary<int, List<SecretCopyTalentCfg>> equipSkillIdMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<SecretCopyTalentCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, SecretCopyTalentCfg>();
			qualityMapItems = new Dictionary<int, List<SecretCopyTalentCfg>>();
			typeMapItems = new Dictionary<int, List<SecretCopyTalentCfg>>();
			equipSkillIdMapItems = new Dictionary<int, List<SecretCopyTalentCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(qualityMapItems, item.Quality, item);
				AddItem(typeMapItems, item.Type, item);
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

			var config = await DataTableManager.LoadTableAsync<SecretCopyTalentCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, SecretCopyTalentCfg>();
			qualityMapItems = new Dictionary<int, List<SecretCopyTalentCfg>>();
			typeMapItems = new Dictionary<int, List<SecretCopyTalentCfg>>();
			equipSkillIdMapItems = new Dictionary<int, List<SecretCopyTalentCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(qualityMapItems, item.Quality, item);
				AddItem(typeMapItems, item.Type, item);
				AddItem(equipSkillIdMapItems, item.EquipSkillId, item);
			}

			loaded = true;
		}

		public SecretCopyTalentCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<SecretCopyTalentCfg> GetDataByQuality(int quality)
		{
			Load();

			if(!qualityMapItems.TryGetValue(quality, out var itemList))
			{
				return null;
			}
			return itemList;
		}

		public List<SecretCopyTalentCfg> GetDataByType(int type)
		{
			Load();

			if(!typeMapItems.TryGetValue(type, out var itemList))
			{
				return null;
			}
			return itemList;
		}

		public List<SecretCopyTalentCfg> GetDataByEquipSkillId(int equipSkillId)
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