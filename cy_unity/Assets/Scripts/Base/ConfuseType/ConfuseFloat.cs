using System;
using System.Runtime.InteropServices;

namespace DH.Base
{
    public struct ConfuseFloat : IEquatable<ConfuseFloat>
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct FloatIntUnion
        {
            [FieldOffset(0)] public float FloatValue;

            [FieldOffset(0)] public int IntValue;
        }


        private int key;
        private int value;
        private bool isInit;

        private static int Encrypt(float value, int key)
        {
            FloatIntUnion union = new FloatIntUnion()
            {
                FloatValue = value,
            };

            return union.IntValue ^ key;
        }

        private static float Decrypt(int value, int key)
        {
            FloatIntUnion union = new FloatIntUnion()
            {
                IntValue = value ^ key,
            };

            return union.FloatValue;
        }

        public void SetValue(float value)
        {
            if (!isInit)
            {
                key = ConfuseTypeUtility.GetRandomKeyInt32();
                isInit = true;
            }

            this.value = Encrypt(value, key);
        }

        public float GetValue()
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
            if (obj is ConfuseFloat)
            {
                return Equals((ConfuseFloat) obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(ConfuseFloat other)
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

        public static implicit operator ConfuseFloat(float value)
        {
            ConfuseFloat confuse = new ConfuseFloat();
            confuse.SetValue(value);
            return confuse;
        }

        public static implicit operator float(ConfuseFloat value)
        {
            return value.GetValue();
        }
    }
    
    public static class ConfuseTypeUtility
    {
        public static long GetRandomKeyInt64()
        {
#if UNITY_2020_3_OR_NEWER
            int keyA = UnityEngine.Random.Range(Int32.MinValue, Int32.MaxValue);
            int keyB = UnityEngine.Random.Range(Int32.MinValue, Int32.MaxValue);
#else
            int keyA = Guid.NewGuid().GetHashCode();
            int keyB = Guid.NewGuid().GetHashCode();
#endif
            return (keyA << 32) | keyB;
        }

        public static int GetRandomKeyInt32()
        {
#if UNITY_2020_3_OR_NEWER
            return UnityEngine.Random.Range(Int32.MinValue, Int32.MaxValue);
#else
            return Guid.NewGuid().GetHashCode();
#endif
        }
    }
    
}
    