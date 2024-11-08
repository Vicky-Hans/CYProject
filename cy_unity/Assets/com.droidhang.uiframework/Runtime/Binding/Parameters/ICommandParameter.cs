

using System;

namespace DH.UIFramework.Parameters
{
    public interface ICommandParameter
    {
        object GetValue();

        Type GetValueType();
    }
}
