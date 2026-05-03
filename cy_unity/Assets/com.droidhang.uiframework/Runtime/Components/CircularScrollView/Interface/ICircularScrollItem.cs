using System;

namespace DH.UIFramework
{
    public interface ICircularScrollItem
    {
        public void OnItemShowCallback(int itemIndex, object vmData); //item显示时调用
        public void OnItemHideCallback(); //滚动出显示范围外回收时调用
    }
}