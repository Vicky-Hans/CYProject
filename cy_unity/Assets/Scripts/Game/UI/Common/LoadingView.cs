using System.Collections.Generic;
using DH.Data;
using Game.UI.CommonView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace DH.Game
{
    public partial class LoadingView : BaseLoadingView
    {
        public Slider progressSlider;
        public Image progressBar;
        public GameObject progressIcon;

        public List<GameObject> viewList = new ();
        
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            // var idx = Lodash.RandRange(0, viewList.Count);
            // for (int i = 0; i < viewList.Count; i++)
            // {
            //     viewList[i].SetActive(idx == i);
            // }
        }
    
        public override void SetProgress(float progress)
        {
            progressBar.fillAmount = progress / 100.0f;
            var total = progressIcon.transform.parent.GetComponent<RectTransform>().rect.width;
            progressIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(progressBar.fillAmount*total,0 );
        }
        

        public override void UpdateState(TransitionState transitionState)
        {
            targetProgressValue = ProcedureConfig.GetTransitionProgress(transitionState);
            SetProgressText(ProcedureConfig.GetTransitionText(transitionState));
            // SetProgress(ProcedureConfig.GetTransitionProgress(transitionState));
        }
        
    }
}