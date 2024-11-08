using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DH.Asset;
using DHFramework;
using DHHybridCLR.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace DHHybridCLR.Scripts
{
    public partial class GameRoot
    {
        public StartupLauncherConfig StartupConfig { get; set; }

        public void InitStartupConfig()
        {
            StartupConfig = Resources.Load<StartupLauncherConfig>("StartupLauncherConfig");
            if (StartupConfig == null)
            {
                DHLog.Error("请先创建StartupLauncherConfig以启动游戏");
            }
        }
    }
}
