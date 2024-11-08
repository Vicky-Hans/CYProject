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
	public partial class MonsterCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "stageId")]
		public int StageId { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "modelId")]
		public int ModelId { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "modelPicture")]
		public string ModelPicture { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "monsterPro")]
		public List<Attribute> MonsterPro { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "monsterType")]
		public int MonsterType { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "characterId")]
		public List<int> CharacterId { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "atkType")]
		public int AtkType { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "crowd")]
		public int Crowd { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "skillType")]
		public int SkillType { get; private set; }

		[Key(11)]
		[JsonProperty(PropertyName = "atkDistance")]
		public int AtkDistance { get; private set; }

		[Key(12)]
		[JsonProperty(PropertyName = "splitModelId")]
		public int SplitModelId { get; private set; }

		[Key(13)]
		[JsonProperty(PropertyName = "summonMonsterId")]
		public List<int> SummonMonsterId { get; private set; }

		[Key(14)]
		[JsonProperty(PropertyName = "drop")]
		public List<Reward> Drop { get; private set; }

		[Key(15)]
		[JsonProperty(PropertyName = "atkId")]
		public List<MonsterSkill> AtkId { get; private set; }

		[Key(16)]
		[JsonProperty(PropertyName = "killExp")]
		public int KillExp { get; private set; }

		[Key(17)]
		[JsonProperty(PropertyName = "killEnergy")]
		public int KillEnergy { get; private set; }

		public MonsterCfg()
		{
		}

		[SerializationConstructor]
		public MonsterCfg(int Id, int StageId, int Type, int ModelId, string ModelPicture, List<Attribute> MonsterPro, int MonsterType, List<int> CharacterId, int AtkType, int Crowd, int SkillType, int AtkDistance, int SplitModelId, List<int> SummonMonsterId, List<Reward> Drop, List<MonsterSkill> AtkId, int KillExp, int KillEnergy)
		{
			this.Id = Id;
			this.StageId = StageId;
			this.Type = Type;
			this.ModelId = ModelId;
			this.ModelPicture = ModelPicture;
			this.MonsterPro = MonsterPro;
			this.MonsterType = MonsterType;
			this.CharacterId = CharacterId;
			this.AtkType = AtkType;
			this.Crowd = Crowd;
			this.SkillType = SkillType;
			this.AtkDistance = AtkDistance;
			this.SplitModelId = SplitModelId;
			this.SummonMonsterId = SummonMonsterId;
			this.Drop = Drop;
			this.AtkId = AtkId;
			this.KillExp = KillExp;
			this.KillEnergy = KillEnergy;
		}
	}
	public partial class MonsterCfgCollection : CfgCollectionBase<MonsterCfg>
	{
		private const string dataPath = "Monster";
		private Dictionary<int, MonsterCfg> idMapItems;
		private Dictionary<int, List<MonsterCfg>> stageIdMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<MonsterCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MonsterCfg>();
			stageIdMapItems = new Dictionary<int, List<MonsterCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(stageIdMapItems, item.StageId, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<MonsterCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MonsterCfg>();
			stageIdMapItems = new Dictionary<int, List<MonsterCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(stageIdMapItems, item.StageId, item);
			}

			loaded = true;
		}

		public MonsterCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<MonsterCfg> GetDataByStageId(int stageId)
		{
			Load();

			if(!stageIdMapItems.TryGetValue(stageId, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}