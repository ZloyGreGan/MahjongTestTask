using UnityEngine;

namespace Game.Scripts.Mahjong.GameMenu.Board.Tile
{
    public interface ITileBlockRule
    {
        bool IsTileBlocked(ITile tile, IBoard board);
    }
    
    public class HorizontalTileBlockRule : ITileBlockRule
    {
        public bool IsTileBlocked(ITile tile, IBoard board)
        {
            int index = tile.GetTileIndex();
            Vector2Int boardSize = board.GetBoardSize();
            int col = index % boardSize.x;
            
            if (col == 0 || col == boardSize.x - 1)
            {
                return false;
            }

            ITile tileLeft = board.GetTileByIndex(index - 1);
            ITile tileRight = board.GetTileByIndex(index + 1);

            return tileLeft != null && tileRight != null;
        }
    }
}