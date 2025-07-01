using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Common.MVP;
using Game.Scripts.Mahjong.MainMenu.MVP.Presenter;
using Zenject;

namespace Game.Scripts.Common.UI.WindowsDirector
{
    public class UIDirector : IWindowsDirector, IInitializable, IDisposable
    {
        public event Action<Type> OnWindowOpen;
        
        private List<IView> _views;
        private List<APresenter> _presenters;

        [Inject]
        public void Construct(
            List<IView> views,
            List<APresenter> presenters
            )
        {
            _views = views;
            _presenters = presenters;
        }
        
        public void Initialize()
        {
            OpenWindow<MainMenuPresenter>();
        }
        
        public TPresenter OpenWindow<TPresenter>() where TPresenter : APresenter
        {
            return (TPresenter) InnerOpenWindow(typeof(TPresenter));
        }

        private APresenter InnerOpenWindow(Type presenterType)
        {
            var presenter = _presenters.FirstOrDefault(p => p.GetType() == presenterType);
            if (presenter == null) throw new Exception($"Error for Get window Presenter of type {presenterType}");
            
            foreach (var handler in _presenters)
            {
                if (handler.IsActive) handler.SetActive(false);
            }
            
            HideAllWindowsViews();
            
            presenter.SetActive(true);
            OnWindowOpen?.Invoke(presenter.GetType());

            return presenter;
        }
        
        private void HideAllWindowsViews()
        {
            foreach (var view in _views.Where(view => view.IsActive))
            {
                view.SetActive(false);
            }
        }

        public void Dispose() { }
    }
}