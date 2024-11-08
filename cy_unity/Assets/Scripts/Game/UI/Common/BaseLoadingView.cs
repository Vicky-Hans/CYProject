using DH.UIFramework;
using DHFramework;
using TMPro;
using UnityEngine;

namespace Game.UI.CommonView
{
    public partial class BaseLoadingView : BaseView
    {
        public override bool FullScreen => true;
        public TMP_Text progressTxt;
        public float progressValue = 0f;
        public float targetProgressValue = 0f;
        
        public void SetProgressText(string text)
        {
            UIHelper.SetText(progressTxt, text);
        }
    
        public virtual void SetProgress(float progress)
        {
            
        }

        public virtual void Update()
        {
            if (progressValue < targetProgressValue)
            {
                progressValue += Time.deltaTime * 500f;
                SetProgress(progressValue);
            }
        }

        public virtual void UpdateState(TransitionState transitionState)
        {
            var tmpValue = ProcedureConfig.GetTransitionProgress(transitionState);
            targetProgressValue = tmpValue > targetProgressValue ? tmpValue : targetProgressValue;
            SetProgressText(ProcedureConfig.GetTransitionText(transitionState));
            // SetProgress(ProcedureConfig.GetTransitionProgress(transitionState));
        }

        protected virtual void OnDestroy()
        {
            DHLog.Debug("OnDestroy");
        }
    }
}