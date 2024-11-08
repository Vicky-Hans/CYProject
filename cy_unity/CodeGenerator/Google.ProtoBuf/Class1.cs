using System;
using System.Collections.Generic;

namespace Google.Protobuf
{

    public class EquipItem : IBufferMessage
    {

    }

    public partial class PbData : IBufferMessage
    {
        private string data1;
        private int data2;
        private int data3;
        private int data7;
        private List<EquipItem> data4;
        private List<int> data5;
        private Dictionary<int, EquipItem> data6;

        public string Data1 { get => data1; set => data1 = value; }
        public int Data2 { get => data2; set => data2 = value; }
        public int Data3 { get => data3; set => data3 = value; }
        public List<EquipItem> Data4 { get => data4; set => data4 = value; }
        public List<int> Data5 { get => data5; set => data5 = value; }
        public Dictionary<int, EquipItem> Data6 { get => data6; set => data6 = value; }
        public int Data7 { get => data7; set => data7 = value; }
    }

}
