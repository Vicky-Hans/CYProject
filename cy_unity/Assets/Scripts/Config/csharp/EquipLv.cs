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
	public partial class EquipLvCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "consume1")]
		public int Consume1 { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "consume2")]
		public int Consume2 { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "consume3")]
		public int Consume3 { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "consume4")]
		public int Consume4 { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "consumeCoin1")]
		public List<Reward> ConsumeCoin1 { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "consumeCoin2")]
		public List<Reward> ConsumeCoin2 { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "consumeCoin3")]
		public List<Reward> ConsumeCoin3 { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "consumeCoin4")]
		public List<Reward> ConsumeCoin4 { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "attrAdd")]
		public List<WeaponAttrAdd> AttrAdd { get; private set; }

		public EquipLvCfg()
		{
		}

		[SerializationConstructor]
		public EquipLvCfg(int Id, int Consume1, int Consume2, int Consume3, int Consume4, List<Reward> ConsumeCoin1, List<Reward> ConsumeCoin2, List<Reward> ConsumeCoin3, List<Reward> ConsumeCoin4, List<WeaponAttrAdd> AttrAdd)
		{
			this.Id = Id;
			this.Consume1 = Consume1;
			this.Consume2 = Consume2;
			this.Consume3 = Consume3;
			this.Consume4 = Consume4;
			this.ConsumeCoin1 = ConsumeCoin1;
			this.ConsumeCoin2 = ConsumeCoin2;
			this.ConsumeCoin3 = ConsumeCoin3;
			this.ConsumeCoin4 = ConsumeCoin4;
			this.AttrAdd = AttrAdd;
		}
	}
	public partial class EquipLvCfgCollection : CfgCollectionBase<EquipLvCfg>
	{
		private const string dataPath = "EquipLv";
		private Dictionary<int, EquipLvCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<EquipLvCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipLvCfg>();
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

			var config = await DataTableManager.LoadTableAsync<EquipLvCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipLvCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public EquipLvCfg GetDataById(int id)
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