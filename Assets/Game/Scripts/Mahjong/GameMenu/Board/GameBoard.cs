using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Mahjong.GameMenu.Board.Tile;
using UnityEngine;

namespace Game.Scripts.Mahjong.GameMenu.Board
{
    public interface IBoard
    {
        void AddTile(ITile tile);
        List<ITile> GrabAllTiles();

        bool IsTileBlocked(ITile tile);
        
        List<ITile> GetAllTiles();
        ITile GetTileByIndex(int index);
        void ClearContentByIndex(int index);
        
        List<(ITile, ITile)> FindAvailablePairs();
        Vector2Int GetBoardSize();
    }
    
    public class GameBoard : IBoard
    {
        public Vector2Int GetBoardSize() => _boardSize;
        
        private readonly Dictionary<int, ITile> _tiles = new();
        private readonly Vector2Int _boardSize;
        private readonly List<ITileBlockRule> _blockRules;
        
        private List<ITile> _cachedTiles = new();
        private bool _tilesDirty = true;
        
        public GameBoard(Vector2Int boardSize)
        {
            _boardSize = boardSize;
            _blockRules = new List<ITileBlockRule> { new HorizontalTileBlockRule() };
        }

        public void AddTile(ITile tile)
        {
            if (tile is not Tile.Tile tileCast) return;
            
            _tiles[tileCast.GetTileIndex()] = tileCast;
            _tilesDirty = true;
        }

        public void ClearContentByIndex(int index)
        {
            _tiles[index] = null;
            _tilesDirty = true;
        }

        public List<ITile> GrabAllTiles()
        {
            var grabbedTiles = _tiles.Values.Where(tile => tile != null).ToList();
            _tiles.Clear();
            _cachedTiles.Clear();
            _tilesDirty = true;
            return grabbedTiles;
        }

        public bool IsTileBlocked(ITile tile)
        {
            if (tile == null) return false;
            
            return _blockRules.Any(rule => rule.IsTileBlocked(tile, this));
        }

        public ITile GetTileByIndex(int index)
        {
            return _tiles.GetValueOrDefault(index);
        }

        public List<ITile> GetAllTiles()
        {
            if (_tilesDirty)
            {
                _cachedTiles = _tiles.Values.Where(t => t != null).ToList();
                _tilesDirty = false;
            }
            return _cachedTiles;
        }

        public List<(ITile, ITile)> FindAvailablePairs()
        {
            List<ITile> tiles = GetAllTiles();
            var pairs = new List<(ITile, ITile)>();
            
            var tileGroups = tiles
                .Where(tile => tile != null)
                .GroupBy(t => t.GetTileId())
                .Where(g => g.Count() >= 2)
                .ToList();
            
            foreach (var group in tileGroups)
            {
                var groupTiles = group.ToList();
                for (int i = 0; i < groupTiles.Count; i++)
                {
                    var tile1 = groupTiles[i];
                    if (tile1.IsBlocked(this)) continue;

                    for (int j = i + 1; j < groupTiles.Count; j++)
                    {
                        var tile2 = groupTiles[j];
                        if (!tile2.IsBlocked(this))
                        {
                            pairs.Add((tile1, tile2));
                        }
                    }
                }
            }

            return pairs;
        }
    }
}