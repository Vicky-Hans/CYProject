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
	public partial class MonsterModelCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "modelType")]
		public int ModelType { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "model")]
		public string Model { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "modelSize")]
		public int ModelSize { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "atkAction")]
		public string AtkAction { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "skillAction1")]
		public string SkillAction1 { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "skillAction2")]
		public string SkillAction2 { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "hurtAction")]
		public string HurtAction { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "walkAction")]
		public string WalkAction { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "deadAction")]
		public string DeadAction { get; private set; }

		[Key(11)]
		[JsonProperty(PropertyName = "enterAction")]
		public string EnterAction { get; private set; }

		[Key(12)]
		[JsonProperty(PropertyName = "deadEffect")]
		public string DeadEffect { get; private set; }

		[Key(13)]
		[JsonProperty(PropertyName = "description")]
		public string Description { get; private set; }

		[Key(14)]
		[JsonProperty(PropertyName = "enterId")]
		public int EnterId { get; private set; }

		[Key(15)]
		[JsonProperty(PropertyName = "atkDistance")]
		public int AtkDistance { get; private set; }

		public MonsterModelCfg()
		{
		}

		[SerializationConstructor]
		public MonsterModelCfg(int Id, int Type, int ModelType, string Model, int ModelSize, string AtkAction, string SkillAction1, string SkillAction2, string HurtAction, string WalkAction, string DeadAction, string EnterAction, string DeadEffect, string Description, int EnterId, int AtkDistance)
		{
			this.Id = Id;
			this.Type = Type;
			this.ModelType = ModelType;
			this.Model = Model;
			this.ModelSize = ModelSize;
			this.AtkAction = AtkAction;
			this.SkillAction1 = SkillAction1;
			this.SkillAction2 = SkillAction2;
			this.HurtAction = HurtAction;
			this.WalkAction = WalkAction;
			this.DeadAction = DeadAction;
			this.EnterAction = EnterAction;
			this.DeadEffect = DeadEffect;
			this.Description = Description;
			this.EnterId = EnterId;
			this.AtkDistance = AtkDistance;
		}
	}
	public partial class MonsterModelCfgCollection : CfgCollectionBase<MonsterModelCfg>
	{
		private const string dataPath = "MonsterModel";
		private Dictionary<int, MonsterModelCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<MonsterModelCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MonsterModelCfg>();
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

			var config = await DataTableManager.LoadTableAsync<MonsterModelCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MonsterModelCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public MonsterModelCfg GetDataById(int id)
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