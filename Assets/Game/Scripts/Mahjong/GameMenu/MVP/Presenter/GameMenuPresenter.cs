using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.Common.MVP;
using Game.Scripts.Common.UI.WindowsDirector;
using Game.Scripts.Mahjong.GameMenu.Board.TileMatching;
using Game.Scripts.Mahjong.MainMenu.MVP.Presenter;
using Game.Scripts.MahjongSystem.Model;
using Game.Scripts.MahjongSystem.View;
using UnityEngine;

namespace Game.Scripts.Mahjong.GameMenu.MVP.Presenter
{
    public class GameMenuPresenter : WindowPresenter<IGameBoardView, IGameBoardModel>, IDisposable
    {
        private CancellationTokenSource _autoCompleteCts;
        
        public GameMenuPresenter(
            IGameBoardView view, 
            IGameBoardModel model,
            IWindowsDirector windowsDirector
            ) : base(view, model, windowsDirector)
        { }

        protected override void OnEnable()
        {
            GenerateLevel();
        }

        protected override void OnDisable() { }

        public void OnBackButtonClicked()
        {
            _windowsDirector.OpenWindow<MainMenuPresenter>();
        }

        public void OnRegenerateLevelButtonClicked()
        {
            Model.RegenerateLevel();
            View.RegenerateLevel(Model.GetBoard());
        }

        public void OnAssistancePairsButtonClicked()
        {
            Model.AssistanceSelectTiles();
        }
        
        public void OnAutoCompleteButtonClicked()
        {
            _autoCompleteCts?.Cancel();
            _autoCompleteCts = new CancellationTokenSource();
            AutoCompleteAsync(_autoCompleteCts.Token).Forget();
        }
        
        public void OnTileClicked(int tileIndex)
        {
            TileMatchingResult result = Model.TrySelectTile(tileIndex);
            switch (result.Result)
            {
                case ESelectTileResult.Selected:
                    break;
                case ESelectTileResult.Deselected:
                    break;
                case ESelectTileResult.Tile_Blocked:
                    View.AnimateTileBlocked(result.TileIndex_1);
                    break;
                case ESelectTileResult.Not_Found:
                    Debug.Log("Not found");
                    break;
                case ESelectTileResult.Dont_Match:
                    Debug.Log("Dont match");
                    break;
                case ESelectTileResult.Remove_Pairs:
                    OnRemovePair(result.TileIndex_1, result.TileIndex_2);
                    break;
                case ESelectTileResult.Level_Finished:
                    OnFinish();
                    break;
            }
        }
        
        private async UniTaskVoid AutoCompleteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Model.AutoCompleteLevel(cancellationToken, (index1, index2) =>
                {
                    OnRemovePair(index1, index2);
                }, OnFinish);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Autocomplete cancelled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Autocomplete failed: {ex.Message}");
            }
        }
        
        private void OnFinish()
        {
            GenerateLevel();
            View.AddScore(200);
        }

        private void OnRemovePair(int tileIndex_1, int tileIndex_2)
        {
            View.AnimateRemovePair(tileIndex_1, tileIndex_2);
            View.AddScore(10);
        }

        private void GenerateLevel()
        {
            Model.InitializeBoard(View.GetBoardSize());
            View.InitializeBoard(Model.GetBoard());
        }

        public void Dispose() { }
    }
}