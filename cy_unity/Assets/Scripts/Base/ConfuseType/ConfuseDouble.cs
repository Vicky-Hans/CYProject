using System;
using System.Runtime.InteropServices;

namespace DH.Base
{
    public struct ConfuseDouble : IEquatable<ConfuseDouble>
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct DoubleIntUnion
        {
            [FieldOffset(0)] public double DoubleValue;

            [FieldOffset(0)] public long LongValue;
        }


        private long key;
        private long value;
        private bool isInit;

        private static long Encrypt(double value, long key)
        {
            DoubleIntUnion union = new DoubleIntUnion()
            {
                DoubleValue = value,
            };

            return union.LongValue ^ key;
        }

        private static double Decrypt(long value, long key)
        {
            DoubleIntUnion union = new DoubleIntUnion()
            {
                LongValue = value ^ key,
            };

            return union.DoubleValue;
        }

        public void SetValue(double value)
        {
            if (!isInit)
            {
                key = ConfuseTypeUtility.GetRandomKeyInt64();
                isInit = true;
            }

            this.value = Encrypt(value, key);
        }

        public double GetValue()
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
            if (obj is ConfuseDouble)
            {
                return Equals((ConfuseDouble) obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(ConfuseDouble other)
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

        public static implicit operator ConfuseDouble(double value)
        {
            ConfuseDouble confuse = new ConfuseDouble();
            confuse.SetValue(value);
            return confuse;
        }

        public static implicit operator double(ConfuseDouble value)
        {
            return value.GetValue();
        }
    }
}