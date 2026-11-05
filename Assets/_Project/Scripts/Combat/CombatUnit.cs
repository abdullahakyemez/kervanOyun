using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;

namespace Kervan.Combat
{
    /// <summary>
    /// Savaş alanındaki canlı bir birimi (Alp, Muhafız, Okçu, Haydut), ayak altı seçim halkasını
    /// ve baş üstü durum göstergelerini temsil eden bileşen.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class CombatUnit : MonoBehaviour
    {
        [Header("Tanımlayıcı & Veri")]
        [SerializeField] private UnitDataSO _data;
        [SerializeField] private bool _isPlayerControlled;
        [SerializeField] private HexCoordinates _coordinates;
        [SerializeField] private string _caravanGuardId;

        [Header("Canlı Statlar")]
        [SerializeField] private int _currentHealth;
        [SerializeField] private int _currentArmor;
        [SerializeField] private int _currentStamina;
        [SerializeField] private int _currentMorale;
        [SerializeField] private bool _isDefending;
        [SerializeField] private bool _isRouting;
        [SerializeField] private bool _isDead;
        [SerializeField] private bool _isUsingRangedWeapon;

        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _selectionRingRenderer;
        private Transform _worldBarRoot;
        private Transform _hpFillTransform;
        private Transform _armorFillTransform;
        private Coroutine _moveCoroutine;
        private Coroutine _actionCoroutine;

        private static Sprite _sharedPixelSprite;

        // GDD §1 & §4 Renk Standartları
        private static readonly Color ColorRingAlly = new Color(0.165f, 0.294f, 0.216f, 0.70f);     // #2A4B37 Sancak Yeşili
        private static readonly Color ColorRingEnemy = new Color(0.545f, 0.149f, 0.118f, 0.70f);    // #8B261E Koyu Al
        private static readonly Color ColorRingSelected = new Color(0.855f, 0.647f, 0.125f, 0.95f); // #DAA520 Altın
        private static readonly Color ColorBarBg = new Color(0.12f, 0.09f, 0.08f, 0.85f);           // #1E1815 Koyu Mürekkep
        private static readonly Color ColorHpAlly = new Color(0.18f, 0.52f, 0.24f, 0.95f);          // Dost Can Yeşili
        private static readonly Color ColorHpEnemy = new Color(0.68f, 0.16f, 0.12f, 0.95f);         // Düşman Can Kırmızısı
        private static readonly Color ColorArmor = new Color(0.36f, 0.42f, 0.48f, 0.95f);           // #4A4E51 Çelik Gri

        // Olaylar (UI ve CombatTurnManager için)
        public event Action<CombatUnit> OnStatsChanged;
        public event Action<CombatUnit> OnUnitDied;

        // Özellikler
        public UnitDataSO Data => _data;
        public bool IsPlayerControlled => _isPlayerControlled;
        public HexCoordinates Coordinates => _coordinates;
        public string CaravanGuardId => _caravanGuardId;
        public int CurrentHealth => _currentHealth;
        public int CurrentArmor => _currentArmor;
        public int CurrentStamina => _currentStamina;
        public int CurrentMorale => _currentMorale;
        public bool IsDefending => _isDefending;
        public bool IsRouting => _isRouting;
        public bool IsDead => _isDead;
        public bool IsUsingRangedWeapon => _isUsingRangedWeapon;

        public int ActiveAttackRange
        {
            get
            {
                if (_data == null) return 1;
                if (_data.CombatRole == UnitCombatRole.Ranged) return _data.RangedRange;
                if (_data.CombatRole == UnitCombatRole.Hybrid) return _isUsingRangedWeapon ? _data.RangedRange : _data.MeleeRange;
                return _data.MeleeRange;
            }
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sortingLayerName = "Entities";
        }

        public void Initialize(
            UnitDataSO data,
            HexCoordinates startingCoords,
            bool isPlayerControlled,
            Sprite selectionRingSprite = null,
            int overrideHealth = -1,
            int overrideMorale = -1,
            string caravanGuardId = null)
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null) _spriteRenderer.sortingLayerName = "Entities";

            _data = data;
            _coordinates = startingCoords;
            _isPlayerControlled = isPlayerControlled;
            _caravanGuardId = caravanGuardId;
            _isDead = false;
            _isDefending = false;
            _isRouting = false;

