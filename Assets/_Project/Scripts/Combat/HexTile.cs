using UnityEngine;

namespace Kervan.Combat
{
    public enum TileHighlightState
    {
        None,
        Reachable,      // Yürüme menzili (Turkuaz)
        AttackTarget,   // Saldırı menzili (Koyu Al)
        Selected,       // Seçili altıgen (Altın)
        PathPreview     // Yol rotası (Kehribar Sarısı)
    }

    /// <summary>
    /// Savaş alanındaki tek bir altıgen karoyu ve üzerindeki engel/vurgu görsellerini temsil eden bileşen.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HexTile : MonoBehaviour
    {
        [Header("Konum & Durum")]
        [SerializeField] private HexCoordinates _coordinates;
        [SerializeField] private bool _isWalkable = true;
        [SerializeField] private int _movementCost = 1;

        [Header("Görsel Bileşenler")]
        [SerializeField] private SpriteRenderer _tileRenderer;
        [SerializeField] private SpriteRenderer _highlightRenderer;
        [SerializeField] private SpriteRenderer _obstacleRenderer;

        // Üzerindeki aktif savaş birimi
        public CombatUnit OccupyingUnit { get; set; }

        public HexCoordinates Coordinates => _coordinates;
        public bool IsWalkable => _isWalkable && OccupyingUnit == null;
        public bool HasObstacle => !_isWalkable;
        public int MovementCost => _movementCost;

        // GDD §2 & §3 standart renk kodları
        private static readonly Color ColorBaseGrid = new Color(1.0f, 0.96f, 0.88f, 0.24f);          // Hafif taktiksel ızgara konturu
        private static readonly Color ColorObstacleBase = new Color(0.28f, 0.22f, 0.18f, 0.45f);     // Engel altı karartma
        private static readonly Color ColorReachable = new Color(0.0f, 0.545f, 0.608f, 0.65f);       // #008B9B Selçuklu Turkuazı
        private static readonly Color ColorAttack = new Color(0.68f, 0.16f, 0.12f, 0.80f);           // #8B261E Koyu Al
        private static readonly Color ColorSelected = new Color(0.855f, 0.647f, 0.125f, 0.85f);      // #DAA520 Altın
        private static readonly Color ColorPath = new Color(1.0f, 0.84f, 0.25f, 0.85f);              // Sarı Rota İzi

        private void Awake()
        {
            EnsureRenderers();
        }

        private void EnsureRenderers()
        {
            if (_tileRenderer == null)
            {
                _tileRenderer = GetComponent<SpriteRenderer>();
            }

            if (_highlightRenderer == null)
            {
                Transform highlightChild = transform.Find("Highlight");
                if (highlightChild != null)
                {
                    _highlightRenderer = highlightChild.GetComponent<SpriteRenderer>();
                }
            }

            if (_obstacleRenderer == null)
            {
                Transform obstacleChild = transform.Find("ObstacleProp");
                if (obstacleChild != null)
                {
                    _obstacleRenderer = obstacleChild.GetComponent<SpriteRenderer>();
                }
            }
        }

        public void Initialize(HexCoordinates coords, Sprite frameSprite = null, bool isWalkable = true)
        {
            EnsureRenderers();

            _coordinates = coords;
            _isWalkable = isWalkable;
            name = $"Tile_{coords.Q}_{coords.R}";
            transform.position = coords.ToWorldPosition();

            if (_tileRenderer != null)
            {
                if (frameSprite != null)
                {
                    _tileRenderer.sprite = frameSprite;
                }
                _tileRenderer.sortingLayerName = "HexGrid";
                _tileRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10f);
                _tileRenderer.color = _isWalkable ? ColorBaseGrid : ColorObstacleBase;
            }

            if (_highlightRenderer != null)
            {
                if (frameSprite != null && _highlightRenderer.sprite == null)
                {
                    _highlightRenderer.sprite = frameSprite;
                }
                _highlightRenderer.sortingLayerName = "GridHighlights";
                _highlightRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10f) + 1;
            }

            SetHighlight(TileHighlightState.None);
        }

        public void SetHighlight(TileHighlightState state)
        {
            if (_highlightRenderer == null) return;

            switch (state)
            {
                case TileHighlightState.None:
                    _highlightRenderer.enabled = false;
                    break;

                case TileHighlightState.Reachable:
                    _highlightRenderer.enabled = true;
                    _highlightRenderer.color = ColorReachable;
                    break;

                case TileHighlightState.AttackTarget:
                    _highlightRenderer.enabled = true;
                    _highlightRenderer.color = ColorAttack;
                    break;

                case TileHighlightState.Selected:
                    _highlightRenderer.enabled = true;
                    _highlightRenderer.color = ColorSelected;
                    break;

                case TileHighlightState.PathPreview:
                    _highlightRenderer.enabled = true;
                    _highlightRenderer.color = ColorPath;
                    break;
            }
        }

        /// <summary>
        /// Karoyu taktiksel siper/engel (kaya, barikat vb.) durumuna geçirir ve engel görselini yerleştirir.
        /// </summary>
        public void SetObstacle(bool isObstacle, Sprite obstacleSprite = null)
        {
            EnsureRenderers();
            _isWalkable = !isObstacle;

            if (_tileRenderer != null)
            {
                _tileRenderer.color = isObstacle ? ColorObstacleBase : ColorBaseGrid;
            }

            if (isObstacle && obstacleSprite != null)
            {
                if (_obstacleRenderer == null)
                {
                    GameObject propObj = new GameObject("ObstacleProp");
                    propObj.transform.SetParent(transform, false);
                    propObj.transform.localPosition = new Vector3(0f, -0.08f, 0f);
                    _obstacleRenderer = propObj.AddComponent<SpriteRenderer>();
                }

                _obstacleRenderer.sprite = obstacleSprite;
                _obstacleRenderer.sortingLayerName = "Entities";
                _obstacleRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100f);
                _obstacleRenderer.enabled = true;
            }
            else if (_obstacleRenderer != null)
            {
                _obstacleRenderer.enabled = false;
            }
        }
    }
}
