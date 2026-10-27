using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kervan.UI
{
    /// <summary>
    /// Seyahat ekranındaki (TravelScene) 2D arka plan katmanlarını
    /// UV kaydırma (Wrap Mode: Repeat) yöntemiyle farklı hızlarda akıtan parallax bileşeni.
    /// İş kuralı içermez; yalnızca görsel akışı, mola duraklatmasını ve hız çarpanını yönetir.
    /// </summary>
    public class TravelParallaxUI : MonoBehaviour
    {
        [Serializable]
        public struct ParallaxLayer
        {
            [Tooltip("Katman adı (Inspector'da kolay takip için)")]
            public string LayerName;

            [Tooltip("Kaydırılacak RawImage bileşeni (Texture Wrap Mode: Repeat olmalıdır)")]
            public RawImage TargetImage;

            [Tooltip("Yatay UV kayma hızı (birim/saniye). Arkadan öne doğru artmalıdır.")]
            [Range(0f, 1f)]
            public float ScrollSpeed;
        }

        private const float DefaultSkySpeed = 0.008f;
        private const float DefaultMountainSpeed = 0.025f;
        private const float DefaultMidHillSpeed = 0.06f;
        private const float DefaultRoadSpeed = 0.15f;

        [Header("Parallax Katmanları (Arkadan Öne)")]
        [Tooltip("Sırasıyla Gökyüzü, Dağlar, Orta Bozkır ve Yol katmanları")]
        [SerializeField] private ParallaxLayer[] _layers = new ParallaxLayer[]
        {
            new ParallaxLayer { LayerName = "01_Sky", ScrollSpeed = DefaultSkySpeed },
            new ParallaxLayer { LayerName = "02_Mountains", ScrollSpeed = DefaultMountainSpeed },
            new ParallaxLayer { LayerName = "03_MidHills", ScrollSpeed = DefaultMidHillSpeed },
            new ParallaxLayer { LayerName = "04_Roads", ScrollSpeed = DefaultRoadSpeed }
        };

        [Header("Akış Kontrolü")]
        [Tooltip("İşaretliyse parallax akışı duraklatılır (Mola / Kamp durumu)")]
        [SerializeField] private bool _isPaused;

        [Tooltip("Genel yürüyüş temposu çarpanı (1 = normal, 2 = iki kat hızlı)")]
        [Range(0f, 4f)]
        [SerializeField] private float _speedMultiplier = 1f;

        /// <summary>
        /// Parallax akışı şu anda duraklatılmış mı?
        /// </summary>
        public bool IsPaused => _isPaused;

        /// <summary>
        /// Aktif hız çarpanı (1x, 2x vb.).
        /// </summary>
        public float SpeedMultiplier => _speedMultiplier;

        private void Reset()
        {
            // Bileşen eklendiğinde alt objelerdeki RawImage'ları otomatik eşleştirmeyi dener (Editör kolaylığı)
            RawImage[] childImages = GetComponentsInChildren<RawImage>(true);
            float[] defaultSpeeds = { DefaultSkySpeed, DefaultMountainSpeed, DefaultMidHillSpeed, DefaultRoadSpeed };
            string[] defaultNames = { "01_Sky", "02_Mountains", "03_MidHills", "04_Roads" };

            _layers = new ParallaxLayer[defaultNames.Length];
            for (int i = 0; i < _layers.Length; i++)
            {
                _layers[i] = new ParallaxLayer
                {
                    LayerName = defaultNames[i],
                    TargetImage = i < childImages.Length ? childImages[i] : null,
                    ScrollSpeed = defaultSpeeds[i]
                };
            }
        }

        private void Awake()
        {
            if (_layers == null || _layers.Length == 0)
            {
                Debug.LogError("[TravelParallaxUI] Parallax katman listesi boş! Inspector'dan katmanları atayın.", this);
                return;
            }

            for (int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i].TargetImage == null)
                {
                    Debug.LogWarning($"[TravelParallaxUI] '{_layers[i].LayerName}' (İndeks {i}) için TargetImage atanmamış.", this);
                }
            }
        }

        private void Update()
        {
            if (_isPaused || _speedMultiplier <= 0f || _layers == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            for (int i = 0; i < _layers.Length; i++)
            {
                RawImage img = _layers[i].TargetImage;
                if (img == null)
                {
                    continue;
                }

                float speed = _layers[i].ScrollSpeed * _speedMultiplier;
                if (speed <= 0f)
                {
                    continue;
                }

                Rect uv = img.uvRect;
                uv.x = Mathf.Repeat(uv.x + (speed * dt), 1f);
                img.uvRect = uv;
            }
        }

        /// <summary>
        /// Seyahat akışını duraklatır veya devam ettirir (Mola Ver / Yola Devam Et).
        /// </summary>
        public void SetPaused(bool paused)
        {
            _isPaused = paused;
        }

        /// <summary>
        /// Seyahat görsel akış hızını değiştirir (ör. 1f = 1x, 2f = 2x tempo).
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Clamp(multiplier, 0f, 4f);
        }

        /// <summary>
        /// Tüm katmanların UV kaydırma değerlerini başlangıç (0) konumuna sıfırlar.
        /// </summary>
        public void ResetOffsets()
        {
            if (_layers == null)
            {
                return;
            }

            for (int i = 0; i < _layers.Length; i++)
            {
                RawImage img = _layers[i].TargetImage;
                if (img == null)
                {
                    continue;
                }

                Rect uv = img.uvRect;
                uv.x = 0f;
                img.uvRect = uv;
            }
        }
    }
}
