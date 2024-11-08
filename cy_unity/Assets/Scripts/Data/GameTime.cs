using DH.UIFramework;

namespace DH.Data
{
    public partial class GameTime :ObservableSingleton<GameTime>
    {
        [AutoNotify] private float gTime;
        [AutoNotify] private bool pause;

        public void OnUpdate(float dt)
        {
            if (Pause) return;
            GTime += dt;
        }
    }
}