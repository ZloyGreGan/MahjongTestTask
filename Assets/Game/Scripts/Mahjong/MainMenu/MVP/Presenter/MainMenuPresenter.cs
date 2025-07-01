using System;
using Game.Scripts.Common.MVP;
using Game.Scripts.Common.UI.WindowsDirector;
using Game.Scripts.Mahjong.GameMenu.MVP.Presenter;
using Game.Scripts.Mahjong.MainMenu.MVP.Model;
using Game.Scripts.Mahjong.MainMenu.MVP.View;
using UnityEngine;

namespace Game.Scripts.Mahjong.MainMenu.MVP.Presenter
{
    public class MainMenuPresenter : WindowPresenter<IMainMenuView, IMainMenuModel>, IDisposable
    {
        public MainMenuPresenter(
            IMainMenuView view, 
            IMainMenuModel model, 
            IWindowsDirector windowsDirector
            ) : base(view, model, windowsDirector)
        { }

        protected override void OnEnable() { }

        protected override void OnDisable() { }

        public void PlayGame()
        {
            _windowsDirector.OpenWindow<GameMenuPresenter>();
        }

        public void ExitGame()
        {
            Application.Quit();
        }

        public void Dispose() { }
    }
}