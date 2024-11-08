using DH.UIFramework.Observables;

namespace DH.Data
{
    public class BattleResource : ObservableObject
    {
        private UnitBase owner;
        private long hp;
        private long armor;
        private long maxHp;
        private long goldCoin;

        public BattleResource(UnitBase owner)
        {
            this.owner = owner;
            owner.attr.OnAttributeChanged += OnAttributeChanged;
        }

        private void OnAttributeChanged(AttributeType attributeType)
        {
            RaisePropertyChanged(nameof(HpDesc));
        }

        public long Hp
        {
            get => hp;
            set
            {
                if (!Set(ref hp, value))
                {
                    return;
                }

                RaisePropertyChanged(nameof(Progress));
                RaisePropertyChanged(nameof(HpDesc));
            }
        }

        public long Armor
        {
            get => armor;
            set
            {
                if(!Set(ref armor, value))
                {
                    return;
                }
                RaisePropertyChanged(nameof(ArmorProgress));
                RaisePropertyChanged(nameof(ArmorDesc));
            }
        }

        public long MaxHp
        {
            get => maxHp;
            set
            { 
                Set(ref maxHp, value);
                if(Hp > maxHp)
                {
                    Hp = maxHp;
                }
            } 
        }

        public float Progress => MaxHp == 0 ? 1 : (float)hp / maxHp;

        public float ArmorProgress => MaxHp == 0 ? 1 : (float)armor / maxHp;

        public string HpDesc => $"{hp}/{maxHp}";
        
        public string ArmorDesc => $"{armor}/{maxHp}";

        public bool Full => hp == maxHp;
        public long GoldCoin
        {
            get => goldCoin;
            set => Set(ref goldCoin, value);
        }

        public void Init()
        {
            Hp = (long)owner.attr.Calc(AttributeType.Hp);
            var hpBonus = owner.attr.Calc(AttributeType.HpBonus);
            Hp =  Lodash.RoundToInt(Hp * (1 + hpBonus));
            MaxHp = Hp;
        }

        public void Init(long cur, long max)
        {
            MaxHp = max;
            Hp = cur;
        }

        public void AddHpPer(float hpPer)
        {
            var heal = (long)(MaxHp * hpPer);
            AddHp(heal);
        }

        public void AddHp(long val)
        {
            var old = Hp;
            Hp += val;
            if (Hp >= MaxHp)
            {
                Hp = MaxHp;
            }
        }

        public void DecHp(long val)
        {
            var old = Hp;

            Hp -= val;
            if (Hp <= 0)
            {
                Hp = 0;
            }
        }

        public void AddMaxHp(long val)
        {
            MaxHp += val;
        }
        
        public void DecMaxHp(long val)
        {
            MaxHp -= val;
            if (Hp > MaxHp)
            {
                Hp = MaxHp;
            }
            if(armor > MaxHp)
            {
                Armor = MaxHp;
            }
        }
        public void AddArmorPer(float armorPer)
        {
            var heal = (long)(MaxHp * armorPer);
            AddArmor(heal);
        }
        public void AddArmor(long val)
        {
            Armor += val;
            if (Armor >= MaxHp)
            {
                Armor = MaxHp;
            }
        }

        public void DecArmor(long val)
        {
            if(armor >= val)
            {
                Armor -= val;
            }
            else
            {
                val -= armor;
                Armor = 0;
                DecHp(val);
            }
        }

        public void AddCoin(long val)
        {
            GoldCoin += val;
        }

        public bool CanAfford(long val)
        {
            return GoldCoin >= val;
        }

        public bool UseCoin(long val)
        {
            if (GoldCoin < val)
            {
                return false;
            }

            GoldCoin -= val;
            return true;
        }

        public bool IsDead()
        {
            return Hp <= 0;
        }

        public bool IsAlive()
        {
            return !IsDead();
        }

        public void Revive()
        {
            Hp = (long)owner.attr.Calc(AttributeType.Hp);
            MaxHp = Hp;
        }
    }
}