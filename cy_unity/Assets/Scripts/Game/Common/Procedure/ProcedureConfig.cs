using DH.Config;
using DH.Game;

public enum ProcedureState
{
    None,
    Enter,
    Active,
    DeActive,
    Exit
}

public enum TransitionState
{
    None,
    UnLoad,
    LoadScene,
    Preload,
    Enter,
    End
}

public static partial class ProcedureConfig
{
    public static string GetTransitionText(TransitionState transitionState)
    {
        string key;
        switch (transitionState)
        {
            case TransitionState.UnLoad:
            case TransitionState.LoadScene:
                key = LocalizeHelper.GetGlobal(GlobalLanguageId.SceneLoading);
                break;
            case TransitionState.Preload:
                key = LocalizeHelper.GetGlobal(GlobalLanguageId.ResourceLoading);
                break;
            default:
                key = LocalizeHelper.GetGlobal(GlobalLanguageId.EnterGame);
                break;
        }

        return key;
    }

    public static int GetTransitionProgress(TransitionState transitionState)
    {
        var progress = 0;
        switch (transitionState)
        {
            case TransitionState.UnLoad:
                progress = 0;
                break;
            case TransitionState.LoadScene:
                progress = 20;
                break;
            case TransitionState.Preload:
                progress = 50;
                break;
            case TransitionState.Enter:
                progress = 80;
                break;
            case TransitionState.End:
                progress = 100;
                break;
        }

        return progress;
    }
}