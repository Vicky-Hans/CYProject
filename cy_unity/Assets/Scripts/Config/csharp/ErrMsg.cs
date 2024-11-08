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
	public partial class ErrMsgCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "msg")]
		public string Msg { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "LanguageId")]
		public string LanguageId { get; private set; }

		public ErrMsgCfg()
		{
		}

		[SerializationConstructor]
		public ErrMsgCfg(int Id, string Msg, string LanguageId)
		{
			this.Id = Id;
			this.Msg = Msg;
			this.LanguageId = LanguageId;
		}
	}
	public partial class ErrMsgCfgCollection : CfgCollectionBase<ErrMsgCfg>
	{
		private const string dataPath = "ErrMsg";
		private Dictionary<int, ErrMsgCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<ErrMsgCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ErrMsgCfg>();
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

			var config = await DataTableManager.LoadTableAsync<ErrMsgCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ErrMsgCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public ErrMsgCfg GetDataById(int id)
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