using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;

namespace Kervan.Combat
{
    public enum CombatState
    {
        Initializing,
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat
    }

    /// <summary>
    /// Savasin durum makinesini (FSM), oyuncu girdi secimlerini
    /// ve tur dongusunu yoneten merkezi sinif.
    /// </summary>
    public class CombatTurnManager : MonoBehaviour
    {
        [Header("Baglantilar")]
        [SerializeField] private HexGrid _grid;

        [Header("Savas Kadrolari")]
        [SerializeField] private List<CombatUnit> _playerUnits = new List<CombatUnit>();
        [SerializeField] private List<CombatUnit> _enemyUnits = new List<CombatUnit>();

        [Header("Durum")]
        [SerializeField] private CombatState _currentState = CombatState.Initializing;
        [SerializeField] private int _roundNumber = 1;

        private CombatUnit _selectedUnit;
        private HexCoordinates? _previewTargetCoords;
        private Dictionary<HexCoordinates, int> _reachableTiles = new Dictionary<HexCoordinates, int>();

        // Olaylar
        public event Action<CombatUnit> OnUnitSelected;
        public event Action<CombatTurnPhase, int> OnTurnChanged;
        public event Action<bool> OnCombatEnded; // True: Zafer, False: Yenilgi

        public CombatState CurrentState => _currentState;
        public int RoundNumber => _roundNumber;
        public CombatUnit SelectedUnit => _selectedUnit;
        public IReadOnlyList<CombatUnit> PlayerUnits => _playerUnits;
        public IReadOnlyList<CombatUnit> EnemyUnits => _enemyUnits;

        private void Start()
        {
            if (_grid == null)
            {
                _grid = FindFirstObjectByType<HexGrid>();
            }

            // Birimlerin olum olaylarini dinle
            foreach (var u in _playerUnits) u.OnUnitDied += HandleUnitDied;
            foreach (var u in _enemyUnits) u.OnUnitDied += HandleUnitDied;

            StartPlayerTurn();
        }

        public void RegisterUnit(CombatUnit unit)
        {
            if (unit == null) return;
            if (unit.IsPlayerControlled)
            {
                if (!_playerUnits.Contains(unit)) _playerUnits.Add(unit);
            }
            else
            {
                if (!_enemyUnits.Contains(unit)) _enemyUnits.Add(unit);
            }
            unit.OnUnitDied += HandleUnitDied;
        }

        public void StartPlayerTurn()
        {
            _currentState = CombatState.PlayerTurn;
            _previewTargetCoords = null;

            foreach (var unit in _playerUnits)
            {
                unit.ResetTurn();
            }

            EventBus.Publish(new CombatTurnChangedEvent(CombatTurnPhase.PlayerTurn, _roundNumber));
            OnTurnChanged?.Invoke(CombatTurnPhase.PlayerTurn, _roundNumber);

            // Ilk yasayan oyuncu birimini otomatik sec
            CombatUnit firstAlive = _playerUnits.Find(u => !u.IsDead);
            if (firstAlive != null)
            {
                SelectUnit(firstAlive);
            }
        }

        public void SelectUnit(CombatUnit unit)
        {
            if (_currentState != CombatState.PlayerTurn) return;
            if (unit == null || unit.IsDead) return;

            _selectedUnit = unit;
            _previewTargetCoords = null;
            _grid.ClearAllHighlights();

            OnUnitSelected?.Invoke(_selectedUnit);

            if (_selectedUnit.IsPlayerControlled)
            {
                UpdateMovementAndAttackHighlights();
            }
        }

        private void UpdateMovementAndAttackHighlights()
        {
            if (_selectedUnit == null || _selectedUnit.IsDead) return;

            _grid.ClearAllHighlights();

            // 1. Kendi karesini altin rengi secili yap
            _grid.HighlightTiles(new[] { _selectedUnit.Coordinates }, TileHighlightState.Selected);

            // 2. Yuruyebilecegi altigenleri hesapla (Turkuaz)
            _reachableTiles = HexPathfinder.GetReachableTiles(_grid, _selectedUnit.Coordinates, _selectedUnit.CurrentStamina);
            _grid.HighlightTiles(_reachableTiles.Keys, TileHighlightState.Reachable);

            // 3. Vurabilecegi dusmanlari kirmizi yap
            var attackableCoords = new List<HexCoordinates>();
            int attackCost = _selectedUnit.Data != null ? _selectedUnit.Data.StaminaCostAttack : 2;

            if (_selectedUnit.CurrentStamina >= attackCost)
            {
                int range = (_selectedUnit.Data != null && _selectedUnit.Data.CombatRole == UnitCombatRole.Ranged)
                    ? _selectedUnit.Data.RangedRange : (_selectedUnit.Data != null ? _selectedUnit.Data.MeleeRange : 1);

                foreach (var enemy in _enemyUnits)
                {
                    if (enemy != null && !enemy.IsDead)
                    {
                        if (HexCoordinates.Distance(_selectedUnit.Coordinates, enemy.Coordinates) <= range)
                        {
                            attackableCoords.Add(enemy.Coordinates);
                        }
                    }
                }
            }

            _grid.HighlightTiles(attackableCoords, TileHighlightState.AttackTarget);
        }

