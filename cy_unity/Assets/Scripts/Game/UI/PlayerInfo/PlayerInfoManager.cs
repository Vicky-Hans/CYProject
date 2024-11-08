using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.UIFramework.Observables;
using DHFramework;


public class PlayerInfoManager : ObservableSingleton<PlayerInfoManager>
{
   private readonly string SecondDayTag = "SecondDay"; 
   public  readonly ObservableDictionary<string, string> supportLanguage = new ObservableDictionary<string, string>()
   {
      {"en","English"},
      {"cn","简体中文"},
      {"zh","繁体中文"},
      {"de","Deutsch"},
      {"fr","Français"},
      {"es","Español"},
      {"pt","Português"},
      {"ko", "한국어"},
      {"ja","日本語"}
   };

   /// <summary>
   /// 第二天
   /// </summary>
   public bool SecondDay;
   
   private DigestData data = DataCenter.charcaterData.Digest;
   
   public void Init()
   {
      DataCenter.charcaterData.Digest.PropertyChanged -= OnPropertyChanged;
      DataCenter.charcaterData.Digest.PropertyChanged += OnPropertyChanged;
      
      var midnightTimestamp = ServerTime.Instance.SecondDaySeconds() + 1;
      StartSchedule(midnightTimestamp);
   }
   
   private void StartSchedule(long offset)
   {
      StopShedule();
      GlobalSchedule.Instance.AddScheduler(SecondDayFunc, 86400, offset, -1, SecondDayTag);
   }
        
   private void StopShedule()
   {
      GlobalSchedule.Instance.RemoveSchedulerByTag(SecondDayTag);
   }
   private void SecondDayFunc()
   {
      RaisePropertyChanged(nameof(SecondDay));
   }

    void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
   {
      if (e.PropertyName == nameof(data.AddExp))
      {
         GetCanUpGradeLevel(data.AddExp);
      }

   }
    
   /// <summary>
   /// 这里检查当前要加的经验值是否可以升级
   /// </summary>
   /// <returns> -1表示没有配置。 0 表示不能升级 大于0的表示可以升的等级数 </returns>
   public async void  GetCanUpGradeLevel(long exp)
   {
      
      var cfg = ConfigCenter.ProLevelCfgColl.GetDataById(data.Lv);
      if (cfg == null) return ;

      long tempExp = data.Exp + exp;
      int retLv = 0;
      while (tempExp >= cfg.Exp)
      {
         retLv += 1;
         tempExp -= cfg.Exp;
         cfg = ConfigCenter.ProLevelCfgColl.GetDataById(data.Lv + retLv);

         if (cfg == null) break;
      }

      var curLv = DataCenter.charcaterData.Digest.Lv;
      var newLevel = curLv + retLv;
      if (retLv > 0)
      {
         await UniTask.Delay(200);
         UpgradeViewModel vm = new UpgradeViewModel(curLv, newLevel);
         UIManager.Instance.OpenDialog<UpgradeView>(vm).Forget();
      }
   }
}
