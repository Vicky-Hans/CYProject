using System;

namespace DH.Base
{
    public struct ConfuseLong : IEquatable<ConfuseLong>
    {
        private long key;
        private long value;
        private bool isInit;

        private static long Encrypt(long value, long key)
        {
            return value ^ key;
        }

        private static long Decrypt(long value, long key)
        {
            return value ^ key;
        }

        public void SetValue(long value)
        {
            if (!isInit)
            {
                key = ConfuseTypeUtility.GetRandomKeyInt64();
                isInit = true;
            }

            this.value = Encrypt(value, key);
        }

        public long GetValue()
        {
            return Decrypt(value, key);
        }

        public override string ToString()
        {
            return GetValue().ToString();
        }

        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ConfuseLong)
            {
                return Equals((ConfuseLong) obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(ConfuseLong other)
        {
            if (other.value == this.value && other.key == this.key)
            {
                return true;
            }
            else
            {
                return other.GetValue() == this.GetValue();
            }
        }

        public static implicit operator ConfuseLong(long value)
        {
            ConfuseLong confuse = new ConfuseLong();
            confuse.SetValue(value);
            return confuse;
        }

        public static implicit operator long(ConfuseLong value)
        {
            return value.GetValue();
        }
        
    }
}