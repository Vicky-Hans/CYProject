using DH.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public class ModelDataItemModel
    {
        public ModelData Data;
    }

    public class OtherModelDataItemModel
    {
        public ModelData Data;
    }

    public partial class TestViewModel : ViewModelBase
    {
        public List<ModelDataItemModel> rewardData = new List<ModelDataItemModel>();
        public List<OtherModelDataItemModel> rewardData1 = new List<OtherModelDataItemModel>();

        private bool CheckSame(int key, ModelData value, OtherModelDataItemModel model)
        {
            return false;
        }

        private OtherModelDataItemModel CreateCell(int key, ModelData value)
        {
            return new OtherModelDataItemModel();
        }

        public TestViewModel()
        {
            //BindCollection(DataCenter.modelDatas, rewardData, (item) =>
            //{
            //    var itemData = new ModelDataItemModel();
            //    itemData.Data = item;
            //    return itemData;
            //});

            //BindCollection(DataCenter.ModelPropDatas, rewardData1, (item) =>
            //{
            //    var itemData = new OtherModelDataItemModel();
            //    itemData.Data = item;
            //    return itemData;
            //});

            //BindCollection(DataCenter.modelDicDatas, rewardData1,CreateCell,CheckSame);
        }

        private OtherModelDataItemModel Creator(ModelData data)
        {
            var itemData = new OtherModelDataItemModel();
            itemData.Data = data;
            return itemData;
        }
    }
}
