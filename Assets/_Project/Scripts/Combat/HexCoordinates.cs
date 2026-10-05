using System;
using UnityEngine;

namespace Kervan.Combat
{
    /// <summary>
    /// 2D Izometrik altigen harita icin Axial (q, r) koordinat yapisi.
    /// Red Blob Games Hexagonal Grid matematik standardina dayanir.
    /// </summary>
    [Serializable]
    public struct HexCoordinates : IEquatable<HexCoordinates>
    {
        [SerializeField] private int _q;
        [SerializeField] private int _r;

        public int Q => _q;
        public int R => _r;
        public int S => -_q - _r; // Kubik ucuncu bilesen

        public HexCoordinates(int q, int r)
        {
            _q = q;
            _r = r;
        }

        // 6 Komsunun Axial Yon Vektorleri (Pointy-Top / Isometric Hex)
        public static readonly HexCoordinates[] Directions = new HexCoordinates[]
        {
            new HexCoordinates(1, 0),   // Dogu
            new HexCoordinates(1, -1),  // Kuzeydogu
            new HexCoordinates(0, -1),  // Kuzeybati
            new HexCoordinates(-1, 0),  // Bati
            new HexCoordinates(-1, 1),  // Guneybati
            new HexCoordinates(0, 1)    // Guneydogu
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
        /// Iki altigen hucre arasindaki adim mesafesini hesaplar (Manhattan Hex Distance).
        /// </summary>
        public static int Distance(HexCoordinates a, HexCoordinates b)
        {
            return (Mathf.Abs(a.Q - b.Q) + Mathf.Abs(a.R - b.R) + Mathf.Abs(a.S - b.S)) / 2;
        }

        public int DistanceTo(HexCoordinates other)
        {
            return Distance(this, other);
        }

        #region Isometric Grid Projeksiyonu (256x128 px, PPU = 128 -> W: 2.0, H: 1.0)

        public const float TileWidth = 2.0f;
        public const float TileHeight = 1.0f;
        // Pointy-topped hex araligi
        public const float SpacingX = TileWidth * 0.8660254f; // sqrt(3)/2 * W
        public const float SpacingY = TileHeight * 0.75f;

        /// <summary>
        /// Axial koordinati Unity dunya pozisyonuna (Vector3) cevirir.
        /// </summary>
        public Vector3 ToWorldPosition(float zOffset = 0f)
        {
            float x = (Q + R * 0.5f) * SpacingX;
            float y = R * SpacingY;
            return new Vector3(x, y, zOffset);
        }

        /// <summary>
        /// Unity dunya pozisyonunu en yakin altigen koordinatina yuvarlar.
        /// </summary>
        public static HexCoordinates FromWorldPosition(Vector3 worldPos)
        {
            float rFloat = worldPos.y / SpacingY;
            float qFloat = (worldPos.x / SpacingX) - (rFloat * 0.5f);
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
