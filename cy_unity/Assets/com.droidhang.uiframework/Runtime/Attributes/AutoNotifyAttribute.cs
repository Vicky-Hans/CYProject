using System;

namespace DH.UIFramework
{
    public enum NotifyAccess
    {
        Public,
        Private,
        Protected,
        Internal,
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class AutoNotifyAttribute : Attribute
    {
        public AutoNotifyAttribute()
        {
            
        }

        public AutoNotifyAttribute(NotifyAccess access)
        {
            
        }
    }
    
    [AttributeUsage(AttributeTargets.Field,AllowMultiple = true)]
    public class ChangeForAttribute : Attribute
    {
        public ChangeForAttribute(string propertyName)
        {
            
        }
    }
}