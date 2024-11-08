

namespace DH.UIFramework.Interactivity
{
    public class VisibilityNotification
    {
        public bool Visible { get; private set; }
        public object ViewModel { get; private set; }

        public VisibilityNotification(bool visible) : this(visible, null)
        {
        }

        public VisibilityNotification(bool visible, object viewModel)
        {
            this.Visible = visible;
            this.ViewModel = viewModel;
        }
    }
}
