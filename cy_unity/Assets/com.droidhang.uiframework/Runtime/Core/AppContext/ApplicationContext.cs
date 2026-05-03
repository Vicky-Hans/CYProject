

using DH.UIFramework.Services;

namespace DH.UIFramework.AppContext
{
    /// <summary>
    /// ApplicationContext
    /// </summary>
    public class ApplicationContext : Context
    {
        public ApplicationContext() : this(null)
        {
        }

        public ApplicationContext(IServiceContainer container) : base(container, null)
        {

        }
    }
}
