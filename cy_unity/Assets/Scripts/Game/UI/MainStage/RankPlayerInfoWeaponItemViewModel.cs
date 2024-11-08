using Cysharp.Threading.Tasks;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.UIFramework;
using Extend;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class RankPlayerInfoWeaponItemViewModel : ViewModelBase
    {

        [AutoNotify] private Vector2 cellPos;
        [AutoNotify] private Vector3 iconScale;
        [AutoNotify] private Vector3 localPos;
        private int curWeaponId;
        private Vector2 curPos;
        private DhImage iconImg;
        /// <summary>
        /// 单个cell大小
        /// </summary>
        private Vector2 cellSize = Vector2.one * 80;
        private string bgPath = "ranking[ranking_lattice_1]";
        private string nonePath = UIHelper.NoneImagePath();
        public DhImage IconImg
        {
            get => null;
            set
            {
                iconImg = value;
                if(iconImg == null) return;
                UpdateIcon();
            }
        }

        [Preserve]
        public RankPlayerInfoWeaponItemViewModel(int weaponId, Vector2 pos)
        {
            curWeaponId = weaponId;
            if (curWeaponId == -1)
            {
                curPos = pos;
                CellPos = new Vector2(curPos.x * cellSize.x, -curPos.y * cellSize.y);
            }
            else
            {
                curPos = GetSStartPosByPosXAndType(weaponId,pos);
                CellPos = new Vector2(curPos.y * cellSize.x, -curPos.x * cellSize.y);
            }
            LocalPos = new Vector3(CellPos.x, CellPos.y, 0);
            UpdateIcon();
        }
        private void UpdateIcon()
        {
            if (iconImg == null)  return;
            
            if (curWeaponId == -1)
            {
                AssetsManager.LoadAssetAsyncWithCallback(bgPath, (Sprite sprite) =>
                {
                    SetIconSprite(sprite);
                });
                iconScale = Vector3.one;
            }
            else
            {
                var cfg = ConfigCenter.EquipModelCfgColl.GetDataById(curWeaponId);
                if (cfg == null)
                {
                    AssetsManager.LoadAssetAsyncWithCallback(nonePath, (Sprite sprite) =>
                    {
                        SetIconSprite(sprite);
                    });
                    IconScale = Vector3.one;
                }
                else
                {
                    AssetsManager.LoadAssetAsyncWithCallback(cfg.BattlePic, (Sprite sprite) =>
                    {
                        SetIconSprite(sprite);
                    });
                    IconScale = Vector3.one * 0.54f;
                    AddHighEffect(cfg.HighEffect);
                   
                }
            }
        }
        private void SetIconSprite(Sprite sprite)
        {
            if (iconImg == null) return;
            iconImg.sprite = sprite;
            iconImg.SetNativeSize();
        }

        private async UniTaskVoid AddHighEffect(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if(iconImg == null) return;
            GameObject actionItem = await AssetsManager.InstantiateWithParentAsync(path, iconImg.transform, false);
        }

        private Vector2 GetSStartPosByPosXAndType(int weaponId, Vector2 pos)
        {
            var cfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponId);
            if (cfg == null) return pos;
            return GetSStartPosByPosXAndType(pos, (GridType)cfg.GridType);
            
        }
        
        /// <summary>
        /// 根据格子类型获取起始位置
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private Vector2 GetSStartPosByPosXAndType(Vector2 pos, GridType type)
        {
            Vector2 ret = pos;
            switch (type)
            {
                case GridType.LzThreeNum: ret.y -=1; break;
            }
            return ret;
        }
    }
}