using DH.Data;
namespace DH.Game
{
    public partial class BuffFireYellow: BaseBuff
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