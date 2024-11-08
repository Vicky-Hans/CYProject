using System.Collections.Generic;
using DH.Config;
using UnityEngine;

namespace DH.Data
{
    /// <summary>
    /// 物品位置类型
    /// </summary>
    public enum ELocationType
    {
        /// <summary>
        /// 仓库
        /// </summary>
        Godown = 1,
        /// <summary>
        /// 背包
        /// </summary>
        Backpack = 2,
        /// <summary>
        /// 心愿池
        /// </summary>
        WishPool = 3
    }
    /// <summary>
    /// 背包武器数据
    /// </summary>
    public class BackpackWeaponData
    {
        public long Uid;
        public int Width;
        public int Height;
        public int RowIdx;//实际行
        public int ColumnIdx;//实际列
        public int EquipId;//装备Id
        public int WeaponId;//模型Id
        public int WeaponAttrType;//武器属性类型1~普通武器 2~圣物武器 
        public int AdType;//广告类型
        public int WeaponLev;
        public string IconPath;
        public string AdIconPath;
        public string HighEffect;//高品阶特效路径
        public int WeaponParamId;//武器参数Id*10+广告状态（0-未看，1-看过）
        public bool IsUnlocked;//是否解锁(是否看过广告)
        public GridType ShapType;//形状类型
        public List<int> NextInfo;//下级武器信息
        public ELocationType LocationType;//位置类型（背包还是随机库里）
        public List<Vector2Int> OccupyList;//占位信息
    }
    /// <summary>
    /// 新增格子数据
    /// </summary>
    public class GridAddData
    {
        public long Uid;
        public int Width;
        public int Height;
        public int GridId;
        public int AdType;//广告类型
        public int GridParamId;//格子参数Id*10+广告状态（0-未看，1-看过）
        public string IconPath;
        public string AdIconPath;
        public bool IsUnlocked;//是否解锁(是否看过广告)
        public GridType ShapType;//形状类型
        public ELocationType LocationType;//位置类型（背包还是随机库里）
        public List<Vector2Int> OccupyList;//占位信息
    }
    
    public enum EBlockState {
        Normal = 1,
        AddCell = 2,
    }
    /// <summary>
    /// 随机库武器数据
    /// </summary>
    public class RandomWeaponData
    {
        public int WeaponId;//模型Id
        public int WeaponAttrType;//武器属性类型1~普通武器 2~圣物武器 
    }
    public enum EDragState{
        None = 0,//待机状态
        Weapon = 1,//武器
        BlockAdd = 2,//空格子
        DragBatch = 3,//所有地图
    }

    public enum EBlockOccupyState {
        Solid = 1,
        Virtual = 2,
    }
}