        /// <summary>
        /// EKRAN.md standardindaki iki dokunuslu (Two-Tap Execution) girdi yonetimi.
        /// 1. Dokunus: Hedef onizleme (Preview).
        /// 2. Dokunus: Eylemi calistir (Execute).
        /// </summary>
        public void HandleTileClicked(HexCoordinates clickedCoords)
        {
            if (_currentState != CombatState.PlayerTurn || _selectedUnit == null) return;

            var clickedTile = _grid.GetTile(clickedCoords);
            if (clickedTile == null) return;

            // Tiklanan karede bir dost birim varsa onu sec
            if (clickedTile.OccupyingUnit != null && clickedTile.OccupyingUnit.IsPlayerControlled && !clickedTile.OccupyingUnit.IsDead)
            {
                SelectUnit(clickedTile.OccupyingUnit);
                return;
            }

            // Tiklanan karede dusman varsa: Saldiri akisi
            if (clickedTile.OccupyingUnit != null && !clickedTile.OccupyingUnit.IsPlayerControlled && !clickedTile.OccupyingUnit.IsDead)
            {
                var targetEnemy = clickedTile.OccupyingUnit;

                if (_selectedUnit.CanAttack(targetEnemy))
                {
                    // Iki dokunus kontrolu: Ayni dusmana ikinci kez basildiysa vur!
                    if (_previewTargetCoords == clickedCoords)
                    {
                        _previewTargetCoords = null;
                        _selectedUnit.ExecuteAttack(targetEnemy);
                        UpdateMovementAndAttackHighlights();
                        CheckBattleOutcome();
                    }
                    else
                    {
                        // Ilk dokunus: Hedefe odaklan (Floating Prediction UI guncellenir)
                        _previewTargetCoords = clickedCoords;
                        clickedTile.SetHighlight(TileHighlightState.Selected);
                    }
                }
                return;
            }

            // Tiklanan kare bos ve ulasilabilir ise: Hareket akisi
            if (_reachableTiles.ContainsKey(clickedCoords))
            {
                if (_previewTargetCoords == clickedCoords)
                {
                    // Ikinci dokunus: Hareketi baslat!
                    _previewTargetCoords = null;
                    var path = HexPathfinder.FindPath(_grid, _selectedUnit.Coordinates, clickedCoords);
                    _selectedUnit.MoveAlongPath(path, _grid, () =>
                    {
                        UpdateMovementAndAttackHighlights();
                    });
                }
                else
                {
                    // Ilk dokunus: Rota onizleme
                    _previewTargetCoords = clickedCoords;
                    var path = HexPathfinder.FindPath(_grid, _selectedUnit.Coordinates, clickedCoords);
                    _grid.ClearAllHighlights();
                    _grid.HighlightTiles(_reachableTiles.Keys, TileHighlightState.Reachable);
                    _grid.HighlightTiles(path, TileHighlightState.PathPreview);
                }
            }
        }

        public void EndPlayerTurn()
        {
            if (_currentState != CombatState.PlayerTurn) return;

            _grid.ClearAllHighlights();
            _previewTargetCoords = null;
            _selectedUnit = null;

            StartCoroutine(EnemyTurnCoroutine());
        }

        private IEnumerator EnemyTurnCoroutine()
        {
            _currentState = CombatState.EnemyTurn;
            EventBus.Publish(new CombatTurnChangedEvent(CombatTurnPhase.EnemyTurn, _roundNumber));
            OnTurnChanged?.Invoke(CombatTurnPhase.EnemyTurn, _roundNumber);

            yield return new WaitForSeconds(0.5f);

            foreach (var enemy in _enemyUnits)
            {
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.ResetTurn();
                    bool aiFinished = false;

                    yield return BanditAI.ExecuteAITurn(enemy, _grid, _playerUnits, () => aiFinished = true);

                    while (!aiFinished) yield return null;
                    yield return new WaitForSeconds(0.3f);

                    if (CheckBattleOutcome()) yield break;
                }
            }

            // Tur sonu (Round End)
            _roundNumber++;
            StartPlayerTurn();
        }

        private void HandleUnitDied(CombatUnit deadUnit)
        {
            var tile = _grid.GetTile(deadUnit.Coordinates);
            if (tile != null && tile.OccupyingUnit == deadUnit)
            {
                tile.OccupyingUnit = null;
            }

            CheckBattleOutcome();
        }

        private bool CheckBattleOutcome()
        {
            bool allEnemiesDead = _enemyUnits.TrueForAll(u => u == null || u.IsDead);
            if (allEnemiesDead)
            {
                _currentState = CombatState.Victory;
                _grid.ClearAllHighlights();
                OnCombatEnded?.Invoke(true);
                Debug.Log("[Combat] ZAFER! Tum haydutlar etkisiz hale getirildi.");
                return true;
            }

            bool allPlayersDead = _playerUnits.TrueForAll(u => u == null || u.IsDead);
            if (allPlayersDead)
            {
                _currentState = CombatState.Defeat;
                _grid.ClearAllHighlights();
                OnCombatEnded?.Invoke(false);
                Debug.Log("[Combat] YENILGI! Kervan muhafizlari dustu.");
                return true;
            }

            return false;
        }
    }
}
