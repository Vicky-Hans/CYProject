

using DH.UIFramework.Registry;

namespace DH.UIFramework.Converters
{
    public class ConverterRegistry: KeyValueRegistry<string,IConverter>, IConverterRegistry
    {
        public ConverterRegistry()
        {
            this.Init();
        }

        protected virtual void Init()
        {
        }
    }
}
