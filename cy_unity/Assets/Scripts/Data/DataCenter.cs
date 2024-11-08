using System.Collections.Generic;
using System.Reflection;
using DH.Proto;
using Google.Protobuf;

namespace DH.Data
{
    public static class DataCenter
    {
        private static List<BaseData> dataHubList = new List<BaseData>();
        private static bool created = false;

        public static CharacterData charcaterData;
        public static LivesData livesData;
        public static ItemsData itemsData;
        public static ShopData shopData;
        public static MailData maildata;
        public static MainStageData mainStageData; 
        public static Userinfo userinfo;
        public static EquipData equipData;
        public static MonthCardData monthCardData;
        public static NewBieData newBieData;
        public static RoleData roleData;
        public static EndlessData endlessData;
        public static AllPassportData allPassportData;
        public static DailyPackData dailyPackData;
        public static LuckyDrawData luckyDrawData;
        public static TriggerGiftData triggerGiftData;
        public static DailyFightData dailyFightData;
        public static MagicDrawData magicDrawData;
        public static SecretData secretData; 
        public static ClothesData clothesData;
        public static CollegeData collegeData;
        public static ChapterFundData chapterFundData;
        public static LuckyTravelData luckyTravelData;
        public static GiftPackData giftPackData;
        public static MagicAgeData magicAgeData;
        public static LuckyEggData luckyEggData;
        public static AutumnSpecialData autumnSpecialData;
        public static MagicBingoData mgicBingoData;
        public static void Init(RspSync data)
        {
            if (created)
            {
                Clear();
                OnHandleSyncData(data);
            }
            else
            {
                CreateData(data);
            }

            InitData();
        }

        private static void CreateData(RspSync data)
        {
            if (created) return;
            charcaterData = new CharacterData(data.Character);
            dataHubList.Add(charcaterData);
            livesData = new LivesData();
            if (data.Lives != null)
            {
                livesData.MergeFrom(data.Lives, true);
                dataHubList.Add(livesData);
            }

            monthCardData = new MonthCardData();
            if (data.MonthCard != null)
            {           
                monthCardData.MergeFrom(data.MonthCard,true);
                dataHubList.Add(monthCardData);
            }

            newBieData = new NewBieData();
            if (data.FirstCharge != null)
            {           
                newBieData.MergeFrom(data.FirstCharge,true);
                dataHubList.Add(newBieData);
            }

            roleData = new RoleData();
            if (data.Hero != null)
            {           
                roleData.MergeFrom(data.Hero,true);
                dataHubList.Add(roleData);
            }
            itemsData = new ItemsData();
            if (data.ItemBag != null)
            {
                itemsData.MergeFrom(data.ItemBag, true);
                dataHubList.Add(itemsData);
            }

            ServerTime.Instance.Init(data.ServerTime);
            
            userinfo = new Userinfo();
            dataHubList.Add(userinfo);

            if (data.Shop != null)
            {
                shopData = new ShopData(data.Shop);
            }
            else
            {
                shopData = new ShopData();
            }
            dataHubList.Add(shopData);
            maildata = new MailData();
            if (data.Mails != null)
            {
                maildata.MergeFrom(data.Mails, true);
                dataHubList.Add(maildata);
            }
           // mainStageData = new MainStageData();
            if (data.MainStage != null)
            {
                // var fieldInfo = typeof(MainStageData).GetField("hangup", BindingFlags.NonPublic | BindingFlags.Instance);
                // var HangupData = new HangupData();
                // fieldInfo.SetValue(mainStageData, HangupData);

                // mainStageData.MergeFrom(data.MainStage, true);
                mainStageData = new MainStageData(data.MainStage);
                dataHubList.Add(mainStageData);
            }
            else
            {
                mainStageData = new MainStageData();
                dataHubList.Add(mainStageData);
            }
            
            equipData = new EquipData();
            if (data.Equip != null)
            {
                equipData.MergeFrom(data.Equip, true);
                dataHubList.Add(equipData);
            }
            endlessData = new EndlessData();
            if (data.Endless != null)
            {
                endlessData.MergeFrom(data.Endless, true);
                dataHubList.Add(endlessData);
            }
            
            allPassportData = new AllPassportData();
            if(data.Passport != null)
            {
                allPassportData.MergeFrom(data.Passport, true);
                dataHubList.Add(allPassportData);
            }
            dailyPackData = new DailyPackData();
            if(data.DailyPack != null)
            {
                dailyPackData.MergeFrom(data.DailyPack, true);
                dataHubList.Add(dailyPackData);
            }
            
            luckyDrawData = new LuckyDrawData();
            if (data.LuckyDraw != null)
            {
                luckyDrawData.MergeFrom(data.LuckyDraw, true);
                dataHubList.Add(luckyDrawData);
            }

            triggerGiftData = new TriggerGiftData();
            if (data.TriggerGift != null)
            {
                triggerGiftData.MergeFrom(data.TriggerGift, true);
                dataHubList.Add(triggerGiftData);
            }
            dailyFightData = new DailyFightData();
            if (data.DailyFight != null)
            {
                dailyFightData.MergeFrom(data.DailyFight, true);
                dataHubList.Add(dailyFightData);
            }
            magicDrawData = new MagicDrawData();
            if (data.MagicDraw != null)
            {
                magicDrawData.MergeFrom(data.MagicDraw, true);
                dataHubList.Add(magicDrawData);
            }
            secretData = new SecretData();
            if (data.Secret != null)
            {
                secretData.MergeFrom(data.Secret, true);
                dataHubList.Add(secretData);
            }

            clothesData = new ClothesData();
            if (data.HeroEquip != null)
            {
                clothesData.MergeFrom(data.HeroEquip, true);
                dataHubList.Add(clothesData);
            }
            collegeData = new CollegeData();
            if (data.School != null)
            {
                collegeData.MergeFrom(data.School, true);
                dataHubList.Add(collegeData);
            }
            
            chapterFundData = new ChapterFundData();
            if (data.ChapterFund != null)
            {
                chapterFundData.MergeFrom(data.ChapterFund, true);
                dataHubList.Add(chapterFundData);
            }

            luckyTravelData = new LuckyTravelData();
            if (data.LuckyTrip != null)
            {
                luckyTravelData.MergeFrom(data.LuckyTrip, true);
                dataHubList.Add(luckyTravelData);
            }

            giftPackData = new GiftPackData();
            if (data.GiftPack != null)
            {
                giftPackData.MergeFrom(data.GiftPack, true);
                dataHubList.Add(giftPackData);
            }
            magicAgeData = new MagicAgeData();
            if (data.MagicAge != null)
            {
                magicAgeData.MergeFrom(data.MagicAge, true);
                dataHubList.Add(chapterFundData);
            }
            luckyEggData = new LuckyEggData();
            if (data.LuckyEgg != null)
            {
                luckyEggData.MergeFrom(data.LuckyEgg, true);
                dataHubList.Add(chapterFundData);
            }
            autumnSpecialData = new AutumnSpecialData();
            if (data.Autumn != null)
            {
                autumnSpecialData.MergeFrom(data.Autumn, true);
                dataHubList.Add(chapterFundData);
            }
            mgicBingoData = new MagicBingoData();
            if (data.Bingo != null)
            {
                mgicBingoData.MergeFrom(data.Bingo, true);
                dataHubList.Add(chapterFundData);
            }
            created = true;
        }

