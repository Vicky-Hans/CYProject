namespace DH.Game
{
    public interface IMonsterAnimator
    {
        public void PlayWalk();
        public void PlayAttack();
        public void PlaySkill1();
        public void PlaySkill2();
        public void PlayIdle();
        public void PlayDead();
        public void PlayAnimation(string aniName);
        public void PlaySpecAnimation(string aniName, bool loop = false);
        public void AddSpecAnimation(string aniName, bool loop = false);
        public void FlipX(bool flag);
        public void Pause();
        public void Resume();
    }
}