using DH.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    internal static class DataCenter
    {
        public static List<ModelData> modelDatas;
        public static Dictionary<int,ModelData> modelDicDatas;
        private static List<ModelData> modelPropDatas;

        public static List<ModelData> ModelPropDatas { get => modelPropDatas; set => modelPropDatas = value; }
    }
}
