using System;
using UnityEngine;

namespace Kervan.Combat
{
    /// <summary>
    /// 2D İzometrik altıgen harita için Axial (q, r) koordinat yapısı.
    /// Tile_HexFrame / Tile_Steppe izometrik kafes (lattice) vektörleriyle tam kenetlenir.
    /// </summary>
    [Serializable]
    public struct HexCoordinates : IEquatable<HexCoordinates>
    {
        [SerializeField] private int _q;
        [SerializeField] private int _r;

        public int Q => _q;
        public int R => _r;
        public int S => -_q - _r; // Kübik üçüncü bileşen

        public HexCoordinates(int q, int r)
        {
            _q = q;
            _r = r;
        }

        // 6 Komşunun Axial Yön Vektörleri
        public static readonly HexCoordinates[] Directions = new HexCoordinates[]
        {
            new HexCoordinates(1, 0),   // 0: Doğu (Sağ)
            new HexCoordinates(1, -1),  // 1: Güneydoğu (Sağ-Alt)
            new HexCoordinates(0, -1),  // 2: Güneybatı (Sol-Alt)
            new HexCoordinates(-1, 0),  // 3: Batı (Sol)
            new HexCoordinates(-1, 1),  // 4: Kuzeybatı (Sol-Üst)
            new HexCoordinates(0, 1)    // 5: Kuzeydoğu (Sağ-Üst)
        };

        public HexCoordinates GetNeighbor(int directionIndex)
        {
            int idx = (directionIndex % 6 + 6) % 6;
            return this + Directions[idx];
        }

        public static HexCoordinates operator +(HexCoordinates a, HexCoordinates b)
        {
            return new HexCoordinates(a.Q + b.Q, a.R + b.R);
        }

        public static HexCoordinates operator -(HexCoordinates a, HexCoordinates b)
        {
            return new HexCoordinates(a.Q - b.Q, a.R - b.R);
        }

        public static bool operator ==(HexCoordinates a, HexCoordinates b)
        {
            return a.Q == b.Q && a.R == b.R;
        }

        public static bool operator !=(HexCoordinates a, HexCoordinates b)
        {
            return !(a == b);
        }

        /// <summary>
        /// İki altıgen hücre arasındaki adım mesafesini hesaplar (Manhattan Hex Distance).
        /// </summary>
        public static int Distance(HexCoordinates a, HexCoordinates b)
        {
            return (Mathf.Abs(a.Q - b.Q) + Mathf.Abs(a.R - b.R) + Mathf.Abs(a.S - b.S)) / 2;
        }

        public int DistanceTo(HexCoordinates other)
        {
            return Distance(this, other);
        }

        #region İzometrik Kafes Projeksiyonu (Tile_HexFrame 184x108 px, PPU = 92 -> 2.0 x 1.174 Unit)

        public const float TileWidth = 2.0f;
        public const float TileHeight = 1.173913f;

        // Q (Doğu: +139 px, +8 px) ve R (Kuzeydoğu: +9 px, +81 px) taban vektörleri (PPU = 92)
        public const float AxisQX = 139f / 92f; // 1.5108696f
        public const float AxisQY = 8f / 92f;   // 0.0869565f
        public const float AxisRX = 9f / 92f;   // 0.0978261f
        public const float AxisRY = 81f / 92f;  // 0.8804348f

        // 8x6 ızgarayı ekranın ortasındaki savaş açıklığına hizalayan merkez ofseti
        private static Vector2 _gridOriginOffset = new Vector2(-5.53f, -2.55f);

        public static Vector2 GridOriginOffset
        {
            get => _gridOriginOffset;
            set => _gridOriginOffset = value;
        }

        /// <summary>
        /// Verilen sütun ve satır sayısına göre ızgaranın dünya merkezini (centerOffset) olacak şekilde ayarlar.
        /// </summary>
        public static void ConfigureGridCenter(int width, int height, Vector2 centerWorldOffset)
        {
            float halfQ = (width - 1) * 0.5f;
            float halfR = (height - 1) * 0.5f;
            float centerX = halfQ * AxisQX + halfR * AxisRX;
            float centerY = halfQ * AxisQY + halfR * AxisRY;
            _gridOriginOffset = new Vector2(centerWorldOffset.x - centerX, centerWorldOffset.y - centerY);
        }

        /// <summary>
        /// Axial koordinatı Unity dünya pozisyonuna (Vector3) çevirir.
        /// </summary>
        public Vector3 ToWorldPosition(float zOffset = 0f)
        {
            float x = _gridOriginOffset.x + (_q * AxisQX) + (_r * AxisRX);
            float y = _gridOriginOffset.y + (_q * AxisQY) + (_r * AxisRY);
            return new Vector3(x, y, zOffset);
        }

        /// <summary>
        /// Unity dünya pozisyonunu ters matris (inverse basis) ile en yakın altıgen koordinatına yuvarlar.
        /// </summary>
        public static HexCoordinates FromWorldPosition(Vector3 worldPos)
        {
            float wx = worldPos.x - _gridOriginOffset.x;
            float wy = worldPos.y - _gridOriginOffset.y;

            const float det = (AxisQX * AxisRY) - (AxisRX * AxisQY);
            float qFloat = ((AxisRY * wx) - (AxisRX * wy)) / det;
            float rFloat = ((-AxisQY * wx) + (AxisQX * wy)) / det;

            return Round(qFloat, rFloat);
        }

        private static HexCoordinates Round(float q, float r)
        {
            float s = -q - r;

            int qi = Mathf.RoundToInt(q);
            int ri = Mathf.RoundToInt(r);
            int si = Mathf.RoundToInt(s);

            float qDiff = Mathf.Abs(qi - q);
            float rDiff = Mathf.Abs(ri - r);
            float sDiff = Mathf.Abs(si - s);

            if (qDiff > rDiff && qDiff > sDiff)
            {
                qi = -ri - si;
            }
            else if (rDiff > sDiff)
            {
                ri = -qi - si;
            }

            return new HexCoordinates(qi, ri);
        }

        #endregion

        public override bool Equals(object obj)
        {
            return obj is HexCoordinates other && Equals(other);
        }

        public bool Equals(HexCoordinates other)
        {
            return _q == other._q && _r == other._r;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_q, _r);
        }

        public override string ToString()
        {
            return $"Hex({_q}, {_r})";
        }
    }
}
