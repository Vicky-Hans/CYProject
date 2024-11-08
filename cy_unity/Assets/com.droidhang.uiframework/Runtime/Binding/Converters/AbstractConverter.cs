

using System;
using DH.UIFramework.Proxy;

namespace DH.UIFramework.Converters
{
    public abstract class AbstractConverter : IConverter
    {
        public virtual void Convert(object value,IModifiable modifiable)
        {
            modifiable?.SetValue(value);
        }

        public virtual object ConvertBack(object value)
        {
            return value;
        }
    }

    public abstract class AbstractConverter<TFrom, TTo> : IConverter<TFrom, TTo>
    {
        public virtual void Convert(object value,IModifiable modifiable)
        {
            modifiable?.SetValue(value);
        }

        public virtual void Convert(TFrom value,IModifiable modifiable)
        {
            throw new NotImplementedException();
        }

        public virtual object ConvertBack(object value)
        {
            return ConvertBack((TTo)value);
        }

        public virtual TFrom ConvertBack(TTo value)
        {
            throw new NotImplementedException();
        }
    }
}