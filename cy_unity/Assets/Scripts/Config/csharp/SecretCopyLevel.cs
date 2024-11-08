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
	public partial class SecretCopyLevelCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "level")]
		public int Level { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "exp")]
		public int Exp { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "total_exp_1")]
		public int Total_exp_1 { get; private set; }

		public SecretCopyLevelCfg()
		{
		}

		[SerializationConstructor]
		public SecretCopyLevelCfg(int Id, int Level, int Exp, int Total_exp_1)
		{
			this.Id = Id;
			this.Level = Level;
			this.Exp = Exp;
			this.Total_exp_1 = Total_exp_1;
		}
	}
	public partial class SecretCopyLevelCfgCollection : CfgCollectionBase<SecretCopyLevelCfg>
	{
		private const string dataPath = "SecretCopyLevel";
		private Dictionary<int, SecretCopyLevelCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<SecretCopyLevelCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, SecretCopyLevelCfg>();
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

			var config = await DataTableManager.LoadTableAsync<SecretCopyLevelCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, SecretCopyLevelCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public SecretCopyLevelCfg GetDataById(int id)
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