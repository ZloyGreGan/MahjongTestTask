using System;
using Game.Scripts.Common.MVP;

namespace Game.Scripts.Common.UI.WindowsDirector
{
    public interface IWindowsDirector
    {
        event Action<Type> OnWindowOpen;
        
        TPresenter OpenWindow<TPresenter>() where TPresenter : APresenter;
    }
}