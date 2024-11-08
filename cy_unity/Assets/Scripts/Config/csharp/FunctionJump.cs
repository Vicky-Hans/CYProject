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
	public partial class FunctionJumpCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "key")]
		public string Key { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "functionId")]
		public int FunctionId { get; private set; }

		public FunctionJumpCfg()
		{
		}

		[SerializationConstructor]
		public FunctionJumpCfg(int Id, string Key, int FunctionId)
		{
			this.Id = Id;
			this.Key = Key;
			this.FunctionId = FunctionId;
		}
	}
	public partial class FunctionJumpCfgCollection : CfgCollectionBase<FunctionJumpCfg>
	{
		private const string dataPath = "FunctionJump";
		private Dictionary<int, FunctionJumpCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<FunctionJumpCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, FunctionJumpCfg>();
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

			var config = await DataTableManager.LoadTableAsync<FunctionJumpCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, FunctionJumpCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public FunctionJumpCfg GetDataById(int id)
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