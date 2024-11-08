using UnityEditor;

namespace DH.Editor
{
    public static partial class DHMenu
    {
        [MenuItem("DH Tools/生成pb", priority = 1)]
        private static void GenPB()
        {
            ProtoGenTools.GenPB();
        }

        [MenuItem("DH Tools/导表", priority = 201)]
        private static void ExprotExcelForDesign()
        {
            ExportExcelTools.Execute();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}