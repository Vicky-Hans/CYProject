using DH.UIFramework.Proxy;
using DH.UIFramework.Registry;

namespace DH.UIFramework.Converters
{
    public interface IConverter
    {
        void Convert(object value, IModifiable target);

        object ConvertBack(object value);
    }

    public interface IConverter<TFrom, TTo> : IConverter
    {
        void Convert(TFrom value, IModifiable target);

        TFrom ConvertBack(TTo value);
    }

    public interface IConverterRegistry : IKeyValueRegistry<string, IConverter>
    {
    }
}
