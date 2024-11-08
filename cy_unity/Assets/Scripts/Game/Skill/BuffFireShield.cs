using DH.Data;
namespace DH.Game
{
    public partial class BuffFireShield: BaseBuff
    {
        public override void OnUpdate(float deltaTime)
        {
            if(Recycled)return;
            if (buff == null)
            {
                Recycle();
                return;
            }
            if (!buff.IsValid(GameTime.Instance.GTime)) Recycle();
        }
    }
}