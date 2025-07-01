using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.Common.Database.Resources;
using Game.Scripts.Common.MVP;
using Game.Scripts.Mahjong.GameMenu.Board;
using Game.Scripts.Mahjong.GameMenu.Board.Generator;
using Game.Scripts.Mahjong.GameMenu.Board.TileMatching;
using UnityEngine;
using Zenject;

namespace Game.Scripts.MahjongSystem.Model
{
    public interface IGameBoardModel : IModel
    {
        void InitializeBoard(Vector2Int boardSize);
        void RegenerateLevel();
        
        TileMatchingResult TrySelectTile(int tileIndex);
        void AssistanceSelectTiles();
        UniTask AutoCompleteLevel(CancellationToken cancellationToken, Action<int, int> onRemovePair = null, Action onFinish = null);
        
        IBoard GetBoard();
    }
    
    public class GameMenuModel : IGameBoardModel
    {
        private IBoard _board;
        private ILevelGenerator _levelGenerator;
        private TileMatchingService _matchingService;
        
        public GameMenuModel(
            IInstantiator instantiator,
            IResourceDatabase resourceDatabase
            )
        {
            _levelGenerator = new LevelGenerator(instantiator, resourceDatabase);
        }

        public void InitializeBoard(Vector2Int boardSize)
        {
            _board = new GameBoard(boardSize);
            _levelGenerator.GenerateLevel(_board);
            _matchingService = new TileMatchingService(_board);
        }

        public void RegenerateLevel()
        {
            _levelGenerator.RegenerateLevel(_board);
            _matchingService.RemoveAssistancePairs();
        }

        public TileMatchingResult TrySelectTile(int tileIndex)
        {
            return _matchingService.TrySelectTile(tileIndex);
        }

        public void AssistanceSelectTiles()
        {
            _matchingService.AssistanceSelectTiles();
        }
        
        public async UniTask AutoCompleteLevel(CancellationToken cancellationToken, Action<int, int> onRemovePair = null, Action onFinish = null)
        {
            await _matchingService.AutoCompleteLevel(cancellationToken, onRemovePair, onFinish);
        }

        public IBoard GetBoard()
        {
            return _board;
        }
    }
}