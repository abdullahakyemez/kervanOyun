using System.Collections.Generic;
using UnityEngine;

namespace Kervan.Combat
{
    /// <summary>
    /// Savaş alanındaki 8x6 izometrik altıgen ızgarayı oluşturan, taktiksel siper engellerini yerleştiren
    /// ve koordinat sorgularını yöneten yönetici sınıf.
    /// </summary>
    public class HexGrid : MonoBehaviour
    {
        [Header("Izgara Boyutları & Hizalama")]
        [SerializeField] private int _gridWidth = 8;
        [SerializeField] private int _gridHeight = 6;
        [Tooltip("Izgara merkezinin ekran ortasındaki savaş açıklığına oturması için dünya ofseti.")]
        [SerializeField] private Vector2 _gridCenterWorldOffset = new Vector2(0f, -0.35f);

        [Header("Görsel & Prefab Referansları")]
        [Tooltip("HexTile prefab'ı. Boş bırakılırsa çalışma anında Tile_HexFrame ile oluşturulur.")]
        [SerializeField] private GameObject _tilePrefab;
        [Tooltip("Yarı şeffaf altıgen taktik çerçevesi (Tile_HexFrame.png).")]
        [SerializeField] private Sprite _hexFrameSprite;
        [Tooltip("Orta kuşakta siper olarak kullanılacak engel görselleri (Prop_Combat_Rock, Prop_Combat_Barricade).")]
        [SerializeField] private List<Sprite> _obstacleSprites = new List<Sprite>();
        [SerializeField] private int _obstacleCount = 3;

        private readonly Dictionary<HexCoordinates, HexTile> _tiles = new Dictionary<HexCoordinates, HexTile>();

        public IReadOnlyDictionary<HexCoordinates, HexTile> Tiles => _tiles;
        public int GridWidth => _gridWidth;
        public int GridHeight => _gridHeight;

        private void Awake()
        {
            if (_tiles.Count == 0)
            {
                GenerateGrid();
            }
        }

        public void GenerateGrid()
        {
            ClearGrid();

            HexCoordinates.ConfigureGridCenter(_gridWidth, _gridHeight, _gridCenterWorldOffset);

            for (int r = 0; r < _gridHeight; r++)
            {
                for (int q = 0; q < _gridWidth; q++)
                {
                    CreateTile(new HexCoordinates(q, r));
                }
            }

            GenerateTacticalObstacles();
        }

        private HexTile CreateTile(HexCoordinates coords)
        {
            GameObject tileObj;
            if (_tilePrefab != null)
            {
                tileObj = Instantiate(_tilePrefab, transform);
            }
            else
            {
                tileObj = new GameObject($"Tile_{coords.Q}_{coords.R}");
                tileObj.transform.SetParent(transform);
                var sr = tileObj.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "HexGrid";

                // Vurgu katmanı için alt nesne
                GameObject highlightObj = new GameObject("Highlight");
                highlightObj.transform.SetParent(tileObj.transform, false);
                highlightObj.transform.localPosition = Vector3.zero;
                var highlightSr = highlightObj.AddComponent<SpriteRenderer>();
                highlightSr.sortingLayerName = "GridHighlights";
                highlightSr.enabled = false;
            }

            HexTile tile = tileObj.GetComponent<HexTile>();
            if (tile == null)
            {
                tile = tileObj.AddComponent<HexTile>();
            }

            tile.Initialize(coords, _hexFrameSprite);
            _tiles[coords] = tile;
            return tile;
        }

        /// <summary>
        /// Haritanın orta sütunlarında (q = 3, 4) yolu tamamen tıkamayan taktiksel kaya/barikat engelleri oluşturur.
        /// </summary>
        private void GenerateTacticalObstacles()
        {
            if (_obstacleCount <= 0 || _gridWidth < 6 || _gridHeight < 4) return;

            var candidateCoords = new List<HexCoordinates>();
            int midQ1 = (_gridWidth / 2) - 1; // 3
            int midQ2 = _gridWidth / 2;       // 4

            for (int r = 0; r < _gridHeight; r++)
            {
                candidateCoords.Add(new HexCoordinates(midQ1, r));
                candidateCoords.Add(new HexCoordinates(midQ2, r));
            }

            int placed = 0;
            int safety = 30;
            int targetCount = Mathf.Min(_obstacleCount, _gridHeight - 2);

            while (placed < targetCount && candidateCoords.Count > 0 && safety-- > 0)
            {
                int idx = Random.Range(0, candidateCoords.Count);
                HexCoordinates coord = candidateCoords[idx];
                candidateCoords.RemoveAt(idx);

                if (!_tiles.TryGetValue(coord, out var tile) || tile.HasObstacle) continue;

                // Aynı satırda yan yana iki engel koyup geçidi boğmamak için kontrol
                HexCoordinates sibling = new HexCoordinates(coord.Q == midQ1 ? midQ2 : midQ1, coord.R);
                if (_tiles.TryGetValue(sibling, out var siblingTile) && siblingTile.HasObstacle) continue;

                Sprite propSprite = (_obstacleSprites != null && _obstacleSprites.Count > 0)
                    ? _obstacleSprites[placed % _obstacleSprites.Count]
                    : null;

                tile.SetObstacle(true, propSprite);
                placed++;
            }
        }

        public HexTile GetTile(HexCoordinates coords)
        {
            _tiles.TryGetValue(coords, out var tile);
            return tile;
        }

        public bool TryGetTile(HexCoordinates coords, out HexTile tile)
        {
            return _tiles.TryGetValue(coords, out tile);
        }

        public List<HexTile> GetNeighbors(HexCoordinates coords)
        {
            var neighbors = new List<HexTile>(6);
            for (int i = 0; i < 6; i++)
            {
                HexCoordinates neighborCoords = coords.GetNeighbor(i);
                if (_tiles.TryGetValue(neighborCoords, out var tile))
                {
                    neighbors.Add(tile);
                }
            }
            return neighbors;
        }

        public void ClearAllHighlights()
        {
            foreach (var kvp in _tiles)
            {
                kvp.Value.SetHighlight(TileHighlightState.None);
            }
        }

        public void HighlightTiles(IEnumerable<HexCoordinates> coordinatesList, TileHighlightState state)
        {
            foreach (var coords in coordinatesList)
            {
                if (_tiles.TryGetValue(coords, out var tile))
                {
                    tile.SetHighlight(state);
                }
            }
        }

        public void ClearGrid()
        {
            foreach (var kvp in _tiles)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }
            _tiles.Clear();
        }
    }
}
