using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;
using UnityEngine.Pool;

namespace DH.Game
{
    public class WeaponSkillController : ObservableObject
    {
        private UnitBase ownerData;
        private BaseMonoUnit ownerObj;
        private BaseAssetEntity entity;
        public readonly ObservableList<BaseWeapon> weapons = new();
        private Dictionary<long, BaseWeapon> weaponSkillMap = new();
        public Dictionary<long, BaseWeapon> WeaponSkillMap => weaponSkillMap;
        public readonly List<WeaponSkill> pendingAddList = new();
        public readonly List<WeaponSkill> pendingRemoveList = new();
        
        public readonly ObservableList<BaseSkill> skills = new();  // 所有上阵武器的技能实例
        private readonly Dictionary<long, BaseSkill> skillsMap = new();
        public Dictionary<long, BaseSkill> SkillsMap => skillsMap;

        private readonly List<long> tmpWeaponSkillList = new();  // 武器uid列表
        public Dictionary<long, float> FireWeaponAtkDic => PlayerData.FireWeaponAtkDic;

        private bool changingWeapon;
        
        public bool CheckedWaveStart { get; set; }
        public Player PlayerData => ownerData as Player;

        public void Init(UnitBase owner, BaseMonoUnit baseMonoUnit, BaseAssetEntity assetEntity = null)
        {
            ownerData = owner;
            ownerObj = baseMonoUnit;
            entity = assetEntity;
            CreateAllSkillIns();
            var weaponList = GameDataManager.Instance.BackpackWeaponList.ToList();
            CheckMaxHp(weaponList);
        }
        public void CreateWeapon(WeaponSkill weaponSkill)
        {
            var weaponIns = new BaseWeapon();
            skillsMap.TryGetValue(weaponSkill.SkillData.id, out var skillIns);
            weaponIns.Init(skillIns, weaponSkill);
            weapons.Add(weaponIns);
            weaponSkillMap.Add(weaponSkill.WeaponUid, weaponIns);
        }
        public void DestroyWeapon(WeaponSkill weaponSkill)
        {
            var weaponIns = weaponSkillMap[weaponSkill.WeaponUid];
            weapons.Remove(weaponIns);
            weaponSkillMap.Remove(weaponSkill.WeaponUid);
        }
        public float GetWeaponProgress(int weaponUid)
        {
            if (!weaponSkillMap.TryGetValue(weaponUid, out var weaponIns)) return 0f;
            return weaponIns.Progress;
        }
        /// <summary>
        /// 获取等级最高的武器
        /// </summary>
        /// <param name="equipId"></param>
        /// <returns></returns>
        public BaseWeapon GetMaxLevelWeapon(int equipId)
        {
            int maxModelId = -1;
            BaseWeapon tmpWeapon = null;
            foreach (var baseWeapon in weapons)
            {
                if (baseWeapon.weaponData.EquipId != equipId) continue;
                if (baseWeapon.weaponData.WeaponModelId > maxModelId)
                {
                    maxModelId = baseWeapon.weaponData.WeaponModelId;
                    tmpWeapon = baseWeapon;
                }
                tmpWeapon ??= baseWeapon;
            }
            return tmpWeapon;
        }
        /// <summary>
        /// 当武器列表发生变化时调用
        /// </summary>
        /// <param name="list"> 当前武器列表</param>
        public void OnWeaponChanged(List<BackpackWeaponData> list)
        {
            changingWeapon = true;
            // 移除不在列表中的武器
            tmpWeaponSkillList.Clear();
            pendingRemoveList.Clear();
            pendingAddList.Clear();
            foreach (var item in list)
            {
                if(item.EquipId == 9)continue; // 钱袋
                tmpWeaponSkillList.Add(item.Uid);
            }
            foreach (var weaponItem in weapons)
            {
                if (!tmpWeaponSkillList.Contains(weaponItem.weaponData.WeaponUid))
                {
                    if(pendingRemoveList.Find(x => x.WeaponUid == weaponItem.weaponData.WeaponUid) == null)
                    {
                        pendingRemoveList.Add(weaponItem.weaponData);
                    }
                }
            }
            // 添加不在列表中的武器
            foreach (var item in list)
            {
                if (!WeaponSkillMap.Keys.Contains(item.Uid) && item.EquipId != 9)
                {
                    if(pendingAddList.Find(x => x.WeaponUid == item.Uid) == null)
                    {
                        pendingAddList.Add(new WeaponSkill(ownerData, item));
                    }
                }
            }
            changingWeapon = false;
            CheckMaxHp(list);
        }
        private void CheckMaxHp(List<BackpackWeaponData> list)
        {
            PlayerData.CheckMaxHp(list);
        }
        public void OnUpdate(float deltaTime)
        {
            if(changingWeapon)return;
            if (pendingRemoveList.Count > 0)
            {
                foreach (var weaponSkill in pendingRemoveList)
                {
                   DestroyWeapon(weaponSkill);
                }
                pendingRemoveList.Clear();
            }
            foreach (var weapon in weapons)
            {
                weapon.OnUpdate(deltaTime);
            }
            if (pendingAddList.Count > 0)
            {
                foreach (var weaponSkill in pendingAddList)
                {
                    CreateWeapon(weaponSkill);
                }
                pendingAddList.Clear();
            }
            if (!CheckedWaveStart) CheckWaveStart();
        }
        /// <summary>
        /// 火之手套605 附魔
        /// </summary>
        private void CheckWaveStart()
        {
            CheckedWaveStart = true;
            FireWeaponAtkDic.Clear();
            var weapon605Count = 0;
            foreach (var weapon in weapons)
            {
                if(weapon.weaponData.WeaponModelId == 605)
                {
                    weapon605Count++;
                }
            }
            if(weapon605Count < 1)return;
            SkillsMap.TryGetValue(600, out var tmpSkillIns);
            if(tmpSkillIns == null)return;
            var skillData = tmpSkillIns.SkillData;
            var fireWeaponNum = Lodash.RoundToInt(skillData.attrMgr.Calc(AttributeType.FireWeaponNum));
            var fireWeaponAtk = skillData.attrMgr.Calc(AttributeType.FireWeaponAtk);
            var fireWeaponList = ListPool<long>.Get();
            foreach (var weapon in weapons)
            {
                if(weapon.weaponData.GetAtkType() is not (EquipAtkType.Physic or EquipAtkType.Magic))continue;
                if (weapon.weaponData.GetAttrType() == EquipAttrType.Fire)
                {
                    fireWeaponList.Add(weapon.weaponData.WeaponUid);
                }
            }
            var needCount = fireWeaponNum * weapon605Count;
            if(fireWeaponList.Count < needCount)
            {
                needCount = fireWeaponList.Count;
            }
            var tmpList = fireWeaponList.OrderBy(x => Guid.NewGuid()).Take(needCount).ToList();
            foreach (var item in tmpList)
            {
                FireWeaponAtkDic.Add(item, fireWeaponAtk);
            }
            ListPool<long>.Release(fireWeaponList);
        }
        /// <summary>
        /// 创建所有上阵技能实例
        /// </summary>
        private void CreateAllSkillIns()
        {
            var list = ownerData.skill.AutoSkills;
            if(list.Count <= 0)return;
            var root = ownerObj.transform.Find("SkillPoint");
            if(!root)
            {
                DHLog.Error("SkillPoint not found");
                return;
            }

            foreach (var item in list)
            {
                CreateSkill(item, root, SkillType.Auto);
            }
        }
        private void CreateSkill(Skill skillData, Transform root, SkillType type)
        {
            if(skillData.Cfg.SkillEffect is (null or ""))return;
            var skillInstance = CreateSkillInstance(skillData.Cfg.SkillEffect, root);
            var skill = skillInstance.GetComponent<BaseSkill>();
            skill.Init(skillData, ownerData, ownerObj);
            InitWithoutWait(skill, skillData).Forget();
            skills.Add(skill);
            skillsMap.Add(skillData.id, skill);
        }
        private GameObject CreateSkillInstance(string path, Transform root)
        {
            if (entity) entity.LoadAssetSync(path);
            return entity
                ? entity.InstantiateObj(path, root)
                : AssetsManager.InstantiateSync(path, root.position, root.rotation, root);
        }
        private void ReleaseSkillInstance(GameObject obj)
        {
            if(obj is null)return;
            if (entity)
                entity.ReleaseObj(obj);
            else
                AssetsManager.ReleaseInstance(obj);
        }
        public void RemoveAllSkillIns()
        {
            foreach (var skillInstance in skills)
            {
                skillInstance.CancelSkill();
                skillInstance.ClearBulletList();
                skillInstance.Release();
                ReleaseSkillInstance(skillInstance.gameObject);
            }
            skills.Clear();
            skillsMap.Clear();
        }
        public void Release()
        {
            RemoveAllSkillIns();
            pendingAddList.Clear();
            pendingRemoveList.Clear();
            weapons.Clear();
            weaponSkillMap.Clear();
            ownerData = null;
            ownerObj = null;
            entity = null;
        }
        private async UniTaskVoid InitWithoutWait(BaseSkill skill, Skill skillData)
        {
            await skill.Init(skillData, ownerData, ownerObj);
        }
    }
}