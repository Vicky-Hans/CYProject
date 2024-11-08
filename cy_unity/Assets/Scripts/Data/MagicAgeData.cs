using DH.Config;
using DH.Proto;


namespace DH.Data
{	
	public enum MagicEraItemState
	{
		Lock,
		NotGet,
		Get
	}
    [ProtoWrap(typeof(MagicAgeSync))]
    public partial class MagicAgeData : BaseData
    {
	    
	    public MagicEraItemState GetState(int  id)
	    {
		    var config = ConfigCenter.AgeMagicPackageCfgColl.GetDataById(id);

		    if (config.FirstId != 0 && !packRecord.ContainsKey(config.FirstId))
		    {
			    return MagicEraItemState.Lock;
		    }

		    if (config.FirstId == 0)
		    {
			    if (!packRecord.ContainsKey(config.Id))
			    {
				    return MagicEraItemState.NotGet;
			    }

			    if (packRecord.ContainsKey(config.Id) && packRecord[id] < config.BuyLimit)
			    {
				    return MagicEraItemState.NotGet;
			    }
		    }
		    else
		    {
			    if (!packRecord.ContainsKey(config.FirstId))
			    {
				    return MagicEraItemState.Lock;
			    }
			    if (packRecord.ContainsKey(config.FirstId) && (!packRecord.ContainsKey(config.Id) || (packRecord.ContainsKey(config.Id) && packRecord[id] < config.BuyLimit)))
			    {
				    return MagicEraItemState.NotGet;
			    }
		    }

		    return MagicEraItemState.Get;

	    }

	    public bool IsRed()
	    {
		    var items = ConfigCenter.AgeMagicPackageCfgColl.DataItems;
		    for (int i = 0; i < items.Count; i++)
		    {
			    var config = items[i]; 
			    if (config.FirstId == 0 && config.PackageId == 0 &&
			        (!packRecord.ContainsKey(config.Id) || (packRecord.ContainsKey(config.Id) && packRecord[config.Id] < config.BuyLimit)))
			    {
				    return true;
			    }
			    if (config.FirstId != 0 && packRecord.ContainsKey(config.FirstId) && config.PackageId == 0 &&
			        (!packRecord.ContainsKey(config.Id) || (packRecord.ContainsKey(config.Id) && packRecord[config.Id] < config.BuyLimit)))
			    {
				    return true;
			    }
		    }
		    return false;
	    }

	    /// <summary>
	    /// 剩余次数
	    /// </summary>
	    /// <param name="id"></param>
	    /// <returns></returns>
	    public int GetNums(int id)
	    {
		    if (packRecord.ContainsKey(id))
		    {
			    return packRecord[id];
		    }
		    return 0;
	    }

	    public bool GetAwards;
	    public void GetAward(int id)
	    {
		    if (!packRecord.ContainsKey(id))
		    {
			    packRecord.Add(id,1);
		    }
		    else
		    {
			    packRecord[id]++;
		    }
			RaisePropertyChanged(nameof(GetAwards));
	    }
	    
	    /// <summary>
	    /// 是否领取完了
	    /// </summary>
	    /// <returns></returns>
	    public bool IsBuyOver()
	    {
		    var items = ConfigCenter.AgeMagicPackageCfgColl.DataItems;
		    for (int i = 0; i < items.Count; i++)
		    {
			    var config = items[i]; 
			    if (!PackRecord.ContainsKey(config.Id))
			    {
				    return false;
			    }
			    if (PackRecord.ContainsKey(config.Id) &&  packRecord[config.Id] < config.BuyLimit)
			    {
				    return false;
			    }
		    }
		    return true;
	    }

	    public int GetMowIndex()
	    {
		    var items = ConfigCenter.AgeMagicPackageCfgColl.DataItems;
		    for (int i = 0; i < items.Count; i++)
		    {
			    var config = items[i]; 
			    if (GetState(config.Id)==MagicEraItemState.NotGet)
			    {
				    return i;
			    }
		    }
		    return 0;
	    }
	    
	    #region 自选缓存

	    public void SetSelectPacket(int packetId, int index)
	    {
		    OptionalRecord[packetId] = index;
		    RaisePropertyChanged(nameof(OptionalRecord));
	    }
        
	    public int GetSelectPacket(int packetId)
	    {
		    if (OptionalRecord.ContainsKey(packetId))
		    {
			    return OptionalRecord[packetId];
		    }
		    return -1;
	    }

	    public bool CheckSelectPacket(int packetId)
	    {
		    return GetSelectPacket(packetId) != -1;
	    }
        
	    #endregion
	    
    }

}
