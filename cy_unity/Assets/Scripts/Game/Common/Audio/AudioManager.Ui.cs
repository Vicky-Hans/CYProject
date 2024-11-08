namespace DH.Game
{
    public enum AudioType
    {
        None,
        ButtonClick,
        ButtonClose,
        OpenUi,
        CloseUi,
        RewardTitle,
        WrongTips,
        GameWin,
        GameFail,
        CoutDown,
        BossComing,
        WeaponRefresh,
        ExchangeChapter,
        FlyGameCoin,
        LuckyOnce,
        LuckyFive,
        LuckyFlyNum,
        ClothesLevelUp,
    }
    public partial class AudioManager
    {
        private const string ButtonClick = "SFX_UI/ui_common_button";
        private const string ButtonClose = "SFX_UI/ui_common_close";
        private const string OpenUi = "SFX_UI/ui_common_openUi";
        private const string CloseUi = "SFX_UI/ui_common_closeUi";
        private const string RewardTitle = "SFX_UI/ui_common_rewardGot";
        private const string WrongTips = "SFX_UI/ui_common_error";
        private const string GameWin = "SFX_UI/ui_game_victory";
        private const string GameFail = "SFX_UI/ui_game_fail";
        private const string CoutDown = "SFX_UI/ui_game_countdown";
        private const string BossComing = "SFX_UI/ui_battle_bossComing";
        private const string WeaponRefresh = "SFX_UI/ui_game_refresh";
        private const string LevelUp = "SFX_UI/ui_common_levelUp";
        private const string ExchangeChapter = "SFX_UI/ui_common_nextStage";
        private const string FlyGameCoin = "SFX_UI/ui_game_itemFly";
        private const string EquipLevelUp = "SFX_UI/ui_common_equiplevelUp";
        private const string LuckyOnce = "SFX_UI/ui_activity_luckyWheel_randomReward01";
        private const string LuckyFive = "SFX_UI/ui_activity_luckyWheel_randomReward02";
        private const string LuckyFlyNum = "SFX_UI/ui_activity_luckyWheel_numFly";
        private const string ClothesLevelUp = "SFX_UI/ui_clothing_levelUp";
        public void Play(AudioType audioType)
        {
            switch (audioType)
            {
                case AudioType.ButtonClick: PlayAudio(ButtonClick); break;
                case AudioType.ButtonClose: PlayAudio(ButtonClose); break;
                case AudioType.OpenUi: PlayAudio(OpenUi); break;
                case AudioType.CloseUi: PlayAudio(CloseUi); break;
                case AudioType.RewardTitle: PlayAudio(RewardTitle); break;
                case AudioType.WrongTips: PlayAudio(WrongTips); break;
                case AudioType.GameWin: PlayAudio(GameWin); break;
                case AudioType.GameFail: PlayAudio(GameFail); break;
                case AudioType.CoutDown: PlayAudio(CoutDown); break;
                case AudioType.BossComing: PlayAudio(BossComing); break;
                case AudioType.WeaponRefresh: PlayAudio(WeaponRefresh); break;
                case AudioType.ExchangeChapter: PlayAudio(ExchangeChapter); break;
                case AudioType.FlyGameCoin: PlayAudio(FlyGameCoin); break;
                case AudioType.LuckyOnce: PlayAudio(LuckyOnce); break;
                case AudioType.LuckyFive: PlayAudio(LuckyFive); break;
                case AudioType.LuckyFlyNum: PlayAudio(LuckyFlyNum); break;
                case AudioType.ClothesLevelUp: PlayAudio(ClothesLevelUp); break;
            }
        }

        public void PlayButtonClick()
        {
            PlayAudio(ButtonClick);
        }

        public void PlayButtonClose()
        {
            PlayAudio(ButtonClose);
        }

        public void PlayOpenUi()
        {
            PlayAudio(OpenUi);
        }

        public void PlayCloseUi()
        {
            PlayAudio(CloseUi);
        }

        public void PlayRewardTitle()
        {
            PlayAudio(RewardTitle);
        }
        
        public void PlayWrongTips()
        {
            PlayAudio(WrongTips);
        }
        
        public void PlayLevelUp()
        {
            PlayAudio(LevelUp);
        }
        
        public void PlayEquipLevelUp()
        {
            PlayAudio(EquipLevelUp);
        }
    }
}