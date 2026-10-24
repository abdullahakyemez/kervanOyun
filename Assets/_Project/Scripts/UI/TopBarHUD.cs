using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.Core;

namespace Kervan.UI
{
    /// <summary>
    /// Ekranın üst kısmında yer alan kaynak ve durum göstergesini (TopBar HUD) yönetir.
    /// - Akçe (sayı animasyonu ve görsel nabız ile)
    /// - Erzak (kritik eşik rengi ile)
    /// - Tarih (GameCalendar üzerinden gerçek tarih)
    /// - Mevsim (ikon ve metin gösterimi)
    /// GameSession ve EventBus olayları (CaravanStatsChangedEvent, DayPassedEvent) ile tam senkronizedir.
    /// </summary>
    public class TopBarHUD : MonoBehaviour
    {
        [Header("Akçe Göstergesi")]
        [Tooltip("Akçe miktarını gösteren metin")]
        [SerializeField] private TextMeshProUGUI _akceText;
        [Tooltip("Akçe değiştiğinde hafifçe nabız yapacak kapsül (opsiyonel)")]
        [SerializeField] private RectTransform _akcePulseTarget;

        [Header("Erzak Göstergesi")]
        [Tooltip("Erzak miktarını (gün bazlı) gösteren metin")]
        [SerializeField] private TextMeshProUGUI _erzakText;
        [Tooltip("Erzak değiştiğinde hafifçe nabız yapacak kapsül (opsiyonel)")]
        [SerializeField] private RectTransform _erzakPulseTarget;

        [Header("Tarih Göstergesi")]
        [Tooltip("Tarihi gösteren metin (örn: 14 Mayıs 1326)")]
        [SerializeField] private TextMeshProUGUI _dateText;

        [Header("Mevsim Göstergesi")]
        [Tooltip("Mevsim adını gösteren metin (örn: Bahar / Mevsim: Bahar)")]
        [SerializeField] private TextMeshProUGUI _seasonText;
        [Tooltip("Mevsim ikonunun gösterileceği Image bileşeni (opsiyonel)")]
        [SerializeField] private Image _seasonIconImage;
        [SerializeField] private Sprite _springIcon;
        [SerializeField] private Sprite _summerIcon;
        [SerializeField] private Sprite _autumnIcon;
        [SerializeField] private Sprite _winterIcon;

        [Header("Görsel Ayarlar & Eşikler")]
        [Tooltip("Erzak bu gün ve altına indiğinde kritik uyarı rengi uygulanır")]
        [SerializeField] private int _lowFoodWarningThreshold = 3;
        [Tooltip("Normal erzak metin rengi")]
        [SerializeField] private Color _normalFoodColor = Color.white;
        [Tooltip("Kritik erzak uyarı rengi (Koyu Al / Danger)")]
        [SerializeField] private Color _warningFoodColor = new Color(0.72f, 0.11f, 0.11f); // #B71C1C
        [Tooltip("Sayı sayaç animasyonu süresi (saniye)")]
        [SerializeField] private float _counterAnimDuration = 0.35f;

        private GameSession _gameSession;
        private int _displayedAkce = 500;
        private int _displayedFood = 20;
        private Coroutine _akceAnimCoroutine;
        private Coroutine _pulseCoroutine;
        private Vector3 _originalAkceScale = Vector3.one;

        private void Awake()
        {
            if (_akcePulseTarget != null)
            {
                _originalAkceScale = _akcePulseTarget.localScale;
            }
        }

        private void Start()
        {
            InitializeData();
            RegisterListeners();
        }

        private void OnDestroy()
        {
            UnregisterListeners();
            if (_akceAnimCoroutine != null) StopCoroutine(_akceAnimCoroutine);
            if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
        }

        private void RegisterListeners()
        {
            EventBus.Subscribe<CaravanStatsChangedEvent>(OnCaravanStatsChanged);
            EventBus.Subscribe<DayPassedEvent>(OnDayPassed);
        }

        private void UnregisterListeners()
        {
            EventBus.Unsubscribe<CaravanStatsChangedEvent>(OnCaravanStatsChanged);
            EventBus.Unsubscribe<DayPassedEvent>(OnDayPassed);
        }

        /// <summary>
        /// GameSession üzerinden mevcut verileri okur ve tüm HUD göstergelerini yeniler.
        /// </summary>
        public void InitializeData()
        {
            if (!Services.TryGet(out _gameSession))
            {
                _gameSession = new GameSession();
            }

            RefreshAll();
        }

