using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Scripts.Mahjong.GameMenu.Board.Tile
{
    public interface ITile
    {
        int GetTileIndex();
        int GetTileId();
        bool IsBlocked(IBoard board);
        MonoBehaviour MonoBehaviour();

        void Initialize(int tileIndex, int tileId);
        void SetIcon(Sprite sprite);
        void SelectTile();
        void DeselectTile();
        void AssistanceSelectTile();
    }
    
    public class Tile : MonoBehaviour, ITile, IPointerClickHandler
    {
        public Action<ITile> OnClick;
        
        [SerializeField] private Image _icon;
        [SerializeField] private Image _background;

        [Space] 
        [SerializeField] private Sprite _unselectedSprite;
        [SerializeField] private Sprite _selectedSprite;
        [SerializeField] private Sprite _assistanceSelectedSprite;
        
        public int GetTileIndex() => _tileIndex;
        public int GetTileId() => _tileId;
        public MonoBehaviour MonoBehaviour() => this;

        private int _tileIndex;
        private int _tileId;
        private bool _isAssisted;

        public void Initialize(int tileIndex, int tileId)
        {
            _tileIndex = tileIndex;
            _tileId = tileId;
            
            _isAssisted = false;
            DeselectTile();
        }

        public bool IsBlocked(IBoard board)
        {
            return board.IsTileBlocked(this);
        }

        public void SetIcon(Sprite sprite)
        {
            _icon.sprite = sprite;
        }

        public void SelectTile()
        {
            _background.sprite = _selectedSprite;
        }

        public void DeselectTile()
        {
            if (!_isAssisted)
            {
                _background.sprite = _unselectedSprite;
            }
            else
            {
                AssistanceSelectTile();
            }
        }

        public void AssistanceSelectTile()
        {
            _isAssisted = true;
            _background.sprite = _assistanceSelectedSprite;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this);
        }
    }
}