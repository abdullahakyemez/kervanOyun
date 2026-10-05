using UnityEngine;

namespace Kervan.Combat
{
    public enum TileHighlightState
    {
        None,
        Reachable,      // Yürüme menzili (Turkuaz)
        AttackTarget,   // Saldırı menzili (Kırmızı)
        Selected,       // Seçili altıgen (Altın)
        PathPreview     // Yol rotası (Sarı)
    }

    /// <summary>
    /// Savas alanindaki tek bir altigen karoyu temsil eden bilesen.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HexTile : MonoBehaviour
    {
        [Header("Konum & Durum")]
        [SerializeField] private HexCoordinates _coordinates;
        [SerializeField] private bool _isWalkable = true;
        [SerializeField] private int _movementCost = 1;

        [Header("Gorsel Bilesenler")]
        [SerializeField] private SpriteRenderer _tileRenderer;
        [SerializeField] private SpriteRenderer _highlightRenderer;

        // Uzerindeki aktif savas birimi
        public CombatUnit OccupyingUnit { get; set; }

        public HexCoordinates Coordinates => _coordinates;
        public bool IsWalkable => _isWalkable && OccupyingUnit == null;
        public bool HasObstacle => !_isWalkable;
        public int MovementCost => _movementCost;

        // TASARİM.md standart renk kodlari
        private static readonly Color ColorReachable = new Color(0.0f, 0.545f, 0.608f, 0.40f);    // #008B9B Turkuaz
        private static readonly Color ColorAttack = new Color(0.545f, 0.149f, 0.118f, 0.45f);       // #8B261E Koyu Al
        private static readonly Color ColorSelected = new Color(0.855f, 0.647f, 0.125f, 0.50f);     // #DAA520 Altin
        private static readonly Color ColorPath = new Color(1.0f, 0.90f, 0.30f, 0.60f);            // Sarimsi Yol

        private void Awake()
        {
            if (_tileRenderer == null)
            {
                _tileRenderer = GetComponent<SpriteRenderer>();
            }

            // Vurgu katmani yoksa alt nesne olarak olustur
            if (_highlightRenderer == null)
            {
                Transform highlightChild = transform.Find("Highlight");
                if (highlightChild != null)
                {
                    _highlightRenderer = highlightChild.GetComponent<SpriteRenderer>();
                }
            }
        }

        public void Initialize(HexCoordinates coords, bool isWalkable = true)
        {
            _coordinates = coords;
            _isWalkable = isWalkable;
            name = $"Tile_{coords.Q}_{coords.R}";
            transform.position = coords.ToWorldPosition();

            if (_tileRenderer != null)
            {
                _tileRenderer.sortingLayerName = "HexGrid";
                _tileRenderer.sortingOrder = 0;
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

        public void SetObstacle(bool isObstacle)
        {
            _isWalkable = !isObstacle;
            if (_tileRenderer != null)
            {
                _tileRenderer.color = isObstacle ? new Color(0.4f, 0.35f, 0.3f, 1f) : Color.white;
            }
        }
    }
}
