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
	public partial class MailCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "content")]
		public string Content { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "from")]
		public string From { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "hyperlink")]
		public string Hyperlink { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "sendTime")]
		public CYTime SendTime { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "reward")]
		public List<Reward> Reward { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "expireTime")]
		public int ExpireTime { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "tipsKey")]
		public string TipsKey { get; private set; }

		public MailCfg()
		{
		}

		[SerializationConstructor]
		public MailCfg(int Id, int Type, string Name, string Content, string From, string Hyperlink, CYTime SendTime, List<Reward> Reward, int ExpireTime, string TipsKey)
		{
			this.Id = Id;
			this.Type = Type;
			this.Name = Name;
			this.Content = Content;
			this.From = From;
			this.Hyperlink = Hyperlink;
			this.SendTime = SendTime;
			this.Reward = Reward;
			this.ExpireTime = ExpireTime;
			this.TipsKey = TipsKey;
		}
	}
	public partial class MailCfgCollection : CfgCollectionBase<MailCfg>
	{
		private const string dataPath = "Mail";
		private Dictionary<int, MailCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<MailCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MailCfg>();
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

			var config = await DataTableManager.LoadTableAsync<MailCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MailCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public MailCfg GetDataById(int id)
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