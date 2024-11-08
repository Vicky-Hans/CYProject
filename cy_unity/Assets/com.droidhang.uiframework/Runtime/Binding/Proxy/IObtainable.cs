

namespace DH.UIFramework.Proxy
{
    public interface IObtainable
    {
        object GetValue();

        TValue GetValue<TValue>();
    }
}
