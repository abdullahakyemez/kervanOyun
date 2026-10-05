using System.Collections.Generic;
using UnityEngine;

namespace Kervan.Combat
{
    /// <summary>
    /// Savas alanindaki altigen izgarayi olusturan, karo durumlarini saklayan
    /// ve koordinat sorgularini yoneten yonetici sinif.
    /// </summary>
    public class HexGrid : MonoBehaviour
    {
        [Header("Izgara Boyutlari")]
        [SerializeField] private int _gridWidth = 8;
        [SerializeField] private int _gridHeight = 6;

        [Header("Prefab Referansi")]
        [Tooltip("HexTile prefab'i. Bos birakilirsa calisma aninda prosedurel olusturulur.")]
        [SerializeField] private GameObject _tilePrefab;

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

            for (int r = 0; r < _gridHeight; r++)
            {
                int rOffset = r >> 1; // Satir kaymasi
                for (int q = -rOffset; q < _gridWidth - rOffset; q++)
                {
                    CreateTile(new HexCoordinates(q, r));
                }
            }
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

                // Vurgu katmani icin child nesne
                GameObject highlightObj = new GameObject("Highlight");
                highlightObj.transform.SetParent(tileObj.transform);
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

            tile.Initialize(coords);
            _tiles[coords] = tile;
            return tile;
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
