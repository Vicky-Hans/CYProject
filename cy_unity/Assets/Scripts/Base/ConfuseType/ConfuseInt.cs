using System;

namespace DH.Base
{
    public struct ConfuseInt : IEquatable<ConfuseInt>
    {
        private int key;
        private int value;
        private bool isInit;

        private static int Encrypt(int value, int key)
        {
            return value ^ key;
        }

        private static int Decrypt(int value, int key)
        {
            return value ^ key;
        }

        public void SetValue(int value)
        {
            if (!isInit)
            {
                key = ConfuseTypeUtility.GetRandomKeyInt32();
                isInit = true;
            }

            this.value = Encrypt(value, key);
        }

        public int GetValue()
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
            if (obj is ConfuseInt)
            {
                return Equals((ConfuseInt) obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(ConfuseInt other)
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

        public static implicit operator ConfuseInt(int value)
        {
            ConfuseInt confuse = new ConfuseInt();
            confuse.SetValue(value);
            return confuse;
        }

        public static implicit operator int(ConfuseInt value)
        {
            return value.GetValue();
        }
        
    }
}