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
	public partial class GuideCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "isShowPerson")]
		public int IsShowPerson { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "isShowMask")]
		public int IsShowMask { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "isPlayAction")]
		public int IsPlayAction { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "isShowTouch")]
		public int IsShowTouch { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "isShowDesc")]
		public int IsShowDesc { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "descPosition")]
		public int DescPosition { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "reward")]
		public List<Reward> Reward { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "firstId")]
		public int FirstId { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "nextId")]
		public int NextId { get; private set; }

		[Key(11)]
		[JsonProperty(PropertyName = "triggerStr")]
		public Dictionary<string, string> TriggerStr { get; private set; }

		[Key(12)]
		[JsonProperty(PropertyName = "completeStr")]
		public Dictionary<string, string> CompleteStr { get; private set; }

		public GuideCfg()
		{
		}

		[SerializationConstructor]
		public GuideCfg(int Id, int Type, int IsShowPerson, int IsShowMask, int IsPlayAction, int IsShowTouch, int IsShowDesc, int DescPosition, List<Reward> Reward, int FirstId, int NextId, Dictionary<string, string> TriggerStr, Dictionary<string, string> CompleteStr)
		{
			this.Id = Id;
			this.Type = Type;
			this.IsShowPerson = IsShowPerson;
			this.IsShowMask = IsShowMask;
			this.IsPlayAction = IsPlayAction;
			this.IsShowTouch = IsShowTouch;
			this.IsShowDesc = IsShowDesc;
			this.DescPosition = DescPosition;
			this.Reward = Reward;
			this.FirstId = FirstId;
			this.NextId = NextId;
			this.TriggerStr = TriggerStr;
			this.CompleteStr = CompleteStr;
		}
	}
	public partial class GuideCfgCollection : CfgCollectionBase<GuideCfg>
	{
		private const string dataPath = "Guide";
		private Dictionary<int, GuideCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<GuideCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, GuideCfg>();
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

			var config = await DataTableManager.LoadTableAsync<GuideCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, GuideCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public GuideCfg GetDataById(int id)
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