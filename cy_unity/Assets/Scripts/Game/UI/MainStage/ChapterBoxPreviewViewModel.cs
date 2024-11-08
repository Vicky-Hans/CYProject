using System.Collections.Generic;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using DHFramework;
using Game.UI.MainStage;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class ChapterBoxPreviewViewModel : ViewModelBase
    {

        [AutoNotify] private string titleStr;
        public ObservableList<CellItemViewModel> Items = new();
        [AutoNotify] private Vector2 bgSize;

        [AutoNotify] private Vector3 nodePos;
        // public Vector3 Offset = 
        [AutoNotify] private Vector3 offset = new Vector3(0,70, 0);
        [Preserve]
        public ChapterBoxPreviewViewModel(List<Reward>rewards)
        {
            for (int i = 0; i < rewards.Count; i++)
            {
                CellItemViewModel model = CellItemViewModel.Create(rewards[i]);
                model.SetSize(ECellItemSizeType.Size120X100);
                Items.Add(model);
            }

            UpdateBgSize();
        }

        [Command]
        private void OnClickClose()
        {
            UIManager.Instance.CloseDialog<ChapterBoxPreviewView>();
        }

        private void UpdateBgSize()
        {
            var count = Items.Count;
            var width = 130 * count + 40;
            var height = 160;
            BgSize = new Vector2(width, height);
        }
    }
    
}