            if (_data != null)
            {
                _currentHealth = overrideHealth > 0 ? Mathf.Clamp(overrideHealth, 1, _data.MaxHealth) : _data.MaxHealth;
                _currentArmor = _data.MaxArmor;
                _currentStamina = _data.MaxStamina;
                _currentMorale = overrideMorale >= 0 ? Mathf.Clamp(overrideMorale, 10, 150) : _data.BaseMorale;
                _isUsingRangedWeapon = (_data.CombatRole == UnitCombatRole.Ranged);

                if (_data.DefaultSprite != null && _spriteRenderer != null)
                {
                    _spriteRenderer.sprite = _data.DefaultSprite;
                }
            }

            // Düşman birimleri sağ kanatta başladığı için sola (kervana) bakar
            if (_spriteRenderer != null)
            {
                _spriteRenderer.flipX = !_isPlayerControlled;
            }

            transform.position = startingCoords.ToWorldPosition();
            SetupVisualLayers(selectionRingSprite);
            SetSelectedVisual(false);
            UpdateSorting();
            UpdateWorldBars();
        }

        private void SetupVisualLayers(Sprite selectionRingSprite)
        {
            // 1. Ayak altı seçim/takım halkası
            if (_selectionRingRenderer == null)
            {
                Transform ringChild = transform.Find("SelectionRing");
                if (ringChild == null)
                {
                    GameObject ringObj = new GameObject("SelectionRing");
                    ringObj.transform.SetParent(transform, false);
                    ringObj.transform.localPosition = new Vector3(0f, -0.05f, 0f);
                    ringObj.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
                    _selectionRingRenderer = ringObj.AddComponent<SpriteRenderer>();
                }
                else
                {
                    _selectionRingRenderer = ringChild.GetComponent<SpriteRenderer>();
                }
            }

            if (_selectionRingRenderer != null)
            {
                if (selectionRingSprite != null)
                {
                    _selectionRingRenderer.sprite = selectionRingSprite;
                }
                _selectionRingRenderer.sortingLayerName = "GridHighlights";
                _selectionRingRenderer.sortingOrder = 5;
            }

            // 2. Baş üstü mini Can ve Zırh çubukları (WorldUI)
            if (_worldBarRoot == null)
            {
                EnsureSharedPixelSprite();
                GameObject barRoot = new GameObject("WorldStatBars");
                barRoot.transform.SetParent(transform, false);
                barRoot.transform.localPosition = new Vector3(0f, 1.38f, 0f);
                _worldBarRoot = barRoot.transform;

                // Arka plan çerçevesi
                CreateBarQuad("BarBG", _worldBarRoot, Vector3.zero, new Vector2(0.86f, 0.16f), ColorBarBg, 10);

                // Can (HP) çubuğu (üst)
                Color hpColor = _isPlayerControlled ? ColorHpAlly : ColorHpEnemy;
                _hpFillTransform = CreateLeftAnchoredBar("HpFill", _worldBarRoot, new Vector3(-0.40f, 0.03f, 0f), new Vector2(0.80f, 0.06f), hpColor, 11);

                // Zırh (Armor) çubuğu (alt)
                _armorFillTransform = CreateLeftAnchoredBar("ArmorFill", _worldBarRoot, new Vector3(-0.40f, -0.04f, 0f), new Vector2(0.80f, 0.04f), ColorArmor, 11);
            }
        }

        private static void EnsureSharedPixelSprite()
        {
            if (_sharedPixelSprite != null) return;
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            _sharedPixelSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
        }

        private static void CreateBarQuad(string objName, Transform parent, Vector3 localPos, Vector2 size, Color color, int sortOrder)
        {
            GameObject go = new GameObject(objName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _sharedPixelSprite;
            sr.color = color;
            sr.sortingLayerName = "WorldUI";
            sr.sortingOrder = sortOrder;
        }

        private static Transform CreateLeftAnchoredBar(string objName, Transform parent, Vector3 leftAnchorPos, Vector2 fullSize, Color color, int sortOrder)
        {
            GameObject pivotObj = new GameObject(objName + "_Pivot");
            pivotObj.transform.SetParent(parent, false);
            pivotObj.transform.localPosition = leftAnchorPos;

            GameObject fillObj = new GameObject(objName);
            fillObj.transform.SetParent(pivotObj.transform, false);
            fillObj.transform.localPosition = new Vector3(fullSize.x * 0.5f, 0f, 0f);
            fillObj.transform.localScale = new Vector3(fullSize.x, fullSize.y, 1f);

            var sr = fillObj.AddComponent<SpriteRenderer>();
            sr.sprite = _sharedPixelSprite;
            sr.color = color;
            sr.sortingLayerName = "WorldUI";
            sr.sortingOrder = sortOrder;

            return pivotObj.transform;
        }

        public void SetSelectedVisual(bool isSelected)
        {
            if (_selectionRingRenderer == null) return;
            if (_isDead)
            {
                _selectionRingRenderer.enabled = false;
                return;
            }

            _selectionRingRenderer.enabled = true;
            if (isSelected)
            {
                _selectionRingRenderer.color = ColorRingSelected;
                _selectionRingRenderer.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
            }
            else
            {
                _selectionRingRenderer.color = _isPlayerControlled ? ColorRingAlly : ColorRingEnemy;
                _selectionRingRenderer.transform.localScale = new Vector3(0.82f, 0.82f, 1f);
            }
        }

        private void UpdateWorldBars()
        {
            if (_worldBarRoot == null || _data == null) return;

            if (_isDead)
            {
                _worldBarRoot.gameObject.SetActive(false);
                return;
            }

            float hpRatio = _data.MaxHealth > 0 ? Mathf.Clamp01((float)_currentHealth / _data.MaxHealth) : 0f;
            float armorRatio = _data.MaxArmor > 0 ? Mathf.Clamp01((float)_currentArmor / _data.MaxArmor) : 0f;

            if (_hpFillTransform != null)
            {
                _hpFillTransform.localScale = new Vector3(hpRatio, 1f, 1f);
            }

            if (_armorFillTransform != null)
            {
                _armorFillTransform.localScale = new Vector3(armorRatio, 1f, 1f);
            }
        }

        public void ResetTurn()
        {
            if (_isDead) return;

            _isDefending = false;
            if (_data != null)
            {
                _currentStamina = _data.MaxStamina;
            }

            // Panikleyen birim moral toparlamayı dener
            if (_isRouting && _currentMorale > 30)
            {
                _isRouting = false;
            }

            UpdateWorldBars();
            OnStatsChanged?.Invoke(this);
        }

        /// <summary>
        /// Hibrit birimler (Alp) için Kılıç (Yakın) ve Yay (Menzilli) modları arasında geçiş yapar.
        /// </summary>
        public bool ToggleWeaponMode()
        {
            if (_data == null || _data.CombatRole != UnitCombatRole.Hybrid) return false;
            _isUsingRangedWeapon = !_isUsingRangedWeapon;
            OnStatsChanged?.Invoke(this);
            return _isUsingRangedWeapon;
        }

        /// <summary>
        /// Nara / Taktik yetenek ile birime moral ve geçici savunma kazandırır.
        /// </summary>
        public void AddMoraleBoost(int moraleDelta)
        {
            if (_isDead) return;
            _currentMorale = Mathf.Clamp(_currentMorale + moraleDelta, 0, 140);
            if (_currentMorale >= 35) _isRouting = false;
            OnStatsChanged?.Invoke(this);
        }

        public bool SpendStamina(int amount)
        {
            if (_currentStamina < amount) return false;
            _currentStamina -= amount;
            OnStatsChanged?.Invoke(this);
            return true;
        }

        public bool CanMoveTo(int staminaCost)
        {
            return !_isDead && !_isRouting && _currentStamina >= staminaCost;
        }

        public void MoveAlongPath(List<HexCoordinates> path, HexGrid grid, Action onComplete = null)
        {
            if (path == null || path.Count == 0 || _isDead)
            {
                onComplete?.Invoke();
                return;
            }

            int totalCost = path.Count * (_data != null ? _data.StaminaCostMovePerTile : 1);
            _currentStamina = Mathf.Max(0, _currentStamina - totalCost);

            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(MoveAlongPathCoroutine(path, grid, onComplete));
        }

        private IEnumerator MoveAlongPathCoroutine(List<HexCoordinates> path, HexGrid grid, Action onComplete)
        {
            var currentTile = grid.GetTile(_coordinates);
            if (currentTile != null && currentTile.OccupyingUnit == this)
            {
                currentTile.OccupyingUnit = null;
            }

            for (int i = 0; i < path.Count; i++)
            {
                HexCoordinates nextCoords = path[i];
                Vector3 targetPos = nextCoords.ToWorldPosition();

                if (targetPos.x < transform.position.x - 0.05f) _spriteRenderer.flipX = true;
                else if (targetPos.x > transform.position.x + 0.05f) _spriteRenderer.flipX = false;

                while (Vector3.Distance(transform.position, targetPos) > 0.04f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 5.5f);
                    UpdateSorting();
                    yield return null;
                }

                transform.position = targetPos;
                _coordinates = nextCoords;
                UpdateSorting();
            }

            var newTile = grid.GetTile(_coordinates);
            if (newTile != null)
            {
                newTile.OccupyingUnit = this;
            }

            OnStatsChanged?.Invoke(this);
            onComplete?.Invoke();
        }

        public bool CanAttack(CombatUnit target)
        {
            if (_isDead || _isRouting || target == null || target.IsDead) return false;
            int cost = _data != null ? _data.StaminaCostAttack : 2;
            if (_currentStamina < cost) return false;

            int dist = HexCoordinates.Distance(_coordinates, target.Coordinates);
            return dist <= ActiveAttackRange;
        }

        public CombatAttackResult ExecuteAttack(CombatUnit target)
        {
            if (!CanAttack(target)) return CombatAttackResult.Missed();

            int cost = _data != null ? _data.StaminaCostAttack : 2;
            _currentStamina -= cost;

            // Hedefe doğru dön
            if (target.transform.position.x < transform.position.x - 0.05f) _spriteRenderer.flipX = true;
            else if (target.transform.position.x > transform.position.x + 0.05f) _spriteRenderer.flipX = false;

            if (_actionCoroutine != null) StopCoroutine(_actionCoroutine);
            _actionCoroutine = StartCoroutine(AttackLungeCoroutine(target.transform.position));

            int dist = HexCoordinates.Distance(_coordinates, target.Coordinates);
            var result = DamageCalculator.ResolveAttack(this, target, dist);

            target.ApplyDamageResult(result);
            OnStatsChanged?.Invoke(this);

            return result;
        }

        private IEnumerator AttackLungeCoroutine(Vector3 targetWorldPos)
        {
            Vector3 origin = _coordinates.ToWorldPosition();
            Vector3 dir = (targetWorldPos - origin).normalized;
            Vector3 lungePos = origin + (dir * 0.32f);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 14f;
                transform.position = Vector3.Lerp(origin, lungePos, t);
                yield return null;
            }

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 10f;
                transform.position = Vector3.Lerp(lungePos, origin, t);
                yield return null;
            }

            transform.position = origin;
        }

        public void Defend()
        {
            if (_isDead || _isRouting) return;
            int cost = _data != null ? _data.StaminaCostDefend : 1;
            if (_currentStamina >= cost)
            {
                _currentStamina -= cost;
                _isDefending = true;
                OnStatsChanged?.Invoke(this);
            }
        }

        public void ApplyDamageResult(CombatAttackResult result)
        {
            if (!result.IsHit || _isDead) return;

            _currentHealth = result.RemainingHealth;
            _currentArmor = result.RemainingArmor;

            int moraleLoss = result.TargetDied ? 100 : (result.HealthDamage / 2) + 5;
            _currentMorale = Mathf.Max(0, _currentMorale - moraleLoss);

            EventBus.Publish(new CombatHitEvent(result.ArmorDamage, result.HealthDamage));

            if (result.TargetPanicked)
            {
                _isRouting = true;
            }

            UpdateWorldBars();

            if (result.TargetDied)
            {
                Die();
            }
            else
            {
                StartCoroutine(FlashDamageCoroutine());
            }

            OnStatsChanged?.Invoke(this);
        }

        private IEnumerator FlashDamageCoroutine()
        {
            if (_spriteRenderer == null) yield break;
            Color original = Color.white;
            _spriteRenderer.color = new Color(1f, 0.35f, 0.35f, 1f);
            yield return new WaitForSeconds(0.16f);
            if (!_isDead && _spriteRenderer != null)
            {
                _spriteRenderer.color = original;
            }
        }

        private void Die()
        {
            _isDead = true;
            _currentHealth = 0;
            _currentStamina = 0;
            SetSelectedVisual(false);
            UpdateWorldBars();

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = new Color(0.35f, 0.32f, 0.30f, 0.55f);
            }
            OnUnitDied?.Invoke(this);
        }

        private void UpdateSorting()
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                // Y-sorting ile öndeki birim arkadakini örter
                _spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100f);
            }
        }
    }
}
