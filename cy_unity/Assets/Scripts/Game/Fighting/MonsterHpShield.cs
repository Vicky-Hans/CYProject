using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class MonsterHpShield : BaseAssetEntity
    {
        public GameObject blueShieldObj;
        public GameObject blueShieldrokenObj;
        public GameObject yellowShieldObj;
        public GameObject yellowShielrokendObj;
        private SkillTrigger hpShieldTrigger;
        private MonsterController monsterController;
        public int HpShield { get; set; }

        public void Init(MonsterController monster, SkillTrigger trigger)
        {
            monsterController = monster;
            hpShieldTrigger = trigger;
            var shieldPercent = trigger.attrMgr.Calc(AttributeType.HpShield);
            HpShield = Lodash.RoundToInt(shieldPercent * monster.MonsterData.resource.MaxHp);
            blueShieldObj.SetActive(false);
            blueShieldrokenObj.SetActive(false);
            yellowShieldObj.SetActive(false);
            yellowShielrokendObj.SetActive(false);
            if (trigger.trigger.ContainsKey(AttributeName.Dead))
            {
                yellowShieldObj.SetActive(true);
            }
            else
            {
                blueShieldObj.SetActive(true);
            }
        }
        public async void DecHp(int hp)
        {
            HpShield -= hp;
            if (HpShield > 0) return;
            monsterController.MonsterData.resource.Hp += HpShield;
            blueShieldObj.SetActive(false);
            yellowShieldObj.SetActive(false);
            FightingSoundHelper.Instance.PlayShieldBroken();
            if (hpShieldTrigger.trigger.ContainsKey(AttributeName.Dead))
            {
                yellowShielrokendObj.SetActive(true);
                await PauseTask.Delay(1);
                if (yellowShielrokendObj != null) yellowShielrokendObj.SetActive(false);
            }
            else
            {
                blueShieldrokenObj.SetActive(true);
                await PauseTask.Delay(1);
                if (blueShieldrokenObj != null) blueShieldrokenObj.SetActive(false);
            }
            Destroy(this);
        }
    }
}