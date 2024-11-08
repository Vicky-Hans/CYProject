using System.Collections.Generic;
using DH.Config;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;

namespace Dh.Game.ViewModels
{
    public partial class CommonItemListModel : ViewModelBase
    {
        [AutoNotify] private ObservableList<CellItemViewModel> itemListModels=new();
        public CommonItemListModel(List<Reward> list)
        {
            foreach (var item in list)
            {
                itemListModels.Add(CellItemViewModel.Create(item,ECellItemSizeType.Size117X76));
            }
        }
    }
}
