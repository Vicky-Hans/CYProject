using DH.UIFramework.Registry;

namespace DH.UIFramework.Converters
{
    public interface IConverterRegistry : IKeyValueRegistry<string, IConverter>
    {
    }
}
