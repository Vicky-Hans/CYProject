using DH.Config;
using DH.Data;
using Attribute = DH.Config.Attribute;

namespace DH.Game
{
    public partial class AttributesManager : ObservableSingleton<AttributesManager>
    {

        public string GetAttrIcon(string attrName)
        {
            var cfg = ConfigCenter.AttributesCfgColl.GetDataByName(attrName);
            return cfg != null ? cfg.Icon : UIHelper.NoneImagePath();
        }
        
        public string GetAttrName(string attrName)
        {
            var cfg = ConfigCenter.AttributesCfgColl.GetDataByName(attrName);
            if (cfg != null)
            { 
                var cfgL = ConfigCenter.AttributesLanguageCfgColl.GetDataById(cfg.Id);
                return cfgL?.Name ?? string.Empty;
            }
            return string.Empty;
        }
        
        public string GetAttrDesc(Attribute attr)
        {
            var cfg = ConfigCenter.AttributesCfgColl.GetDataByName(attr.Type);
            if (cfg == null) return string.Empty;
            return UIHelper.ParseValueToStringByType(attr.Value,(GameConst.ENumType)cfg.ShowType,cfg.Type);
        }

        public bool IsShowAttr(string attrName)
        {
            var cfg = ConfigCenter.AttributesCfgColl.GetDataByName(attrName);
            if (cfg != null)
            {
                return cfg.ShowType==1;
            }
            return false;
        }

        public string GetAttrDesc(WeaponAttr attr,WeaponAttr addAttr=null)
        {
            return GetAttrDesc(attr.Type,attr.Value,addAttr?.Value ?? 0);
        }
        
        public string GetAttrDesc(string type,float value,float addValue=0,bool isIntger = false)
        {
            var cfg = ConfigCenter.AttributesCfgColl.GetDataByName(type);
            if (cfg == null) return string.Empty;
            var strDesc = UIHelper.ParseValueToStringByType(value,(GameConst.ENumType)cfg.ShowType,cfg.Type,isIntger);
            if (addValue != 0)
            {
                var cfgAdd = ConfigCenter.AttributesCfgColl.GetDataByName(type);
                if (cfgAdd != null)
                {
                    var descAdd = UIHelper.ParseValueToStringByType(addValue, (GameConst.ENumType)cfgAdd.ShowType,cfgAdd.Type,isIntger);
                    strDesc += (addValue > 0 ?"+<color=#7CDE44>"+descAdd+"</color>": "<color=#7CDE44>"+descAdd+"</color>");

                }
            }
            return strDesc;
        }
    }
}
