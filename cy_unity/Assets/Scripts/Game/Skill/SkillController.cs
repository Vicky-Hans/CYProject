using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;

namespace DH.Game
{
    /// <summary>
    /// 逻辑层技能管理
    /// </summary>
    public class SkillController : ObservableObject
    {
        private BaseMonoUnit ownerObj;
        private UnitBase ownerData;
        private BaseAssetEntity entity;
        public readonly ObservableList<BaseSkill> skills = new();
        public readonly ObservableList<BaseSkill> activeSkills = new();
        private readonly Dictionary<long, BaseSkill> skillsMap = new();
        public Dictionary<long, BaseSkill> SkillsMap => skillsMap;
        /// <summary>
        /// 正在释放中的技能，等待技能释放完成后释放资源
        /// </summary>
        private readonly List<BaseSkill> pendingRemoveSkill = new();
        private NotifyCollectionChangedEventHandler handler;
        public BaseActiveSkill MainActiveSkill => activeSkills.Count > 0 ? activeSkills[0] as BaseActiveSkill : null;
        public void Init(UnitBase owner, BaseMonoUnit monoBehaviour, BaseAssetEntity assetEntity = null)
        {
            handler = AutoSkillsOnCollectionChanged;
            ownerData = owner;
            ownerObj = monoBehaviour;
            entity = assetEntity;
            CreateSkills();
            ownerData.skill.AutoSkills.CollectionChanged += handler;
        }
        /// <summary>
        /// 释放所有技能对象和资源，用于怪物死亡时及时回收释放的技能
        /// </summary>
        private void ReleaseAllSkill()
        {
            foreach (var skill in skills)
            {
                skill.CancelSkill();
                skill.Release();
                ReleaseSkillInstance(skill.gameObject);
            }
            skills.Clear();
            foreach (var skill in pendingRemoveSkill)
            {
                skill.CancelSkill();
                skill.Release();
                ReleaseSkillInstance(skill.gameObject);
            }
            pendingRemoveSkill.Clear();
            foreach (var skill in activeSkills)
            {
                skill.CancelSkill();
                skill.Release();
                ReleaseSkillInstance(skill.gameObject);
            }
            activeSkills.Clear();
            skillsMap.Clear();
        }
        public void Release()
        {
            ReleaseAllSkill();
            ownerData.skill.AutoSkills.CollectionChanged -= handler;
            ownerData = null;
            ownerObj = null;
            entity = null;
            handler = null;
        }
        private void AutoSkillsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    CreateSkills(e.NewItems, SkillType.Auto);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveSkills(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var oldSkillId = (e.OldItems[0] as Skill).id;
                    var newSkill = e.NewItems[0] as Skill;
                    var root = ownerObj.transform.Find("SkillPoint");
                    ReplaceSkill(oldSkillId, newSkill, root);
                    break;
            }
        }
        public void OnUpdate(float deltaTime)
        {
            foreach (var skill in activeSkills) skill.OnUpdate(deltaTime);
            foreach (var skill in skills) skill.OnUpdate(deltaTime);
            foreach (var skill in pendingRemoveSkill) skill.OnUpdate(deltaTime);
            if (pendingRemoveSkill.Count > 0) pendingRemoveSkill.RemoveAll(x => x.CurrentState == BaseSkill.State.Cooldown || x.CurrentState == BaseSkill.State.Ready);
        }
        private void CreateSkills(SkillType type = SkillType.Auto)
        {
            var list = ownerData.skill.AutoSkills;
            if(list.Count <= 0)return;
            var root = ownerObj.transform.Find("SkillPoint");
            if (!root)
            {
                Debug.LogError("必须包含名字为SkillPoint的技能挂载点");
                return;
            }
            foreach (var item in list)
            {
                CreateSkill(item, root, type);
            }
            RaisePropertyChanged(nameof(MainActiveSkill));
        }
        private void RemoveSkills(IList skls)
        {
            foreach (var item in skls)
            {
                if (item is not Skill skill) continue;
                if (!skillsMap.TryGetValue(skill.id, out var skillInstance)) continue;
                skillInstance.Release();
                skills.Remove(skillInstance);
                skillsMap.Remove(skill.id);
                ReleaseSkillInstance(skillInstance.gameObject);
            }
        }
        private void CreateSkills(IList skls, SkillType type)
        {
            var root = ownerObj.transform.Find("SkillPoint");
            if (!root)
            {
                DHLog.Error("必须包含名字为SkillPoint的技能挂载点");
                return;
            }
            foreach (var item in skls)
            {
                var skill = item as Skill;
                CreateSkill(skill, root, type);
            }
        }
        private void ReplaceSkill(long oldSkillId, Skill skillData, Transform root)
        {
            var skillInstance = CreateSkillInstance(skillData.Cfg.SkillEffect, root);
            var skill = skillInstance.GetComponent<BaseSkill>();
            InitWithoutWait(skill, skillData).Forget();
            skillsMap.Add(skillData.id, skill);
            skillsMap.TryGetValue(oldSkillId, out var oldSkill);
            if(oldSkill == null)return;
            skillsMap.Remove(oldSkillId);
            var index = skills.IndexOf(oldSkill);
            skills[index] = skill;
            if (oldSkill.CurrentState == BaseSkill.State.Cooldown || oldSkill.CurrentState == BaseSkill.State.Ready)
            {
                oldSkill.CancelSkill();
                oldSkill.Release();
                ReleaseSkillInstance(oldSkill.gameObject);
            }
            else
            {
                pendingRemoveSkill.Add(oldSkill);
            }
        }
        private void CreateSkill(Skill skillData, Transform root, SkillType type)
        {
            if(skillData.Cfg.SkillEffect is (null or "")) return;
            var skillInstance = CreateSkillInstance(skillData.Cfg.SkillEffect, root);
            var skill = skillInstance.GetComponent<BaseSkill>();
            InitWithoutWait(skill, skillData).Forget();
            if (type == SkillType.Active)
            {
                activeSkills.Add(skill);
            }
            else
            {
                skills.Add(skill);
            }
            skillsMap.Add(skillData.id, skill);
        }
        private GameObject CreateSkillInstance(string path, Transform root)
        {
            if (entity) entity.LoadAssetSync(path);
            return entity ? entity.InstantiateObj(path, root) : AssetsManager.InstantiateSync(path, root.position, root.rotation, root);
        }
        private void ReleaseSkillInstance(GameObject obj)
        {
            if(obj is null)return;
            if (entity)
                entity.ReleaseObj(obj);
            else
                AssetsManager.ReleaseInstance(obj);
        }
        private async UniTaskVoid InitWithoutWait(BaseSkill skill, Skill skillData)
        {
            await skill.Init(skillData, ownerData, ownerObj);
        }
    }
}