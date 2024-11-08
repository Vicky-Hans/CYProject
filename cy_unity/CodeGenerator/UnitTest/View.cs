using DH.Data;
using DH.UIFramework.Builder;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DH.UIFramework
{
    public class BaseView
    {
        protected virtual void InitializeBinding()
        {

        }

        public virtual void Create()
        {

        }
    }

    public partial class View : BaseView
    {
        public static ModelData modelData;

        private PbData data;
        public override void Create()
        {
            data = new PbData();
            var item = new BindingBuilder<PbData, ModelData>();
            var bindSet = CreateBindingSet();
            item.Bind(this.data).For(v => v.Data1).ToExpression(vm =>vm.Data1 != string.Empty);
            OnCreate();
        }

        public Dictionary<int, DH.Data.EquipItemData> Wrap(Dictionary<int, DH.Data.EquipItemData> data)
        {
            return data;
        }

        public static string CreateBindingSet()
        {
            return null;
        }


        public string GetParam(object data)
        {
            return null;
        }

        public int GetParam()
        {
            return 0;
        }

        public void OnCreate()
        {

        }
    }
}
