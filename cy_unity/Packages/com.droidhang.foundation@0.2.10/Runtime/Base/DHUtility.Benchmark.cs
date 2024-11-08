using System;
using System.Diagnostics;

namespace DHFramework
{
    public class Benchmark : IDisposable
    {
        private Stopwatch sw;
        private string tag;

        public Benchmark(string tag)
        {
            this.tag = tag;
            sw = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            sw.Stop();
            DHLog.Debug($"执行任务{tag}耗时{sw.Elapsed.TotalMilliseconds}毫秒");
        }
    }
}