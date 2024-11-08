namespace DHFramework
{
    /// <summary>
    /// 游戏框架模块抽象类。
    /// </summary>
    public interface IGameFrameworkComponent
    {
        /// <summary>
        /// 游戏框架模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        void Update(float elapseSeconds, float realElapseSeconds);

        void Shutdown();
    }
}
