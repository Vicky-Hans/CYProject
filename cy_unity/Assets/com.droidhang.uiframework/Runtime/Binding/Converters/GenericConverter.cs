

using System;
using DH.UIFramework.Proxy;

namespace DH.UIFramework.Converters
{    
    public class GenericConverter<TFrom, TTo> : AbstractConverter<TFrom, TTo>
    {
        private Func<TFrom, TTo> handler;
        private Func<TTo, TFrom> backHandler;

        public GenericConverter(Func<TFrom, TTo> handler, Func<TTo, TFrom> backHandler)
        {
            this.handler = handler;
            this.backHandler = backHandler;
        }

        public override void Convert(object value, IModifiable modifiable)
        {
            if (value == null)
            {
                modifiable?.SetValue(default(TTo));
            }
            else
            {
                Convert((TFrom)value, modifiable);
            }
        }

        public override void Convert(TFrom value,IModifiable modifiable)
        {
            if (this.handler != null)
            {
                modifiable?.SetValue(this.handler(value));
                return;
            }
            modifiable?.SetValue(default(TTo));
        }

        public override TFrom ConvertBack(TTo value)
        {
            if (this.backHandler != null)
                return this.backHandler(value);
            return default(TFrom);
        }
    }
}
