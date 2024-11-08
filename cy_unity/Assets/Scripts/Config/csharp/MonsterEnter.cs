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
	public partial class MonsterEnterCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "moveType")]
		public int MoveType { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "enterType")]
		public int EnterType { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "distance")]
		public int Distance { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "coord")]
		public List<int> Coord { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "endCoord")]
		public List<int> EndCoord { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "coordOffset")]
		public List<int> CoordOffset { get; private set; }

		public MonsterEnterCfg()
		{
		}

		[SerializationConstructor]
		public MonsterEnterCfg(int Id, int MoveType, int EnterType, int Distance, List<int> Coord, List<int> EndCoord, List<int> CoordOffset)
		{
			this.Id = Id;
			this.MoveType = MoveType;
			this.EnterType = EnterType;
			this.Distance = Distance;
			this.Coord = Coord;
			this.EndCoord = EndCoord;
			this.CoordOffset = CoordOffset;
		}
	}
	public partial class MonsterEnterCfgCollection : CfgCollectionBase<MonsterEnterCfg>
	{
		private const string dataPath = "MonsterEnter";
		private Dictionary<int, MonsterEnterCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<MonsterEnterCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MonsterEnterCfg>();
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

			var config = await DataTableManager.LoadTableAsync<MonsterEnterCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MonsterEnterCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public MonsterEnterCfg GetDataById(int id)
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