        private static void OnHandleSyncData(RspSync data)
        {
            if (data.Character != null)
            {
                charcaterData.MergeFrom(data.Character, true);
            }

            if (data.Lives != null)
            {
                livesData.MergeFrom(data.Lives, true);
            }

            if (data.ItemBag != null)
            {
                itemsData.MergeFrom(data.ItemBag, true);
            }

            if (data.MonthCard != null)
            {
                monthCardData.MergeFrom(data.MonthCard, true);
            }

            if (data.FirstCharge != null)
            {
                newBieData.MergeFrom(data.FirstCharge, true);
            }

            if (data.Hero != null)
            {
                roleData.MergeFrom(data.Hero, true);
            }

            if (data.Shop != null)
            {
                shopData.MergeFrom(data.Shop, true);
            }

            if (data.Mails != null)
            {
                maildata.MergeFrom(data.Mails, true);
            }

            if (data.MainStage != null)
            {
                mainStageData.MergeFrom(data.MainStage, true);
            }

            if (data.Equip != null)
            {
                equipData.MergeFrom(data.Equip, true);
            }

            if (data.DailyPack != null)
            {
                dailyPackData.MergeFrom(data.DailyPack, true);
            }
            
            if (data.Endless != null)
            {
                endlessData.MergeFrom(data.Endless, true);

                if (data.Passport != null)
                {
                    allPassportData.MergeFrom(data.Passport, true);
                }
            }
            if (data.LuckyDraw != null)
            {
                luckyDrawData.MergeFrom(data.LuckyDraw, true);
            }
            
            if (data.TriggerGift != null)
            {
                triggerGiftData.MergeFrom(data.TriggerGift, true);
            }
            if (data.DailyFight != null)
            {
                dailyFightData.MergeFrom(data.DailyFight, true);
            }
            if (data.MagicDraw != null)
            {
                magicDrawData.MergeFrom(data.MagicDraw, true);
            }
            if (data.Secret != null)
            {
                secretData.MergeFrom(data.Secret, true);
            }
            if (data.HeroEquip != null)
            {
                clothesData.MergeFrom(data.HeroEquip, true);
            }
            
            if (data.School != null)
            {
                collegeData.MergeFrom(data.School, true);
            }
            
            if (data.ChapterFund != null)
            {
                chapterFundData.MergeFrom(data.ChapterFund, true);
            }
            
            if (data.LuckyTrip != null)
            {
                luckyTravelData.MergeFrom(data.LuckyTrip, true);
            }
            
            if (data.GiftPack != null)
            {
                giftPackData.MergeFrom(data.GiftPack, true);
            }
            if (data.MagicAge != null)
            {
                magicAgeData.MergeFrom(data.MagicAge, true);
            }
            if (data.LuckyEgg != null)
            {
                luckyEggData.MergeFrom(data.LuckyEgg, true);
            }
            if (data.Autumn != null)
            {
                autumnSpecialData.MergeFrom(data.Autumn, true);
            }
            if (data.Bingo != null)
            {
                mgicBingoData.MergeFrom(data.Bingo, true);
            }
        }
        private static void InitData()
        {
            for (var i = 0; i < dataHubList.Count; i++)
            {
                dataHubList[i].Init();
            }
        }
        public static void Clear()
        {
            ClearDataHub();
        }
        private static void ClearDataHub()
        {
            for (int i = 0; i < dataHubList.Count; i++)
            {
                dataHubList[i].Clear();
            }
        }
    }
}