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
	public partial class EquipSkillCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "value")]
		public List<string> Value { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "skillId")]
		public int SkillId { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "quality")]
		public int Quality { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "previewSkill")]
		public List<int> PreviewSkill { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "skillType")]
		public int SkillType { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "equipId")]
		public int EquipId { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "DelSkill")]
		public int DelSkill { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "lvUnlockId")]
		public int LvUnlockId { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "skill2UnlockId")]
		public List<int> Skill2UnlockId { get; private set; }

		[Key(11)]
		[JsonProperty(PropertyName = "rejectSkill")]
		public List<int> RejectSkill { get; private set; }

		[Key(12)]
		[JsonProperty(PropertyName = "cardTimes")]
		public int CardTimes { get; private set; }

		[Key(13)]
		[JsonProperty(PropertyName = "lvAstrict")]
		public int LvAstrict { get; private set; }

		[Key(14)]
		[JsonProperty(PropertyName = "frontSkill")]
		public List<int> FrontSkill { get; private set; }

		[Key(15)]
		[JsonProperty(PropertyName = "ifShow")]
		public int IfShow { get; private set; }

		[Key(16)]
		[JsonProperty(PropertyName = "default")]
		public int Default { get; private set; }

		public EquipSkillCfg()
		{
		}

		[SerializationConstructor]
		public EquipSkillCfg(int Id, string Icon, List<string> Value, int SkillId, int Quality, List<int> PreviewSkill, int SkillType, int EquipId, int DelSkill, int LvUnlockId, List<int> Skill2UnlockId, List<int> RejectSkill, int CardTimes, int LvAstrict, List<int> FrontSkill, int IfShow, int Default)
		{
			this.Id = Id;
			this.Icon = Icon;
			this.Value = Value;
			this.SkillId = SkillId;
			this.Quality = Quality;
			this.PreviewSkill = PreviewSkill;
			this.SkillType = SkillType;
			this.EquipId = EquipId;
			this.DelSkill = DelSkill;
			this.LvUnlockId = LvUnlockId;
			this.Skill2UnlockId = Skill2UnlockId;
			this.RejectSkill = RejectSkill;
			this.CardTimes = CardTimes;
			this.LvAstrict = LvAstrict;
			this.FrontSkill = FrontSkill;
			this.IfShow = IfShow;
			this.Default = Default;
		}
	}
	public partial class EquipSkillCfgCollection : CfgCollectionBase<EquipSkillCfg>
	{
		private const string dataPath = "EquipSkill";
		private Dictionary<int, EquipSkillCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<EquipSkillCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipSkillCfg>();
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

			var config = await DataTableManager.LoadTableAsync<EquipSkillCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipSkillCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public EquipSkillCfg GetDataById(int id)
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