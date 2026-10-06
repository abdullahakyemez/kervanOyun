using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;

namespace Kervan.Combat
{
    /// <summary>
    /// Savas alanindaki canli bir birimi (Alp, Muhafiz, Okcu, Haydut) temsil eden bilesen.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class CombatUnit : MonoBehaviour
    {
        [Header("Tanimlayici & Veri")]
        [SerializeField] private UnitDataSO _data;
        [SerializeField] private bool _isPlayerControlled;
        [SerializeField] private HexCoordinates _coordinates;

        [Header("Canli Statlar")]
        [SerializeField] private int _currentHealth;
        [SerializeField] private int _currentArmor;
        [SerializeField] private int _currentStamina;
        [SerializeField] private int _currentMorale;
        [SerializeField] private bool _isDefending;
        [SerializeField] private bool _isRouting;
        [SerializeField] private bool _isDead;

        private SpriteRenderer _spriteRenderer;
        private Coroutine _moveCoroutine;

        // Olaylar (UI ve CombatManager icin)
        public event Action<CombatUnit> OnStatsChanged;
        public event Action<CombatUnit> OnUnitDied;

        // Getters
        public UnitDataSO Data => _data;
        public bool IsPlayerControlled => _isPlayerControlled;
        public HexCoordinates Coordinates => _coordinates;
        public int CurrentHealth => _currentHealth;
        public int CurrentArmor => _currentArmor;
        public int CurrentStamina => _currentStamina;
        public int CurrentMorale => _currentMorale;
        public bool IsDefending => _isDefending;
        public bool IsRouting => _isRouting;
        public bool IsDead => _isDead;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sortingLayerName = "Entities";
        }

        public void Initialize(UnitDataSO data, HexCoordinates startingCoords, bool isPlayerControlled)
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null) _spriteRenderer.sortingLayerName = "Entities";

            _data = data;
            _coordinates = startingCoords;
            _isPlayerControlled = isPlayerControlled;

            if (_data != null)
            {
                _currentHealth = _data.MaxHealth;
                _currentArmor = _data.MaxArmor;
                _currentStamina = _data.MaxStamina;
                _currentMorale = _data.BaseMorale;

                if (_data.DefaultSprite != null && _spriteRenderer != null)
                {
                    _spriteRenderer.sprite = _data.DefaultSprite;
                }
            }

            transform.position = startingCoords.ToWorldPosition();
            UpdateSorting();
        }

        public void ResetTurn()
        {
            if (_isDead) return;

            _isDefending = false;
            if (_data != null)
            {
                _currentStamina = _data.MaxStamina;
            }

            // Panikleyen birim moral toparlamayi dener
            if (_isRouting && _currentMorale > 30)
            {
                _isRouting = false;
            }

            OnStatsChanged?.Invoke(this);
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
            // Eski karodan cik
            var currentTile = grid.GetTile(_coordinates);
            if (currentTile != null && currentTile.OccupyingUnit == this)
            {
                currentTile.OccupyingUnit = null;
            }

            for (int i = 0; i < path.Count; i++)
            {
                HexCoordinates nextCoords = path[i];
                Vector3 targetPos = nextCoords.ToWorldPosition();

                // Adim atarken yon cevir (flipX)
                if (targetPos.x < transform.position.x) _spriteRenderer.flipX = true;
                else if (targetPos.x > transform.position.x) _spriteRenderer.flipX = false;

                while (Vector3.Distance(transform.position, targetPos) > 0.05f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 5f);
                    UpdateSorting();
                    yield return null;
                }

                transform.position = targetPos;
                _coordinates = nextCoords;
                UpdateSorting();
            }

            // Yeni karoya yerles
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
            int range = (_data != null && _data.CombatRole == UnitCombatRole.Ranged) ? _data.RangedRange : (_data != null ? _data.MeleeRange : 1);

            return dist <= range;
        }

        public CombatAttackResult ExecuteAttack(CombatUnit target)
        {
            if (!CanAttack(target)) return CombatAttackResult.Missed();

            int cost = _data != null ? _data.StaminaCostAttack : 2;
            _currentStamina -= cost;

            int dist = HexCoordinates.Distance(_coordinates, target.Coordinates);
            var result = DamageCalculator.ResolveAttack(this, target, dist);

            target.ApplyDamageResult(result);
            OnStatsChanged?.Invoke(this);

            return result;
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

            EventBus.Publish(new CombatHitEvent(result.ArmorDamage, result.HealthDamage));

            if (result.TargetPanicked)
            {
                _isRouting = true;
            }

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
            Color original = _spriteRenderer.color;
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            _spriteRenderer.color = original;
        }

        private void Die()
        {
            _isDead = true;
            _currentHealth = 0;
            _currentStamina = 0;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = new Color(0.4f, 0.4f, 0.4f, 0.6f); // Kararma efekti
            }
            OnUnitDied?.Invoke(this);
        }

        private void UpdateSorting()
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                // BOYUTLANDİRMA.md kurali: Y-sorting ile ondeki birim arkadakini orter
                _spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
            }
        }
    }
}
