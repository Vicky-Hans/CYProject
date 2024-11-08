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
	public partial class HeroMainCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "attr")]
		public List<WeaponAttr> Attr { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "qlt")]
		public int Qlt { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "MaxStar")]
		public int MaxStar { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "card")]
		public string Card { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "headIcon")]
		public string HeadIcon { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "model")]
		public string Model { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "mainSkill")]
		public int MainSkill { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "mainSkillSfx")]
		public string MainSkillSfx { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "energy")]
		public int Energy { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "starSkillGroup")]
		public List<int> StarSkillGroup { get; private set; }

		[Key(11)]
		[JsonProperty(PropertyName = "levelSkillGroup")]
		public List<int> LevelSkillGroup { get; private set; }

		[Key(12)]
		[JsonProperty(PropertyName = "itemId")]
		public int ItemId { get; private set; }

		[Key(13)]
		[JsonProperty(PropertyName = "unlock")]
		public int Unlock { get; private set; }

		[Key(14)]
		[JsonProperty(PropertyName = "unlockItemNum")]
		public int UnlockItemNum { get; private set; }

		[Key(15)]
		[JsonProperty(PropertyName = "gemCost")]
		public List<Reward> GemCost { get; private set; }

		[Key(16)]
		[JsonProperty(PropertyName = "packageId")]
		public int PackageId { get; private set; }

		[Key(17)]
		[JsonProperty(PropertyName = "headId")]
		public List<Reward> HeadId { get; private set; }

		public HeroMainCfg()
		{
		}

		[SerializationConstructor]
		public HeroMainCfg(int Id, List<WeaponAttr> Attr, int Qlt, int MaxStar, string Card, string HeadIcon, string Model, int MainSkill, string MainSkillSfx, int Energy, List<int> StarSkillGroup, List<int> LevelSkillGroup, int ItemId, int Unlock, int UnlockItemNum, List<Reward> GemCost, int PackageId, List<Reward> HeadId)
		{
			this.Id = Id;
			this.Attr = Attr;
			this.Qlt = Qlt;
			this.MaxStar = MaxStar;
			this.Card = Card;
			this.HeadIcon = HeadIcon;
			this.Model = Model;
			this.MainSkill = MainSkill;
			this.MainSkillSfx = MainSkillSfx;
			this.Energy = Energy;
			this.StarSkillGroup = StarSkillGroup;
			this.LevelSkillGroup = LevelSkillGroup;
			this.ItemId = ItemId;
			this.Unlock = Unlock;
			this.UnlockItemNum = UnlockItemNum;
			this.GemCost = GemCost;
			this.PackageId = PackageId;
			this.HeadId = HeadId;
		}
	}
	public partial class HeroMainCfgCollection : CfgCollectionBase<HeroMainCfg>
	{
		private const string dataPath = "HeroMain";
		private Dictionary<int, HeroMainCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<HeroMainCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroMainCfg>();
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

			var config = await DataTableManager.LoadTableAsync<HeroMainCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroMainCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public HeroMainCfg GetDataById(int id)
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