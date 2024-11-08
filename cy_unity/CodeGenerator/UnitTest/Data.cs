using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DH.Game;
using DH.UIFramework;
using DH.UIFramework.Observables;
using Google.Protobuf;

namespace DH.Game
{

    [AttributeUsage(AttributeTargets.Field)]
    public class AssetPathAttribute : Attribute
    {

    }
}

namespace DH.Data
{
    public class ProtoWrapAttribute : Attribute
    {
        public Type type;

        public ProtoWrapAttribute(Type targetType)
        {
            type = targetType;
        }
    }

    public class BaseData : ObservableObject
    {

    }

    [ProtoWrap(typeof(EquipItem))]
    public partial class EquipItemData : BaseData
    {

    }

    [ProtoWrap(typeof(PbData))]
    public partial class ModelNestData : BaseData
    {
        
    }

    [ProtoWrap(typeof(PbData))]
    public partial class ModelData : BaseData
    {
        public static ModelNestData DataStatic => new ModelNestData(null);

        [AutoNotify]
        private int test;
        [AutoNotify]
        private ModelNestData nestData;
        [AssetPath]
        public string assetPath;

        public System.Collections.Generic.Dictionary<int, Google.Protobuf.EquipItem> TestData => new Dictionary<int, EquipItem>();


        public string GetStr() { return null; }

        [Command]
        public void Save()
        {

        }

        public bool CanSave()
        {
            return false;
        }

        public async Task AsyncSave()
        {

        }
    }

    [ProtoWrap(typeof(PbData))]
    public partial class ModelErrorData
    {

    }
}

namespace DH.UIFramework
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AutoNotifyAttribute : Attribute
    {
    }
}
