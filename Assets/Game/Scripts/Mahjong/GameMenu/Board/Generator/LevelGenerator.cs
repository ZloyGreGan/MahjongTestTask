using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Common.Database.Resources;
using Game.Scripts.Mahjong.GameMenu.Board.Tile;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Mahjong.GameMenu.Board.Generator
{
    public interface ILevelGenerator
    {
        void GenerateLevel(IBoard board);
        void RegenerateLevel(IBoard board);
    }
    
    public class LevelGenerator : ILevelGenerator
    {
        private readonly IInstantiator _instantiator;
        private readonly IResourceDatabase _resourceDatabase;
        
        public LevelGenerator(
            IInstantiator instantiator,
            IResourceDatabase resourceDatabase
            )
        {
            _instantiator = instantiator;
            _resourceDatabase = resourceDatabase;
        }
        
        public void GenerateLevel(IBoard board)
        {
            GameObject tilePrefab = _resourceDatabase.GetResourcesByCategory<GameObject>(GameStringConstants.RESOURCES_TILE_PREFAB)[0];
            
            List<(int, Sprite)> sprites = CreateTilePool(board);
            var availablePositions = board.GetBoardSize().x * board.GetBoardSize().y;

            for (int index = 0; index < availablePositions; index++)
            {
                int spriteIndex = Random.Range(0, sprites.Count);
                var (tileId, sprite) = sprites[spriteIndex];
                sprites.RemoveAt(spriteIndex);

                ITile tile = _instantiator.InstantiatePrefabForComponent<ITile>(tilePrefab);
                tile.Initialize(index, tileId);
                tile.SetIcon(sprite);
                board.AddTile(tile);
            }
        }

        public void RegenerateLevel(IBoard board)
        {
            List<ITile> tiles = board.GrabAllTiles();
            var availablePositions = board.GetBoardSize().x * board.GetBoardSize().y;
            
            List<int> indices = new List<int>(availablePositions);
            for (int i = 0; i < availablePositions; i++)
            {
                indices.Add(i);
            }
            
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            for (int i = 0; i < tiles.Count && i < indices.Count; i++)
            {
                ITile tile = tiles[i];
                
                int index = indices[i];

                tile.Initialize(index, tile.GetTileId());
                board.AddTile(tile);
            }
        }
        
        private List<(int, Sprite)> CreateTilePool(IBoard board)
        {
            int totalTiles = board.GetBoardSize().x * board.GetBoardSize().y;
            int uniqueTypes = totalTiles / 4;
            int copiesPerType = 4;

            var tileTypesDatabase = _resourceDatabase.GetResourcesByCategory<Sprite>(GameStringConstants.RESOURCES_TILE_ICONS);
            var sprites = new List<(int, Sprite)>(totalTiles);
            int tileId = 0;
            
            var availableTextures = new List<Sprite>(tileTypesDatabase);
            for (int i = 0; i < uniqueTypes && availableTextures.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, availableTextures.Count);
                Sprite sprite = availableTextures[randomIndex];
                availableTextures.RemoveAt(randomIndex);
                
                for (int j = 0; j < copiesPerType; j++)
                {
                    sprites.Add((tileId, sprite));
                }
                
                tileId++;
            }
            
            for (int i = sprites.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (sprites[i], sprites[j]) = (sprites[j], sprites[i]);
            }

            return sprites;
        }
    }
}