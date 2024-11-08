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
	public partial class EquipModelCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "gridType")]
		public int GridType { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "pic")]
		public string Pic { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "battlePic")]
		public string BattlePic { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "attrType")]
		public int AttrType { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "equip")]
		public int Equip { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "holy")]
		public int Holy { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "class")]
		public int Class { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "showLv")]
		public int ShowLv { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "nextId")]
		public List<int> NextId { get; private set; }

		[Key(11)]
		[JsonProperty(PropertyName = "effect")]
		public int Effect { get; private set; }

		[Key(12)]
		[JsonProperty(PropertyName = "coinNum")]
		public int CoinNum { get; private set; }

		[Key(13)]
		[JsonProperty(PropertyName = "compose")]
		public List<int> Compose { get; private set; }

		[Key(14)]
		[JsonProperty(PropertyName = "initialEffect")]
		public List<int> InitialEffect { get; private set; }

		[Key(15)]
		[JsonProperty(PropertyName = "highEffect")]
		public string HighEffect { get; private set; }

		[Key(16)]
		[JsonProperty(PropertyName = "equipSfx")]
		public string EquipSfx { get; private set; }

		[Key(17)]
		[JsonProperty(PropertyName = "equipSfx01")]
		public string EquipSfx01 { get; private set; }

		[Key(18)]
		[JsonProperty(PropertyName = "synthesisAttr")]
		public List<Attribute> SynthesisAttr { get; private set; }

		public EquipModelCfg()
		{
		}

		[SerializationConstructor]
		public EquipModelCfg(int Id, int Type, int GridType, string Pic, string BattlePic, int AttrType, int Equip, int Holy, int Class, int ShowLv, List<int> NextId, int Effect, int CoinNum, List<int> Compose, List<int> InitialEffect, string HighEffect, string EquipSfx, string EquipSfx01, List<Attribute> SynthesisAttr)
		{
			this.Id = Id;
			this.Type = Type;
			this.GridType = GridType;
			this.Pic = Pic;
			this.BattlePic = BattlePic;
			this.AttrType = AttrType;
			this.Equip = Equip;
			this.Holy = Holy;
			this.Class = Class;
			this.ShowLv = ShowLv;
			this.NextId = NextId;
			this.Effect = Effect;
			this.CoinNum = CoinNum;
			this.Compose = Compose;
			this.InitialEffect = InitialEffect;
			this.HighEffect = HighEffect;
			this.EquipSfx = EquipSfx;
			this.EquipSfx01 = EquipSfx01;
			this.SynthesisAttr = SynthesisAttr;
		}
	}
	public partial class EquipModelCfgCollection : CfgCollectionBase<EquipModelCfg>
	{
		private const string dataPath = "EquipModel";
		private Dictionary<int, EquipModelCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<EquipModelCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipModelCfg>();
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

			var config = await DataTableManager.LoadTableAsync<EquipModelCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipModelCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public EquipModelCfg GetDataById(int id)
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