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
	public partial class MailLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "content")]
		public string Content { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "from")]
		public string From { get; private set; }

		public MailLanguageCfg()
		{
		}

		[SerializationConstructor]
		public MailLanguageCfg(int Id, string Name, string Content, string From)
		{
			this.Id = Id;
			this.Name = Name;
			this.Content = Content;
			this.From = From;
		}
	}
	public partial class MailLanguageCfgCollection : LocalizeCollectionBase<MailLanguageCfg>
	{
		private const string dataPath = "strings/{0}/MailLanguage";
		private Dictionary<int, MailLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<MailLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MailLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<MailLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MailLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public MailLanguageCfg GetDataById(int id)
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