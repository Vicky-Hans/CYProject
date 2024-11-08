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
	public partial class EndlessStageMonsterLvCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "waveId")]
		public List<int> WaveId { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "HpAdd")]
		public int HpAdd { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "AtkAdd")]
		public int AtkAdd { get; private set; }

		public EndlessStageMonsterLvCfg()
		{
		}

		[SerializationConstructor]
		public EndlessStageMonsterLvCfg(int Id, List<int> WaveId, int HpAdd, int AtkAdd)
		{
			this.Id = Id;
			this.WaveId = WaveId;
			this.HpAdd = HpAdd;
			this.AtkAdd = AtkAdd;
		}
	}
	public partial class EndlessStageMonsterLvCfgCollection : CfgCollectionBase<EndlessStageMonsterLvCfg>
	{
		private const string dataPath = "EndlessStageMonsterLv";
		private Dictionary<int, EndlessStageMonsterLvCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<EndlessStageMonsterLvCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EndlessStageMonsterLvCfg>();
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

			var config = await DataTableManager.LoadTableAsync<EndlessStageMonsterLvCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EndlessStageMonsterLvCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public EndlessStageMonsterLvCfg GetDataById(int id)
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