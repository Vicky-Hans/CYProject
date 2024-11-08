using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DH.Config;
using DH.Data;
using DH.UIFramework.Observables;

namespace Data.Exp
{
    public class ExpMgr : ObservableObject
    {
        private UnitBase owner;
        private List<long> cfg;
        private long exp;
        private long lv;

        public long Exp
        {
            get => exp;
            set
            {
                if (!Set(ref exp, value))
                {
                    return;
                }

                RaisePropertyChanged(nameof(Progress));
            }
        }

        public long Lv
        {
            get => lv;
            set
            {
                if (!Set(ref lv, value))
                {
                    return;
                }

                RaisePropertyChanged(nameof(ExpMax));
                RaisePropertyChanged(nameof(Progress));
                RaisePropertyChanged(nameof(IsMaxLevel));
            }
        }

        public long ExpMax => cfg[(int)Lv - 1];

        public long LvMax => cfg.Count + 1;
        public bool IsMaxLevel => Lv >= LvMax;

        public float Progress => ExpMax != 0 ? (float)Exp / ExpMax : 0;

        public ExpMgr(UnitBase owner)
        {
            this.owner = owner;
            Lv = 1;
        }

        public void Init(List<long> cfg)
        {
            this.cfg = cfg;
        }

        public void ExpModify(long add)
        {
            if (add == 0)
            {
                return;
            }

            Exp += add;
            while (Exp >= ExpMax && !IsMaxLevel)
            {
                Exp -= ExpMax;
                Lv++;
            }
        }
    }
}