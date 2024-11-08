using System;
using DHFramework;

namespace DH.Game
{
    public partial class UIManager
    {
        public class GotoScope : IDisposable
        {
            public GotoScope()
            {
                instance.lockCreateView = true;
            }

            public void Dispose()
            {
                instance.lockCreateView = false;
            }
        }

        public class SafeGotoScope : IDisposable
        {
            public SafeGotoScope()
            {
                instance.SetInputState(nameof(SafeGotoScope),false);
                DHLog.Debug("SafeGotoScope enter");
            }

            public void Dispose()
            {
                instance.SetInputState(nameof(SafeGotoScope),true);
                DHLog.Debug("SafeGotoScope release");
            }
        }
    }
}