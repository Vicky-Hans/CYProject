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
	public partial class HolySkillCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "value")]
		public List<string> Value { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "nextValue")]
		public List<string> NextValue { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "skillId")]
		public int SkillId { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "holyId")]
		public int HolyId { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "holyModelId")]
		public int HolyModelId { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "upModelId")]
		public int UpModelId { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "lvUnlockId")]
		public int LvUnlockId { get; private set; }

		public HolySkillCfg()
		{
		}

		[SerializationConstructor]
		public HolySkillCfg(int Id, List<string> Value, List<string> NextValue, int SkillId, int HolyId, int HolyModelId, int UpModelId, int LvUnlockId)
		{
			this.Id = Id;
			this.Value = Value;
			this.NextValue = NextValue;
			this.SkillId = SkillId;
			this.HolyId = HolyId;
			this.HolyModelId = HolyModelId;
			this.UpModelId = UpModelId;
			this.LvUnlockId = LvUnlockId;
		}
	}
	public partial class HolySkillCfgCollection : CfgCollectionBase<HolySkillCfg>
	{
		private const string dataPath = "HolySkill";
		private Dictionary<int, HolySkillCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<HolySkillCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HolySkillCfg>();
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

			var config = await DataTableManager.LoadTableAsync<HolySkillCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HolySkillCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public HolySkillCfg GetDataById(int id)
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