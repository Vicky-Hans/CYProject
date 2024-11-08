using System.Collections.Generic;

namespace DH.Game
{
    public partial class GameNetworkManager
    {
        private Dictionary<string, bool> silentCmdDic = new()
        {
            {"33_1", true},
            {"33_2", true},
            {"33_3", true},
            {"33_4", true},
            {"11_2", true}
        };
    }
}