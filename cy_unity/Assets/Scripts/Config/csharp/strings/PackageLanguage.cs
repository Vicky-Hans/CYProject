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
	public partial class PackageLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "payName")]
		public string PayName { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "value")]
		public string Value { get; private set; }

		public PackageLanguageCfg()
		{
		}

		[SerializationConstructor]
		public PackageLanguageCfg(int Id, string PayName, string Value)
		{
			this.Id = Id;
			this.PayName = PayName;
			this.Value = Value;
		}
	}
	public partial class PackageLanguageCfgCollection : LocalizeCollectionBase<PackageLanguageCfg>
	{
		private const string dataPath = "strings/{0}/PackageLanguage";
		private Dictionary<int, PackageLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<PackageLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, PackageLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<PackageLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, PackageLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public PackageLanguageCfg GetDataById(int id)
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