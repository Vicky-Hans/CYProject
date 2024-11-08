using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using DH.Config;
using DHFramework;
using UnityEngine.Scripting;

namespace DH.Launch
{
    [Preserve]
    public class StartupLanguageCfg
    {
        [JsonProperty(PropertyName = "id")] public string Id { get; private set; }
        [JsonProperty(PropertyName = "name")] public string Name { get; private set; }

        public StartupLanguageCfg()
        {
            
        }
        
        [Preserve]
        [JsonConstructor]
        public StartupLanguageCfg(string Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
        }
    }

    public class StartupLanguageCfgCollection : LocalizeCollectionBase<StartupLanguageCfg>
    {
        private Dictionary<string, StartupLanguageCfg> idMapItems;

        public override void Load()
        {
            if (loaded)
            {
                return;
            }

            var dataPath = StartupEntry.Instance.StartupConfig.StartupLanguageConfigPath;
            var lastLoader = DataTableManager.GetLoadAdapter();
            var lastUseBson = DataTableManager.GetUseBson();
            
            DataTableManager.SetLoadAdapter(new ConfigLoader());
            DataTableManager.SetUseBson(true);
            try
            {
                var config = DataTableManager.LoadLocalizeTable<StartupLanguageCfg>(dataPath);
                dataItems = config.Data;

                idMapItems = new Dictionary<string, StartupLanguageCfg>();
                foreach (var item in dataItems)
                {
                    AddItem(idMapItems, item.Id, item);
                }
            }
            catch (Exception e)
            {
                DHLog.Error($"StartupLanguageCfgCollection exception:\n{e.Message}\n{e.StackTrace}");
            }
            
            DataTableManager.SetLoadAdapter(lastLoader);
            DataTableManager.SetUseBson(lastUseBson);
            loaded = true;
        }

        public override async UniTask LoadAsync()
        {
            if (loaded)
            {
                return;
            }

            var dataPath = StartupEntry.Instance.StartupConfig.StartupLanguageConfigPath;
            var lastLoader = DataTableManager.GetLoadAdapter();
            var lastUseBson = DataTableManager.GetUseBson();
            
            DataTableManager.SetLoadAdapter(new ConfigLoader());
            DataTableManager.SetUseBson(true);
            try
            {
                var config = await DataTableManager.LoadLocalizeTableAsync<StartupLanguageCfg>(dataPath);
                dataItems = config.Data;

                idMapItems = new Dictionary<string, StartupLanguageCfg>();
                foreach (var item in dataItems)
                {
                    AddItem(idMapItems, item.Id, item);
                }
            }
            catch (Exception e)
            {
                DHLog.Error($"StartupLanguageCfgCollection exception:\n{e.Message}\n{e.StackTrace}");
            }
            
            DataTableManager.SetLoadAdapter(lastLoader);
            DataTableManager.SetUseBson(lastUseBson);
            loaded = true;
        }

        public string GetDataById(string id)
        {
            Load();

            if (!idMapItems.TryGetValue(id, out var item))
            {
                return "";
            }

            return item.Name;
        }
    }
}
