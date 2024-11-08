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
	public partial class CopyEquipWeightsCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "class")]
		public int Class { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "weight")]
		public int Weight { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "quantity")]
		public int Quantity { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "adv")]
		public int Adv { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "frame")]
		public string Frame { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "sole")]
		public int Sole { get; private set; }

		public CopyEquipWeightsCfg()
		{
		}

		[SerializationConstructor]
		public CopyEquipWeightsCfg(int Id, int Class, int Weight, int Quantity, int Adv, string Frame, int Sole)
		{
			this.Id = Id;
			this.Class = Class;
			this.Weight = Weight;
			this.Quantity = Quantity;
			this.Adv = Adv;
			this.Frame = Frame;
			this.Sole = Sole;
		}
	}
	public partial class CopyEquipWeightsCfgCollection : CfgCollectionBase<CopyEquipWeightsCfg>
	{
		private const string dataPath = "CopyEquipWeights";
		private Dictionary<int, CopyEquipWeightsCfg> idMapItems;
		private Dictionary<int, List<CopyEquipWeightsCfg>> soleMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<CopyEquipWeightsCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, CopyEquipWeightsCfg>();
			soleMapItems = new Dictionary<int, List<CopyEquipWeightsCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(soleMapItems, item.Sole, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<CopyEquipWeightsCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, CopyEquipWeightsCfg>();
			soleMapItems = new Dictionary<int, List<CopyEquipWeightsCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(soleMapItems, item.Sole, item);
			}

			loaded = true;
		}

		public CopyEquipWeightsCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<CopyEquipWeightsCfg> GetDataBySole(int sole)
		{
			Load();

			if(!soleMapItems.TryGetValue(sole, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}