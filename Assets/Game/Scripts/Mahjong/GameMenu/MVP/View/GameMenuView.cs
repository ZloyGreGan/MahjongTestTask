using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.Scripts.Common.MVP;
using Game.Scripts.Mahjong.GameMenu.Board;
using Game.Scripts.Mahjong.GameMenu.Board.Tile;
using Game.Scripts.Mahjong.GameMenu.MVP.Presenter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.MahjongSystem.View
{
    public interface IGameBoardView : IView
    {
        void InitializeBoard(IBoard board);
        void RegenerateLevel(IBoard board);
        void RemovePair(int index1, int index2);
        
        void AddScore(int score);

        void AnimateTileBlocked(int index);
        void AnimateRemovePair(int index1, int index2);
        
        Vector2Int GetBoardSize();
    }
    
    public class GameMenuView : MonoBehaviour, IGameBoardView
    {
        public Vector2Int GetBoardSize() => _layout.GetSize;
        
        public bool IsActive => gameObject.activeSelf;
        public MonoBehaviour monoBehaviour => this;
        
        [SerializeField] private CustomGridLayoutGroup _layout;
        
        [Space]
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _scoreText;
        
        [Space]
        [SerializeField] private Button _regenerateLevelButton;
        [SerializeField] private Button _autoCompleteButton;
        [SerializeField] private Button _assistanceButton;

        private GameMenuPresenter _presenter;
        private readonly List<Tile> _subscribedTiles = new();
        private int _score;
        
        public void Init(APresenter presenter)
        {
            _presenter = (GameMenuPresenter) presenter;
        }

        private void OnEnable()
        {
            _backButton.onClick.AddListener(() => _presenter.OnBackButtonClicked());
            
            _regenerateLevelButton.onClick.AddListener(() => _presenter.OnRegenerateLevelButtonClicked());
            _autoCompleteButton.onClick.AddListener(() => _presenter.OnAutoCompleteButtonClicked());
            _assistanceButton.onClick.AddListener(() => _presenter.OnAssistancePairsButtonClicked());
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveAllListeners();
            
            _regenerateLevelButton.onClick.RemoveAllListeners();
            _autoCompleteButton.onClick.RemoveAllListeners();
            _assistanceButton.onClick.RemoveAllListeners();
            
            UnsubscribeFromTiles();
        }

        public void InitializeBoard(IBoard board)
        {
            UnsubscribeFromTiles();
            
            _layout.ClearAllCells();
            SetTilesBoard(board);
            
            foreach (var iTile in board.GetAllTiles())
            {
                if (iTile is Tile tileCast)
                {
                    tileCast.OnClick += HandleTileClick;
                    _subscribedTiles.Add(tileCast);
                }
            }
        }

        public void RegenerateLevel(IBoard board)
        {
            _layout.GrabAllCells();
            SetTilesBoard(board);
        }
        
        public void RemovePair(int index1, int index2)
        {
            _layout.ClearCell(index1);
            _layout.ClearCell(index2);
        }

        public void AddScore(int score)
        {
            _score += score;
            _scoreText.text = $"Cчет\n {_score}";
        }

        public void AnimateTileBlocked(int index)
        {
            const float SHAKE_DURATION = 0.3f;
            const float SHAKE_STRENGTH = 10f;
            const int SHAKE_VIBRATO = 10;

            ITile tile = _layout.GetCellContent(index);

            if (tile == null)
            {
                return;
            }

            RectTransform rect = tile.MonoBehaviour().GetComponent<RectTransform>();
            
            Vector2 originalPivot = rect.pivot;
            rect.pivot = new Vector2(0f, 1f);
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(rect.DOPunchPosition(new Vector3(SHAKE_STRENGTH, SHAKE_STRENGTH, 0), SHAKE_DURATION, SHAKE_VIBRATO, 0.5f))
                .AppendCallback(() =>
                {
                    rect.pivot = originalPivot;
                })
                .SetUpdate(true);

            sequence.Play();
        }
        
        public void AnimateRemovePair(int index1, int index2)
        {
            const float MOVE_DURATION = 0.3f;
            const float COLLISION_SCALE = 1.3f;
            const float PUNCH_DURATION = 0.15f;
            const float FADE_DURATION = 0.2f;
            
            ITile tile1 = _layout.GetCellContent(index1);
            ITile tile2 = _layout.GetCellContent(index2);

            if (tile1 == null || tile2 == null)
            {
                _layout.ClearCell(index1);
                _layout.ClearCell(index2);
                return;
            }

            RectTransform rect1 = tile1.MonoBehaviour().GetComponent<RectTransform>();
            RectTransform rect2 = tile2.MonoBehaviour().GetComponent<RectTransform>();
            
            Vector2 originalPivot1 = rect1.pivot;
            Vector2 originalPivot2 = rect2.pivot;
            rect1.pivot = new Vector2(0.5f, 0.5f);
            rect2.pivot = new Vector2(0.5f, 0.5f);
            
            int originalSiblingIndex1 = rect1.GetSiblingIndex();
            int originalSiblingIndex2 = rect2.GetSiblingIndex();
            rect1.SetAsLastSibling();
            rect2.SetAsLastSibling();
            
            Vector3 screenCenter = _layout.GetComponent<RectTransform>().rect.center;
            Vector3 centerWorldPos = _layout.transform.TransformPoint(screenCenter);
            Vector3 centerLocalPos1 = rect1.parent.InverseTransformPoint(centerWorldPos);
            Vector3 centerLocalPos2 = rect2.parent.InverseTransformPoint(centerWorldPos);

            Sequence sequence = DOTween.Sequence();
            
            sequence.Append(rect1.DOLocalMove(centerLocalPos1, MOVE_DURATION).SetEase(Ease.InOutQuad))
                   .Join(rect2.DOLocalMove(centerLocalPos2, MOVE_DURATION).SetEase(Ease.InOutQuad))
                   .Join(rect1.DOScale(Vector3.one * COLLISION_SCALE, MOVE_DURATION).SetEase(Ease.OutQuad))
                   .Join(rect2.DOScale(Vector3.one * COLLISION_SCALE, MOVE_DURATION).SetEase(Ease.OutQuad))
                   .Append(rect1.DOPunchScale(Vector3.one * 0.2f, PUNCH_DURATION, 2, 0.5f))
                   .Join(rect2.DOPunchScale(Vector3.one * 0.2f, PUNCH_DURATION, 2, 0.5f))
                   .Append(rect1.DOScale(Vector3.zero, FADE_DURATION).SetEase(Ease.InBack))
                   .Join(rect2.DOScale(Vector3.zero, FADE_DURATION).SetEase(Ease.InBack))
                   .Join(rect1.DORotate(new Vector3(0, 0, 360), FADE_DURATION, RotateMode.FastBeyond360))
                   .Join(rect2.DORotate(new Vector3(0, 0, -360), FADE_DURATION, RotateMode.FastBeyond360))
                   .AppendCallback(() =>
                   {
                       rect1.pivot = originalPivot1;
                       rect2.pivot = originalPivot2;
                       rect1.SetSiblingIndex(originalSiblingIndex1);
                       rect2.SetSiblingIndex(originalSiblingIndex2);
                       _layout.ClearCell(index1);
                       _layout.ClearCell(index2);
                   })
                   .SetUpdate(true);

            sequence.Play();
        }
        
        private void HandleTileClick(ITile tile)
        {
            _presenter.OnTileClicked(tile.GetTileIndex());
        }

        private void SetTilesBoard(IBoard board)
        {
            foreach (var iTile in board.GetAllTiles())
            {
                _layout.PlaceInCell(iTile.GetTileIndex(), iTile);
            }
        }
        
        private void UnsubscribeFromTiles()
        {
            foreach (var tile in _subscribedTiles)
            {
                if (tile != null)
                {
                    tile.OnClick -= HandleTileClick;
                }
            }
            
            _subscribedTiles.Clear();
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }
    }
}