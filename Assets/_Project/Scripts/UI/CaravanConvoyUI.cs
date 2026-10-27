using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kervan.UI
{
    /// <summary>
    /// Seyahat ekranında (TravelScene) yol üzerinde ilerleyen kervan konvoyundaki birimlere
    /// (Atlı Alp, Deve, Katır, Muhafız, Okçu) faz farkıyla prosedürel adımlama yaylanması (bobbing),
    /// yürüyüş eğimi (tilt/sway) ve ağırlık esnemesi (squash & stretch) uygulayan görünüm bileşeni.
    /// İş kuralı içermez; mola durumunda yumuşakça duruş pozuna döner.
    /// </summary>
    public class CaravanConvoyUI : MonoBehaviour
    {
        [Serializable]
        public struct ConvoyUnit
        {
            [Tooltip("Birim adı (Inspector takibi için)")]
            public string UnitName;

            [Tooltip("Hareket ettirilecek birimin RectTransform bileşeni")]
            public RectTransform TargetRect;

            [Tooltip("Adım sıklığı (radyan/saniye). At/insan için ~6-7, deve için ~4.5-5.5 önerilir.")]
            [Range(1f, 15f)]
            public float StepFrequency;

            [Tooltip("Her adımda dikey yükselme miktarı (piksel)")]
            [Range(0f, 25f)]
            public float BobAmplitude;

            [Tooltip("Adım atarken öne-arkaya hafif salınım açısı (derece)")]
            [Range(0f, 8f)]
            public float TiltAngle;

            [Tooltip("Yere basma anındaki dikey esneme oranı (ör. 0.02 = %2 esneme)")]
            [Range(0f, 0.08f)]
            public float SquashAmount;

            [Tooltip("Konvoydaki diğer birimlerle senkron yürümemesi için faz farkı (radyan)")]
            [Range(0f, 6.28f)]
            public float PhaseOffset;
        }

        private const float DefaultStepFrequency = 5.8f;
        private const float DefaultBobAmplitude = 5.5f;
        private const float DefaultTiltAngle = 1.6f;
        private const float DefaultSquashAmount = 0.018f;
        private const float PhaseStepPerUnit = 0.95f;
        private const float PauseSmoothSpeed = 8f;

        [Header("Konvoy Birimleri (Soldan Sağa / Arkadan Öne)")]
        [Tooltip("Yol üzerinde yürüyen kervan figürleri")]
        [SerializeField] private ConvoyUnit[] _units;

        [Header("Akış Kontrolü")]
        [Tooltip("Atanırsa duraklatma ve hız çarpanını TravelParallaxUI ile senkronize tutar")]
        [SerializeField] private TravelParallaxUI _linkedParallax;

        [Tooltip("İşaretliyse konvoy yürüyüşü duraklatılır ve birimler duruş pozuna geçer")]
        [SerializeField] private bool _isPaused;

        [Tooltip("Genel yürüyüş temposu çarpanı (1 = normal, 2 = iki kat hızlı)")]
        [Range(0f, 4f)]
        [SerializeField] private float _speedMultiplier = 1f;

        // Çalışma anı başlangıç değerleri (Editördeki konumu ve boyutu korumak için)
        private Vector2[] _baseAnchoredPositions;
        private Vector3[] _baseLocalScales;
        private float[] _baseRotationZ;
        private float _walkTime;

        /// <summary>
        /// Konvoy yürüyüşü şu anda duraklatılmış mı?
        /// </summary>
        public bool IsPaused => _isPaused;

        /// <summary>
        /// Aktif yürüyüş temposu çarpanı.
        /// </summary>
        public float SpeedMultiplier => _speedMultiplier;

        private void Reset()
        {
            AutoPopulateUnitsFromChildren();

            if (_linkedParallax == null && transform.parent != null)
            {
                _linkedParallax = transform.parent.GetComponentInChildren<TravelParallaxUI>(true);
            }
        }

        private void Awake()
        {
            if (_units == null || _units.Length == 0)
            {
                AutoPopulateUnitsFromChildren();
            }

            CacheBaseTransforms();
        }

        private void Update()
        {
            if (_units == null || _units.Length == 0)
            {
                return;
            }

            // Bağlı bir TravelParallaxUI varsa onun duraklatma ve hız durumunu takip et
            bool effectivePaused = _isPaused;
            float effectiveMultiplier = _speedMultiplier;

            if (_linkedParallax != null)
            {
                effectivePaused = _isPaused || _linkedParallax.IsPaused;
                effectiveMultiplier = _linkedParallax.SpeedMultiplier * _speedMultiplier;
            }

            float dt = Time.deltaTime;

            if (effectivePaused || effectiveMultiplier <= 0f)
            {
                SmoothReturnToIdle(dt);
                return;
            }

            _walkTime += dt * effectiveMultiplier;

            for (int i = 0; i < _units.Length; i++)
            {
                RectTransform rect = _units[i].TargetRect;
                if (rect == null)
                {
                    continue;
                }

                float phase = (_walkTime * _units[i].StepFrequency) + _units[i].PhaseOffset;

                // 1. Dikey adımlama: |sin(phase)| ile her adımda yumuşak yükselip yere basma
                float stepSin = Mathf.Sin(phase);
                float bobOffset = Mathf.Abs(stepSin) * _units[i].BobAmplitude;

                Vector2 pos = _baseAnchoredPositions[i];
                pos.y += bobOffset;
                rect.anchoredPosition = pos;

                // 2. Hafif yürüyüş salınımı (öne-arkaya beşik eğimi)
                float tilt = Mathf.Cos(phase) * _units[i].TiltAngle;
                rect.localRotation = Quaternion.Euler(0f, 0f, _baseRotationZ[i] + tilt);

                // 3. Yere basma anında hafif dikey esneme (squash & stretch)
                float squashWave = Mathf.Abs(stepSin);
                float scaleMod = (squashWave - 0.5f) * 2f * _units[i].SquashAmount;
                Vector3 baseScale = _baseLocalScales[i];
                rect.localScale = new Vector3(
                    baseScale.x * (1f - (scaleMod * 0.5f)),
                    baseScale.y * (1f + scaleMod),
                    baseScale.z);
            }
        }

        /// <summary>
        /// Konvoyun yürüyüş salınımını duraklatır veya devam ettirir.
        /// </summary>
        public void SetPaused(bool paused)
        {
            _isPaused = paused;
        }

        /// <summary>
        /// Konvoyun yürüyüş temposunu ayarlar (1f = 1x, 2f = 2x).
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Clamp(multiplier, 0f, 4f);
        }

        /// <summary>
        /// Alt objelerdeki Image bileşenlerini tarayarak konvoy birim listesini otomatik doldurur.
        /// </summary>
        [ContextMenu("Alt Birimleri Otomatik Bağla")]
        public void AutoPopulateUnitsFromChildren()
        {
            Image[] childImages = GetComponentsInChildren<Image>(true);
            int validCount = 0;

            for (int i = 0; i < childImages.Length; i++)
            {
                if (childImages[i].transform != transform)
                {
                    validCount++;
                }
            }

            _units = new ConvoyUnit[validCount];
            int index = 0;

            for (int i = 0; i < childImages.Length; i++)
            {
                if (childImages[i].transform == transform)
                {
                    continue;
                }

                RectTransform rect = childImages[i].rectTransform;
                _units[index] = new ConvoyUnit
                {
                    UnitName = rect.name,
                    TargetRect = rect,
                    StepFrequency = DefaultStepFrequency,
                    BobAmplitude = DefaultBobAmplitude,
                    TiltAngle = DefaultTiltAngle,
                    SquashAmount = DefaultSquashAmount,
                    PhaseOffset = (index * PhaseStepPerUnit) % 6.28f
                };
                index++;
            }

            CacheBaseTransforms();
        }

        private void CacheBaseTransforms()
        {
            if (_units == null)
            {
                return;
            }

            int count = _units.Length;
            _baseAnchoredPositions = new Vector2[count];
            _baseLocalScales = new Vector3[count];
            _baseRotationZ = new float[count];

            for (int i = 0; i < count; i++)
            {
                RectTransform rect = _units[i].TargetRect;
                if (rect == null)
                {
                    _baseLocalScales[i] = Vector3.one;
                    continue;
                }

                _baseAnchoredPositions[i] = rect.anchoredPosition;
                _baseLocalScales[i] = rect.localScale;
                _baseRotationZ[i] = rect.localEulerAngles.z;
            }
        }

        private void SmoothReturnToIdle(float dt)
        {
            float t = Mathf.Clamp01(dt * PauseSmoothSpeed);
            for (int i = 0; i < _units.Length; i++)
            {
                RectTransform rect = _units[i].TargetRect;
                if (rect == null)
                {
                    continue;
                }

                rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, _baseAnchoredPositions[i], t);
                rect.localRotation = Quaternion.Lerp(rect.localRotation, Quaternion.Euler(0f, 0f, _baseRotationZ[i]), t);
                rect.localScale = Vector3.Lerp(rect.localScale, _baseLocalScales[i], t);
            }
        }
    }
}
