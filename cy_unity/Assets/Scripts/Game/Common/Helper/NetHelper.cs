using System;
using System.Reflection;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Proto;

namespace DH.Game
{
    public static class NetHelper
    {
        /// <summary>
        /// 通过反射获取 属性值
        /// </summary>
        /// <param name="obj">反射的对象</param>
        /// <param name="variableName">属性或者字段名称</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static object GetValueByReflection(object obj, string variableName)
        {
            // 获取obj的类型信息
            Type type = obj.GetType();
     
            // 先尝试获取属性信息
            PropertyInfo property = type.GetProperty(variableName);
            if (property != null)
            {
                // 如果是可读属性，返回其值
                if (property.CanRead)
                {
                    return property.GetValue(obj);
                }
            }
            else
            {
                // 如果没有找到属性，尝试获取字段信息
                FieldInfo field = type.GetField(variableName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    // 返回字段的值
                    return field.GetValue(obj);
                }
            }
     
            throw new ArgumentException("Variable not found", nameof(variableName));
        }
        
        //统一处理网络消息错误提示
        public static bool CheckNetErrorMessage<T>(T message,bool isTips=false,Action result=null,Action errorResult=null)
        {
            if (message != null)
            {
                try
                {
                    int errorCode = (int)GetValueByReflection(message,"Status");
                    if (errorCode == 0)
                    {
                        result?.Invoke();
                        return true;
                    }
                    //AudioManager.Instance.PlayWrongTips();
                    if (!isTips)
                    {
                        errorResult?.Invoke();
                        return false;
                    }
                    var str = UIHelper.GetNetErrorMessage(errorCode);
                    ToastManager.Show(str);
                }
                catch (Exception e)
                {
                    errorResult?.Invoke();
                    return false;
                }
            }
            errorResult?.Invoke();
            return false;
        }
        
      

    }
}