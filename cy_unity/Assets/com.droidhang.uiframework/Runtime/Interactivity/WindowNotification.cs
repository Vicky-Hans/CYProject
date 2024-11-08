

namespace DH.UIFramework.Interactivity
{
    public class WindowNotification
    {
        public WindowNotification(ActionType actionType) : this(actionType, true, null)
        {
        }

        public WindowNotification(ActionType actionType, bool ignoreAnimation) : this(actionType, ignoreAnimation, null)
        {
        }

        public WindowNotification(ActionType actionType, object viewModel) : this(actionType, true, viewModel)
        {
        }

        public WindowNotification(ActionType actionType, bool ignoreAnimation, object viewModel)
        {
            this.IgnoreAnimation = ignoreAnimation;
            this.ActionType = actionType;
            this.ViewModel = viewModel;
        }

        public bool IgnoreAnimation { get; private set; }

        public ActionType ActionType { get; private set; }

        public object ViewModel { get; private set; }
    }

    public enum ActionType
    {
        CREATE,
        SHOW,
        HIDE,
        DISMISS
    }
}
