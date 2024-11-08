using System;
using System.Collections.Generic;
using DH.Config;
using DH.UIFramework.Observables;
using Google.Protobuf.Collections;

namespace DH.Data
{
    public enum AttributeCalcType
    {
        Add = 1,
        Sub = 2
    }

    public enum AttributeValueType
    {
        ValBase = 0, // 基础值
        ValMul = 1 // 万分比
    }

    public class AttributeMgr : ObservableObject
    {
        private UnitBase owner;
        private ObservableDictionary<long, Attribute> allAttributes;
        public ObservableDictionary<long, Attribute> AllAttributes => allAttributes;

        public Action<AttributeType> OnAttributeChanged;

        public AttributeMgr(UnitBase owner)
        {
            this.owner = owner;
            allAttributes = new ObservableDictionary<long, Attribute>();
        }
        public void Modify(List<DH.Config.Attribute> attrs)
        {
            if(attrs == null)return;
            var collection = ConfigCenter.AttributesCfgColl;
            foreach (var attr in attrs)
            {
                var cfg = collection.GetDataByName(attr.Type);
                if (cfg == null) throw new InvalidOperationException($"Invalid attr type {attr.Type}");

                Modify((AttributeType)cfg.Id, (AttributeValueType)cfg.Type, attr.Value);
            }
        }
        public void Modify(DH.Config.Attribute attr)
        {
            var collection = ConfigCenter.AttributesCfgColl;
            var cfg = collection.GetDataByName(attr.Type);
            if (cfg == null) throw new InvalidOperationException($"Invalid attr type {attr.Type}");
            Modify((AttributeType)cfg.Id, (AttributeValueType)cfg.Type, attr.Value);
        }
        public void Modify(Dictionary<string, int> dic)
        {
            foreach (var item in dic)
            {
                var attr = new DH.Config.Attribute(item.Key, item.Value);
                Modify(attr);
            }
        }
        public void Modify(MapField<string, int> dic)
        {
            foreach (var item in dic)
            {
                var attr = new DH.Config.Attribute(item.Key, item.Value);
                Modify(attr);
            }
        }
        public void Modify(AttributeType id, AttributeValueType vType, long val)
        {
            if (allAttributes.TryGetValue((long)id, out var attr))
            {
                attr.Modify(id, vType, val);
                OnAttributeChanged?.Invoke(id);
            }
            else
            {
                attr = new Attribute(id);
                attr.Modify(id, vType, val);
                allAttributes.Add((long)id, attr);
            }
        }
        public void SetAttr(AttributeType id, float val)
        {
            if (allAttributes.TryGetValue((long)id, out var attr))
            {
                attr.ValueBase = val;
            }
            else
            {
                attr = new Attribute(id);
                attr.ValueBase = val;
                allAttributes.Add((long)id, attr);
            }
        }
        public float Calc(AttributeType aType)
        {
            if (allAttributes.TryGetValue((long)aType, out var attr)) return attr.Value;
            return 0;
        }
    }
}