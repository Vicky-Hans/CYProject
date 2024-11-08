using DH.UIFramework.Observables;
using SP.Base;

namespace DH.Data
{
    public abstract class BaseData : ObservableObject, IInitedable, ICleanable
    {
        public virtual void Init()
        {
            InitData();
        }

        public void Clear()
        {
            ClearData();
        }

        protected virtual void InitData() { }

        protected virtual void ClearData() { }
    }
}