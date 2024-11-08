namespace DH.Game
{
    public partial class BuffElectrify : BaseBuff
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