        /// <summary>
        /// Tüm kaynak ve durum göstergelerini anlık değerlerle günceller.
        /// </summary>
        public void RefreshAll()
        {
            var caravan = _gameSession != null ? _gameSession.Caravan : null;
            int akce = caravan != null ? caravan.CurrentAkce : 500;
            int food = caravan != null ? caravan.CurrentFood : 20;
            int dayCount = _gameSession != null ? _gameSession.DayCount : 1;

            _displayedAkce = akce;
            _displayedFood = food;

            UpdateAkceText(akce);
            UpdateFoodText(food);
            UpdateDateAndSeason(dayCount);
        }

        private void OnCaravanStatsChanged(CaravanStatsChangedEvent evt)
        {
            if (evt.CurrentAkce != _displayedAkce)
            {
                if (_akceAnimCoroutine != null) StopCoroutine(_akceAnimCoroutine);
                _akceAnimCoroutine = StartCoroutine(AnimateAkceCounter(_displayedAkce, evt.CurrentAkce));

                if (_akcePulseTarget != null)
                {
                    if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
                    _pulseCoroutine = StartCoroutine(PulseTarget(_akcePulseTarget, _originalAkceScale));
                }
            }
            else
            {
                UpdateAkceText(evt.CurrentAkce);
            }

            UpdateFoodText(evt.CurrentFood);
        }

        private void OnDayPassed(DayPassedEvent evt)
        {
            UpdateDateAndSeason(evt.CurrentDayCount);
        }

        private void UpdateAkceText(int akce)
        {
            _displayedAkce = akce;
            if (_akceText != null)
            {
                _akceText.text = akce.ToString("N0");
            }
        }

        private void UpdateFoodText(int food)
        {
            _displayedFood = food;
            if (_erzakText != null)
            {
                _erzakText.text = $"{food} Gün";
                _erzakText.color = food <= _lowFoodWarningThreshold ? _warningFoodColor : _normalFoodColor;
            }
        }

        private void UpdateDateAndSeason(int dayCount)
        {
            string dateStr = GameCalendar.GetFormattedDate(dayCount);
            if (_dateText != null)
            {
                _dateText.text = dateStr;
            }

            SeasonType season = GameCalendar.GetSeason(dayCount);
            string seasonName = GameCalendar.GetSeasonDisplayName(season);

            if (_seasonText != null)
            {
                _seasonText.text = $"Mevsim: {seasonName}";
            }

            if (_seasonIconImage != null)
            {
                Sprite targetSprite = GetSeasonSprite(season);
                if (targetSprite != null)
                {
                    _seasonIconImage.sprite = targetSprite;
                    _seasonIconImage.gameObject.SetActive(true);
                }
            }
        }

        private Sprite GetSeasonSprite(SeasonType season)
        {
            switch (season)
            {
                case SeasonType.Spring:
                    return _springIcon;
                case SeasonType.Summer:
                    return _summerIcon;
                case SeasonType.Autumn:
                    return _autumnIcon;
                case SeasonType.Winter:
                    return _winterIcon;
                default:
                    return null;
            }
        }

        private IEnumerator AnimateAkceCounter(int startValue, int targetValue)
        {
            float elapsed = 0f;
            float duration = Mathf.Max(0.1f, _counterAnimDuration);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Yumuşak geçiş (ease-out)
                float curvedT = 1f - Mathf.Pow(1f - t, 2f);
                int current = (int)Mathf.Lerp(startValue, targetValue, curvedT);

                if (_akceText != null)
                {
                    _akceText.text = current.ToString("N0");
                }

                yield return null;
            }

            UpdateAkceText(targetValue);
            _akceAnimCoroutine = null;
        }

        private IEnumerator PulseTarget(RectTransform target, Vector3 baseScale)
        {
            if (target == null) yield break;

            Vector3 punchScale = baseScale * 1.08f;
            float halfDuration = 0.12f;
            float elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                target.localScale = Vector3.Lerp(baseScale, punchScale, elapsed / halfDuration);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                target.localScale = Vector3.Lerp(punchScale, baseScale, elapsed / halfDuration);
                yield return null;
            }

            target.localScale = baseScale;
            _pulseCoroutine = null;
        }
    }
}
