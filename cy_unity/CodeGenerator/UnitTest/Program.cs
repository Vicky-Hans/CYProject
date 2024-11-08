using DH.Data;
using DH.UIFramework;

namespace DH.Game
{
    public partial class Program
    {
        [AutoNotify]
        private string item;

        static void Main(string[] args)
        {
            int i = 1;
            BindCollection(); 
            var view = new View();
            view.Create();
            var modelData = new ModelData(new Google.Protobuf.PbData());
            modelData.Data6[0].PropertyChanged += Program_PropertyChanged; ;
            //TestViewModel testViewModel = new TestViewModel();
            Console.WriteLine("Hello, World!");
            InitCollection();
            DisposeCollection();
        }

        private static void Program_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void BindCollection()
        {

        }

        private static void InitCollection()
        {

        }

        private static void DisposeCollection()
        {

        }
    }
}