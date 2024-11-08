using System;

namespace DH.Foundations.Event
{

    [AttributeUsage(AttributeTargets.Class)]
    public class ClassRegistAttribute : Attribute
    {
        
    }
    
    
    [AttributeUsage(AttributeTargets.Method)]
    public class DHServiceCallbackAttribute : Attribute
    {
        private string serviceName;

        public string ServiceName => serviceName;

        public DHServiceCallbackAttribute(string serviceName)
        {
            this.serviceName = serviceName;
        }
        
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class DHServiceProviderAttribute : Attribute
    {
        private string serviceName;

        public string ServiceName => serviceName;
        
        public DHServiceProviderAttribute(string serviceName)
        {
            this.serviceName = serviceName;
        }
    }
    
}