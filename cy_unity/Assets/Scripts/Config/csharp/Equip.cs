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
	public partial class EquipCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "equipName")]
		public string EquipName { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "equipIcon")]
		public string EquipIcon { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "unlock")]
		public int Unlock { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "quality")]
		public int Quality { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "gridType")]
		public int GridType { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "maxLv")]
		public int MaxLv { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "maxComposeLv")]
		public int MaxComposeLv { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "atkType")]
		public int AtkType { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "doubleUnlock")]
		public int DoubleUnlock { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "model")]
		public List<List<int>> Model { get; private set; }

		[Key(11)]
		[JsonProperty(PropertyName = "target")]
		public int Target { get; private set; }

		[Key(12)]
		[JsonProperty(PropertyName = "attrType")]
		public List<string> AttrType { get; private set; }

		[Key(13)]
		[JsonProperty(PropertyName = "attr")]
		public List<WeaponAttr> Attr { get; private set; }

		[Key(14)]
		[JsonProperty(PropertyName = "equipSkill1")]
		public int EquipSkill1 { get; private set; }

		[Key(15)]
		[JsonProperty(PropertyName = "itemId")]
		public int ItemId { get; private set; }

		[Key(16)]
		[JsonProperty(PropertyName = "equipSkillId")]
		public List<int> EquipSkillId { get; private set; }

		[Key(17)]
		[JsonProperty(PropertyName = "jackpotId")]
		public List<int> JackpotId { get; private set; }

		[Key(18)]
		[JsonProperty(PropertyName = "weight")]
		public int Weight { get; private set; }

		public EquipCfg()
		{
		}

		[SerializationConstructor]
		public EquipCfg(int Id, string EquipName, string EquipIcon, int Unlock, int Quality, int GridType, int MaxLv, int MaxComposeLv, int AtkType, int DoubleUnlock, List<List<int>> Model, int Target, List<string> AttrType, List<WeaponAttr> Attr, int EquipSkill1, int ItemId, List<int> EquipSkillId, List<int> JackpotId, int Weight)
		{
			this.Id = Id;
			this.EquipName = EquipName;
			this.EquipIcon = EquipIcon;
			this.Unlock = Unlock;
			this.Quality = Quality;
			this.GridType = GridType;
			this.MaxLv = MaxLv;
			this.MaxComposeLv = MaxComposeLv;
			this.AtkType = AtkType;
			this.DoubleUnlock = DoubleUnlock;
			this.Model = Model;
			this.Target = Target;
			this.AttrType = AttrType;
			this.Attr = Attr;
			this.EquipSkill1 = EquipSkill1;
			this.ItemId = ItemId;
			this.EquipSkillId = EquipSkillId;
			this.JackpotId = JackpotId;
			this.Weight = Weight;
		}
	}
	public partial class EquipCfgCollection : CfgCollectionBase<EquipCfg>
	{
		private const string dataPath = "Equip";
		private Dictionary<int, EquipCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<EquipCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipCfg>();
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

			var config = await DataTableManager.LoadTableAsync<EquipCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public EquipCfg GetDataById(int id)
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