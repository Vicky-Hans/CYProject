namespace DH.Game
{
    public class BuffFrozen : BaseBuff
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