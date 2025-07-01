using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.Mahjong.GameMenu.Board.Tile;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Mahjong.GameMenu.Board.TileMatching
{
    public enum ESelectTileResult
    {
        Selected,
        Deselected,
        Tile_Blocked,
        Not_Found,
        Dont_Match,
        Remove_Pairs,
        Level_Finished,
    }

    public struct TileMatchingResult
    {
        public ESelectTileResult Result { get; private set; }
        public int TileIndex_1 { get; private set; }
        public int TileIndex_2 { get; private set; }

        public TileMatchingResult(ESelectTileResult result, int tileIndex1, int tileIndex2)
        {
            Result = result;
            TileIndex_1 = tileIndex1;
            TileIndex_2 = tileIndex2;
        }
    }
    
    public class TileMatchingService
    {
        private readonly IBoard _board;
        private readonly List<ITile> _selectedTiles = new(2);
        private (ITile, ITile) _assistancePairs;

        public TileMatchingService(IBoard board)
        {
            _board = board;
        }

        public TileMatchingResult TrySelectTile(int tileIndex)
        {
            ITile tile = _board.GetTileByIndex(tileIndex);
            if (tile == null)
            {
                return new TileMatchingResult(ESelectTileResult.Not_Found, -1, -1);
            }

            if (_board.IsTileBlocked(tile))
            {
                return new TileMatchingResult(ESelectTileResult.Tile_Blocked, tile.GetTileIndex(), -1);
            }

            if (_selectedTiles.Contains(tile))
            {
                tile.DeselectTile();
                _selectedTiles.Remove(tile);
                return new TileMatchingResult(ESelectTileResult.Deselected, tile.GetTileIndex(), -1);
            }

            if (_selectedTiles.Count == 1 && !_selectedTiles[0].GetTileId().Equals(tile.GetTileId()))
            {
                _selectedTiles[0].DeselectTile();
                _selectedTiles[0] = tile;
                tile.SelectTile();
                return new TileMatchingResult(ESelectTileResult.Selected, tile.GetTileIndex(), -1);
            }

            _selectedTiles.Add(tile);
            tile.SelectTile();
            if (_selectedTiles.Count == 2)
            {
                return ProcessSelectedPair();
            }

            return new TileMatchingResult(ESelectTileResult.Selected, tile.GetTileIndex(), -1);
        }

        private TileMatchingResult ProcessSelectedPair()
        {
            var tile1 = _selectedTiles[0];
            var tile2 = _selectedTiles[1];

            int tileIndex1 = tile1.GetTileIndex();
            int tileIndex2 = tile2.GetTileIndex();

            if (tile1.GetTileId() == tile2.GetTileId())
            {
                if ((tile1.Equals(_assistancePairs.Item1) && tile2.Equals(_assistancePairs.Item2)) ||
                    (tile2.Equals(_assistancePairs.Item1) && tile1.Equals(_assistancePairs.Item2)))
                {
                    RemoveAssistancePairs();
                }

                return RemovePair(tile1, tile2);
            }

            _selectedTiles.ForEach(tile => tile.DeselectTile());
            _selectedTiles.Clear();

            return new TileMatchingResult(ESelectTileResult.Dont_Match, tileIndex1, tileIndex2);
        }

        public TileMatchingResult RemovePair(ITile tile1, ITile tile2)
        {
            int tileIndex1 = tile1.GetTileIndex();
            int tileIndex2 = tile2.GetTileIndex();

            _board.ClearContentByIndex(tileIndex1);
            _board.ClearContentByIndex(tileIndex2);

            _selectedTiles.ForEach(tile => tile.DeselectTile());
            _selectedTiles.Clear();

            if (_board.GetAllTiles().Count == 0)
            {
                return new TileMatchingResult(ESelectTileResult.Level_Finished, tileIndex1, tileIndex2);
            }

            return new TileMatchingResult(ESelectTileResult.Remove_Pairs, tileIndex1, tileIndex2);
        }

        public void RemoveAssistancePairs()
        {
            if (_assistancePairs.Item1 != null)
            {
                _assistancePairs.Item1.DeselectTile();
            }
            if (_assistancePairs.Item2 != null)
            {
                _assistancePairs.Item2.DeselectTile();
            }

            _assistancePairs = (null, null);
        }

        public void AssistanceSelectTiles()
        {
            if (_assistancePairs.Item1 != null || _assistancePairs.Item2 != null)
            {
                return;
            }

            var pairs = _board.FindAvailablePairs();
            if (pairs.Count == 0)
            {
                return;
            }
            
            var bestPair = FindBestPair(pairs);
            if (bestPair.Item1 != null && bestPair.Item2 != null)
            {
                bestPair.Item1.AssistanceSelectTile();
                bestPair.Item2.AssistanceSelectTile();
                _assistancePairs = bestPair;
            }
        }

        private (ITile, ITile) FindBestPair(List<(ITile, ITile)> pairs)
        {
            if (pairs.Count == 0)
            {
                return (null, null);
            }

            (ITile, ITile) bestPair = pairs[0];
            int bestScore = 0;

            foreach (var pair in pairs)
            {
                int score = CalculatePairScore(pair.Item1, pair.Item2);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPair = pair;
                }
            }

            return bestPair;
        }

        private int CalculatePairScore(ITile tile1, ITile tile2)
        {
            var tempContents = new Dictionary<int, ITile>(_board.GetAllTiles().ToDictionary(t => t.GetTileIndex(), t => t));
            
            _board.ClearContentByIndex(tile1.GetTileIndex());
            _board.ClearContentByIndex(tile2.GetTileIndex());
            
            int newPairsCount = _board.FindAvailablePairs().Count;
            
            foreach (var tile in tempContents)
            {
                _board.AddTile(tile.Value);
            }

            return newPairsCount;
        }
        
        public async UniTask AutoCompleteLevel(CancellationToken cancellationToken, Action<int, int> onRemovePair = null, Action onFinish = null)
        {
            while (_board.GetAllTiles().Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                var pairs = _board.FindAvailablePairs();
                if (pairs.Count == 0)
                {
                    break;
                }

                var pair = FindBestPair(pairs);
                if (pair.Item1 == null || pair.Item2 == null)
                {
                    break;
                }
                
                int tileIndex1 = pair.Item1.GetTileIndex();
                int tileIndex2 = pair.Item2.GetTileIndex();
                
                var result = RemovePair(pair.Item1, pair.Item2);

                onRemovePair?.Invoke(tileIndex1, tileIndex2);
                
                await UniTask.Delay(1000, cancellationToken: cancellationToken);

                if (result.Result == ESelectTileResult.Level_Finished)
                {
                    onFinish?.Invoke();
                    break;
                }
            }
        }
    }
}