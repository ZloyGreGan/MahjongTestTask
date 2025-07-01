using System;
using Game.Scripts.Common.UI.WindowsDirector;
using UnityEngine;

namespace Game.Scripts.Common.MVP
{
    public abstract class APresenter
    {
        protected readonly IWindowsDirector _windowsDirector;
        protected readonly IModel _model;
        protected readonly IView _view;

        public bool IsInited { get; protected set; }
        public bool IsActive { get; protected set; }

        public IView GetView() => _view;

        protected APresenter(IView view, IModel model, IWindowsDirector windowsDirector)
        {
            _windowsDirector = windowsDirector;
            _model = model;
            _view = view;
        }

        public virtual void SetActive(bool value)
        {
            if (value && IsInited == false)
            {
                _view.Init(this);
                Init();
                IsInited = true;
            }

            if (IsActive == value) return;

            try
            {
                _view.SetActive(value);

                if (value)
                    OnEnable();
                else
                    OnDisable();

                IsActive = value;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Exception on Open {GetType()}: {ex.Message} {ex.StackTrace}");
            }
            finally
            {
                IsActive = value;
            }
        }

        protected virtual void Init() { }
        protected abstract void OnEnable();
        protected abstract void OnDisable();
    }

    public abstract class WindowPresenter<TView, TModel> : APresenter
        where TView : IView
        where TModel : IModel
    {
        protected TView View => (TView)_view;
        protected TModel Model => (TModel)_model;
        
        protected WindowPresenter(TView view, TModel model, IWindowsDirector windowsDirector)
            : base(view, model, windowsDirector) { }
    }
}