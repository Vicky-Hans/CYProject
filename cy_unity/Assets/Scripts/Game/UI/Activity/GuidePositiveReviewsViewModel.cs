using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class GuidePositiveReviewsViewModel : ViewModelBase
    {
        [Preserve]
        public GuidePositiveReviewsViewModel()
        {
            DHUnityUtil.PlayerPrefs.SetInt(GameManager.Instance.GuidePositiveReviewsSrt, 1);
        }

        [Command]
        private void OnClickCloseBtn()
        {
            DHUnityUtil.PlayerPrefs.SetInt(GameManager.Instance.GuidePositiveReviewsSrt, 1);
            UIManager.Instance.CloseDialog<GuidePositiveReviewsView>();
        }

        [Command]
        private void OnClickGotoAppraiseBtn()
        {
            DHUnityUtil.PlayerPrefs.SetInt(GameManager.Instance.GuidePositiveReviewsSrt, 1);
            UIManager.Instance.CloseDialog<GuidePositiveReviewsView>();
            Usdk.CallService("Utils_evaluateApp", string.Empty);
        }

    }
}
