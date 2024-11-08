using System;

namespace DH.Data
{
    public partial class ProtoWrapAttribute : System.Attribute
    {
        public Type type;

        public ProtoWrapAttribute(Type targetType)
        {
            type = targetType;
        }
    }
}