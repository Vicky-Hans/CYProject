namespace DH.Game
{
    public partial class BuffClothesPalsy : BaseBuff
    {
        public override void OnUpdate(float deltaTime)
        {
            if(Recycled)return;
            if (buff == null)
            {
                Recycle();
                return;
            }
            time += deltaTime;
            if (time > buff.duration) Recycle();
        }
    }
}