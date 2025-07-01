using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Mahjong.GameMenu.Board.Tile;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Scripts.Mahjong.GameMenu.Board
{
    class ItemContent
    {
        public ITile Tile { get; }
        public RectTransform RectTransform { get; }

        public ItemContent(ITile tile)
        {
            Tile = tile;
            RectTransform = Tile.MonoBehaviour().GetComponent<RectTransform>();
        }
    }
    
    [RequireComponent(typeof(RectTransform))]
    public class CustomGridLayoutGroup : LayoutGroup
    {
        public Vector2Int GetSize => _size;
        
        [SerializeField] private Vector2 _cellSize = new(100f, 100f);
        [SerializeField] private Vector2 _spacing = Vector2.zero;
        [SerializeField] private int _maxColumns = 3;
        [SerializeField] private int _maxRows = 3;
        [SerializeField] private bool _scaleCells = true;
        [SerializeField] private Vector2 _scalePadding = Vector2.zero;
        [SerializeField] private bool _debugDrawCells = false;

        private readonly Dictionary<int, ItemContent> _cellContents = new();
        private Vector2[] _cellPositions;
        private Vector2 _scaledCellSize;
        private Vector2Int _size;
        
        private bool _isDirty = true;

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalculateCellPositions();
            _isDirty = true;
            UpdateCellContents();
        }

        public override void CalculateLayoutInputVertical() { }

        public override void SetLayoutHorizontal()
        {
            if (_isDirty)
            {
                UpdateCellContents();
            }
        }

        public override void SetLayoutVertical()
        {
            if (_isDirty)
            {
                UpdateCellContents();
            }
        }

        private void CalculateCellPositions()
        {
            int cellCount = CalculateCellCount();
            _cellPositions = new Vector2[cellCount];

            int columns = _maxColumns;
            int rows = _maxRows;
            
            _scaledCellSize = _scaleCells ? CalculateScaledCellSize(columns, rows) : _cellSize;
            
            Vector2 alignmentOffset = CalculateAlignmentOffset(columns, rows);

            for (int i = 0; i < cellCount; i++)
            {
                int row = i / columns;
                int column = i % columns;
                
                float xPos = rectTransform.rect.x + column * (_scaledCellSize.x + _spacing.x) + alignmentOffset.x;
                float yPos = -rectTransform.rect.y + -row * (_scaledCellSize.y + _spacing.y) + alignmentOffset.y;
                
                _cellPositions[i] = new Vector2(xPos, yPos);
            }
        }

        private Vector2 CalculateScaledCellSize(int columns, int rows)
        {
            Vector2 containerSize = rectTransform.rect.size;
            containerSize -= _scalePadding;

            float cellWidth = (containerSize.x - _spacing.x * (columns - 1)) / columns;
            float cellHeight = (containerSize.y - _spacing.y * (rows - 1)) / rows;
            
            float aspectRatio = _cellSize.x / _cellSize.y;
            if (cellWidth / cellHeight > aspectRatio)
            {
                cellWidth = cellHeight * aspectRatio;
            }
            else
            {
                cellHeight = cellWidth / aspectRatio;
            }

            return new Vector2(cellWidth, cellHeight);
        }

        private Vector2 CalculateAlignmentOffset(int columns, int rows)
        {
            Vector2 containerSize = rectTransform.rect.size;
            float gridWidth = columns * _scaledCellSize.x + (columns - 1) * _spacing.x;
            float gridHeight = rows * _scaledCellSize.y + (rows - 1) * _spacing.y;

            Vector2 offset = Vector2.zero;
            
            TextAnchor alignment = childAlignment;
            
            if (alignment == TextAnchor.UpperLeft || alignment == TextAnchor.MiddleLeft || alignment == TextAnchor.LowerLeft)
            {
                offset.x = 0f;
            }
            else if (alignment == TextAnchor.UpperCenter || alignment == TextAnchor.MiddleCenter || alignment == TextAnchor.LowerCenter)
            {
                offset.x = (containerSize.x - gridWidth) / 2f;
            }
            else if (alignment == TextAnchor.UpperRight || alignment == TextAnchor.MiddleRight || alignment == TextAnchor.LowerRight)
            {
                offset.x = containerSize.x - gridWidth;
            }

            if (alignment == TextAnchor.UpperLeft || alignment == TextAnchor.UpperCenter || alignment == TextAnchor.UpperRight)
            {
                offset.y = 0f;
            }
            else if (alignment == TextAnchor.MiddleLeft || alignment == TextAnchor.MiddleCenter || alignment == TextAnchor.MiddleRight)
            {
                offset.y = -(containerSize.y - gridHeight) / 2f;
            }
            else if (alignment == TextAnchor.LowerLeft || alignment == TextAnchor.LowerCenter || alignment == TextAnchor.LowerRight)
            {
                offset.y = - (containerSize.y - gridHeight);
            }

            return offset;
        }

        private void UpdateCellContents()
        {
            foreach (var pair in _cellContents)
            {
                int index = pair.Key;
                ItemContent t = pair.Value;

                if (index < 0 || index >= _cellPositions.Length || t == null)
                    continue;

                t.RectTransform.localPosition = new Vector3(_cellPositions[index].x, _cellPositions[index].y, 0);
                t.RectTransform.sizeDelta = _scaledCellSize;
            }
            
            _isDirty = false;
        }
        
        public void PlaceInCell(int cellIndex, ITile tile)
        {
            if (cellIndex < 0 || cellIndex >= _maxColumns * _maxRows)
            {
                Debug.LogWarning($"Cell index {cellIndex} is out of range (max: {_maxColumns * _maxRows}).");
                return;
            }

            if (tile != null)
            {
                ItemContent content = new ItemContent(tile);
                content.RectTransform.SetParent(rectTransform, false);
                content.Tile.MonoBehaviour().gameObject.SetActive(true);
                _cellContents[cellIndex] = content;
                _isDirty = true;
                CalculateCellPositions();
                UpdateCellContents();
            }
        }
        
        public void ClearCell(int cellIndex)
        {
            if (_cellContents.ContainsKey(cellIndex))
            {
                Destroy(GetCellContent(cellIndex).MonoBehaviour().gameObject);
                _cellContents.Remove(cellIndex);
                _isDirty = true;
                CalculateCellPositions();
                UpdateCellContents();
            }
        }

        public void ClearAllCells()
        {
            foreach (ItemContent itemContent in _cellContents.Values)
            {
                Destroy(itemContent.RectTransform.gameObject);
            }
            
            _cellContents.Clear();
            _isDirty = true;
            CalculateCellPositions();
            UpdateCellContents();
        }

        public List<ITile> GrabAllCells()
        {
            List<ITile> grabbedTiles = GetCellContents();
            
            foreach (ItemContent itemContent in _cellContents.Values)
            {
                itemContent.RectTransform.SetParent(rectTransform);
                itemContent.Tile.MonoBehaviour().gameObject.SetActive(false);
            }
            
            _cellContents.Clear();
            _isDirty = true;
            CalculateCellPositions();
            UpdateCellContents();

            return grabbedTiles;
        }
        
        public ITile GetCellContent(int cellIndex)
        {
            return _cellContents.Values.FirstOrDefault(item => item.Tile.GetTileIndex() == cellIndex)?.Tile;
        }

        public List<ITile> GetCellContents()
        {
            return _cellContents.Values.Select(content => content.Tile).ToList();
        }
        
        public int CalculateCellCount()
        {
            return _maxColumns * _maxRows;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            CalculateCellPositions();
            _isDirty = true;
            UpdateCellContents();

            _size = new Vector2Int(_maxColumns, _maxRows);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _maxColumns = Mathf.Max(1, _maxColumns);
            _maxRows = Mathf.Max(1, _maxRows);
            CalculateCellPositions();
            _isDirty = true;
            UpdateCellContents();
        }

        private void OnDrawGizmos()
        {
            if (!_debugDrawCells || _cellPositions == null || _cellPositions.Length == 0)
                return;

            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            Matrix4x4 originalMatrix = Gizmos.matrix;
            
            Gizmos.matrix = rectTransform.localToWorldMatrix;

            for (int i = 0; i < _cellPositions.Length; i++)
            {
                Vector3 cellPos = new Vector3(_cellPositions[i].x, _cellPositions[i].y, 0f);
                Vector3 size = new Vector3(_scaledCellSize.x, _scaledCellSize.y, 0f);
                Vector3 cubeCenter = cellPos + new Vector3(_scaledCellSize.x / 2f, -_scaledCellSize.y / 2f, 0f);
                
                Gizmos.DrawWireCube(cubeCenter, size);

                Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
                Gizmos.DrawWireSphere(cellPos, 5f);
                Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            }

            Gizmos.matrix = originalMatrix;
        }
#endif
    }
}