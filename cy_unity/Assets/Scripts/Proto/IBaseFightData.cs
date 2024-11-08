using Google.Protobuf.Collections;

namespace DH.Proto
{
    public interface IBaseFightData
    {
        public long Uid { get; set; }
        public long  Seed { get; set; }
        public IBaseStageFightData  Stage { get; set; }
        public FightAttr  Attr { get; set; } 
    }
    
    public partial class  FightData : IBaseFightData
    {
        IBaseStageFightData IBaseFightData.Stage
        {
            get => Stage;
            set => Stage = value as MainFightData;
        }
    }
    public partial class  SFightData : IBaseFightData
    {
        IBaseStageFightData IBaseFightData.Stage
        {
            get => Stage;
            set => Stage = value as SecretFightData;
        }
    }
    

    public interface IBaseStageFightData
    {
        public long Uid { get; set; }
        public int Wave { get; set; }
        public int Money { get; set; }
        public int Lv { get; set; }
        public int Exp { get; set; }
        public int Interest { get; set; }
        public int WaveGotMoney { get; set; }
        public int EquipWaveRefresh { get; set; }
        public int EquipWaveAdRefresh { get; set; }
        public int TalentAdRefresh { get; set; }
        public int EquipAdRefresh { get; set; }
        public MapField<int,int> EquipTalent { get; }
        public MapField<int,int> EffectTalent { get; }
        public RepeatedField<int> Equips { get;}
        public MapField<int,int> EquipSlots { get; }
        public RepeatedField<int> Talents { get; }
        public int TalentRefreshLv { get; set; }
        public int EquipRefreshMoney { get; set; }
        public MapField<string,int> Attr { get; }
        public MapField<int,int> EquipsLv { get; }
        public int ExtraEquipCount { get; set; }
        public MapField<int,int> EquipRatio { get; }
        public int SettleIncrease { get; set; }
        public MapField<int,int> EquipMultiAttr { get; }
        public MapField<long,long> HurtStat { get; }
        public int Kills { get; set; }
        public int SurvivalTime { get; set; }
        public int Energy { get; set; }
        public RepeatedField<int> Wish { get; }
        public MapField<int,int> HeroEquipTalent {get;}
        
        public int TalentBeginRefresh {get;set;}
        public int Dur {get;set;}
        public int HeReviveTimes {get;set;}
        public int Season { get; set; }
        public int Boss { get; set; }
        

    }
    
    public partial class MainFightData : IBaseStageFightData
    {
        public MapField<int,int> HeroEquipTalent {get;}
        public int TalentBeginRefresh { get; set; }
        public int Dur { get; set; }
        public int Season { get; set; }
        public int Boss { get; set; }
    }

    public partial class SecretFightData : IBaseStageFightData
    {
        public int Money { get; set; }
        public int Interest { get; set; }
        public int WaveGotMoney { get; set; }
        public int EquipWaveRefresh { get; set; }
        public int EquipWaveAdRefresh { get; set; }
        public int EquipAdRefresh { get; set; }
        public RepeatedField<int> Equips { get; }
        public int TalentRefreshLv { get; set; }
        public int EquipRefreshMoney { get; set; }
        public int ExtraEquipCount { get; set; }
        public MapField<int, int> EquipRatio { get; set; }
        public int SettleIncrease { get; set; }
        public int SurvivalTime { get; set; }
        public RepeatedField<int> Wish { get; set; }
        public int HeReviveTimes { get => 1;  set {}}
    }
}