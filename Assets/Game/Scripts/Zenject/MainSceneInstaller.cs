using System;

using Game.Scripts.Common.Database.Resources;
using Game.Scripts.Common.MVP;
using Game.Scripts.Common.UI.WindowsDirector;
using Game.Scripts.Mahjong.GameMenu.Board.Generator;
using Game.Scripts.Mahjong.GameMenu.MVP.Presenter;
using Game.Scripts.Mahjong.MainMenu.MVP.Model;
using Game.Scripts.Mahjong.MainMenu.MVP.Presenter;
using Game.Scripts.Mahjong.MainMenu.MVP.View;
using Game.Scripts.MahjongSystem.Model;
using Game.Scripts.MahjongSystem.View;

using UnityEngine;
using Zenject;

namespace Game.Scripts.Zenject
{
    public class MainSceneInstaller : MonoInstaller
    {
        [SerializeField] private ResourceDatabase _resourceDatabase;

        [Space] 
        [SerializeField] private MainMenuView _mainMenuView;
        [SerializeField] private GameMenuView _gameMenuView;
        
        public override void InstallBindings()
        {
            BindSceneData();
            BindModels();
            BindViews();
            BindPresenters();
            BindWindowsDirector();
        }

        private void BindSceneData()
        {
            Container.Bind<IResourceDatabase>()
                .To<ResourceDatabase>()
                .FromInstance(_resourceDatabase)
                .AsSingle()
                .NonLazy();

            Container.Bind<ILevelGenerator>()
                .To<LevelGenerator>()
                .AsSingle()
                .NonLazy();
        }

        private void BindModels()
        {
            Container.BindInterfacesAndSelfTo<MainMenuModel>()
                .FromNew()
                .AsSingle()
                .NonLazy(); 
            
            Container.BindInterfacesAndSelfTo<GameMenuModel>()
                .FromNew()
                .AsSingle()
                .NonLazy(); 
        }

        private void BindViews()
        {
            Container.BindInterfacesAndSelfTo<MainMenuView>()
                .FromInstance(_mainMenuView)
                .AsSingle()
                .NonLazy();
            
            Container.BindInterfacesAndSelfTo<GameMenuView>()
                .FromInstance(_gameMenuView)
                .AsSingle()
                .NonLazy();
        }

        private void BindPresenters()
        {
            Container.Bind(typeof(IDisposable), typeof(APresenter)).To<MainMenuPresenter>()
                .FromNew()
                .AsSingle()
                .Lazy();
            
            Container.Bind(typeof(IDisposable), typeof(APresenter)).To<GameMenuPresenter>()
                .FromNew()
                .AsSingle()
                .Lazy();
        }

        private void BindWindowsDirector()
        {
            Container.BindInterfacesAndSelfTo<UIDirector>()
                .FromNew()
                .AsSingle()
                .NonLazy();
        }
    }
}