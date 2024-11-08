using UnityEngine;

namespace DH.Game
{
    /// <summary>
    /// 只读属性
    /// 用于技能数值展示
    /// </summary>
    public class DebugDataAttribute : PropertyAttribute
    {
        
    }
    
    /// <summary>
    /// 用于自动生成Markdown文件供策划查看该技能属性名称和对应功能名称
    /// </summary>
    public class CommentAttribute : PropertyAttribute
    {
        public string text;
        public string key;

        public CommentAttribute(string text, string key = null)
        {
            this.text = text;
            this.key = key;
        }
    }
}