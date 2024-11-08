using System;

namespace DH.Game
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProcedureDeepAttribute : Attribute
    {
        public ProcedureDeepAttribute(int value)
        {
        }
    }
}