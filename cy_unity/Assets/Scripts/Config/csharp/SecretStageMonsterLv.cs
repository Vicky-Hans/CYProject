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
	public partial class SecretStageMonsterLvCfg
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

		public SecretStageMonsterLvCfg()
		{
		}

		[SerializationConstructor]
		public SecretStageMonsterLvCfg(int Id, List<int> WaveId, int HpAdd, int AtkAdd)
		{
			this.Id = Id;
			this.WaveId = WaveId;
			this.HpAdd = HpAdd;
			this.AtkAdd = AtkAdd;
		}
	}
	public partial class SecretStageMonsterLvCfgCollection : CfgCollectionBase<SecretStageMonsterLvCfg>
	{
		private const string dataPath = "SecretStageMonsterLv";
		private Dictionary<int, SecretStageMonsterLvCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<SecretStageMonsterLvCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, SecretStageMonsterLvCfg>();
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

			var config = await DataTableManager.LoadTableAsync<SecretStageMonsterLvCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, SecretStageMonsterLvCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public SecretStageMonsterLvCfg GetDataById(int id)
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