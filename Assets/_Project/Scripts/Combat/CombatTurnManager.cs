using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;
using Kervan.Core.Save;
using Kervan.TradeAndTravel;
using Kervan.UI;

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
    /// Savaşın durum makinesini (FSM), kervan kadrosundan dinamik birim dizilimini,
    /// oyuncu girdi seçimlerini ve tur döngüsünü yöneten merkezi sınıf.
    /// </summary>
    public class CombatTurnManager : MonoBehaviour
    {
        [Header("Bağlantılar")]
        [SerializeField] private HexGrid _grid;

        [Header("Birim Veri Şablonları (SO) & Görseller")]
        [SerializeField] private UnitDataSO _alpUnitData;
        [SerializeField] private UnitDataSO _defaultGuardData;
        [SerializeField] private UnitDataSO _defaultArcherData;
        [SerializeField] private UnitDataSO _banditMeleeData;
        [SerializeField] private UnitDataSO _banditArcherData;
        [Tooltip("Birimlerin ayak altında görünecek seçim/takım halkası (UI_Unit_SelectionRing.png).")]
        [SerializeField] private Sprite _selectionRingSprite;

        [Header("Savaş Kadroları")]
        [SerializeField] private List<CombatUnit> _playerUnits = new List<CombatUnit>();
        [SerializeField] private List<CombatUnit> _enemyUnits = new List<CombatUnit>();

        [Header("Durum")]
        [SerializeField] private CombatState _currentState = CombatState.Initializing;
        [SerializeField] private int _roundNumber = 1;

        private CombatUnit _selectedUnit;
        private HexCoordinates? _previewTargetCoords;
        private Dictionary<HexCoordinates, int> _reachableTiles = new Dictionary<HexCoordinates, int>();
        private string _lastOutcomeSummary = string.Empty;
        private bool _combatOutcomeProcessed;

        // Olaylar
        public event Action<CombatUnit> OnUnitSelected;
        public event Action<CombatTurnPhase, int> OnTurnChanged;
        public event Action<bool> OnCombatEnded; // True: Zafer, False: Yenilgi
        public event Action<CombatUnit, CombatUnit> OnTargetEnemyFocused;
        public event Action OnTargetEnemyUnfocused;

        public CombatState CurrentState => _currentState;
        public int RoundNumber => _roundNumber;
        public CombatUnit SelectedUnit => _selectedUnit;
        public IReadOnlyList<CombatUnit> PlayerUnits => _playerUnits;
        public IReadOnlyList<CombatUnit> EnemyUnits => _enemyUnits;
        public string LastOutcomeSummary => _lastOutcomeSummary;

        private void Start()
        {
            if (_grid == null)
            {
                _grid = FindAnyObjectByType<HexGrid>();
            }

            if (_grid != null && _grid.Tiles.Count == 0)
            {
                _grid.GenerateGrid();
            }

            // Sahnede önceden dizilmiş birim yoksa GameSession.Caravan üzerinden dinamik kur
            _playerUnits.RemoveAll(u => u == null);
            _enemyUnits.RemoveAll(u => u == null);

            if (_playerUnits.Count == 0 && _enemyUnits.Count == 0)
            {
                SpawnBattlefieldUnits();
            }
            else
            {
                foreach (var u in _playerUnits) u.OnUnitDied += HandleUnitDied;
                foreach (var u in _enemyUnits) u.OnUnitDied += HandleUnitDied;
            }

            StartPlayerTurn();
        }

        /// <summary>
        /// Kervan kadrosundaki gerçek muhafızları sol kanada (q = 0, 1),
        /// pusuya düşüren haydut grubunu sağ kanada (q = 6, 7) yerleştirir.
        /// </summary>
        private void SpawnBattlefieldUnits()
        {
            if (_grid == null) return;

            Transform unitsRoot = new GameObject("BattleUnits").transform;
            unitsRoot.SetParent(transform, false);

            // 1. Oyuncu Kadrosu: Kıdemli Alp her zaman komutan olarak merkezde yer alır
            if (_alpUnitData != null)
            {
                SpawnSingleUnit(_alpUnitData, new HexCoordinates(1, 2), true, unitsRoot);
            }

            // GameSession.Caravan içindeki gerçek muhafızları oku
            IReadOnlyList<CaravanGuardRecord> guards = null;
            if (Services.TryGet<GameSession>(out var session) && session.Caravan != null)
            {
                guards = session.Caravan.Guards;
            }

            HexCoordinates[] meleeSlots = new[]
            {
                new HexCoordinates(1, 3),
                new HexCoordinates(1, 1),
                new HexCoordinates(1, 4),
                new HexCoordinates(1, 0)
            };

            HexCoordinates[] rangedSlots = new[]
            {
                new HexCoordinates(0, 2),
                new HexCoordinates(0, 3),
                new HexCoordinates(0, 1),
                new HexCoordinates(0, 4)
            };

            int meleeIdx = 0;
            int rangedIdx = 0;

            if (guards != null && guards.Count > 0)
            {
                for (int i = 0; i < guards.Count; i++)
                {
                    var record = guards[i];
                    if (record == null || record.UnitData == null || record.CurrentHealth <= 0) continue;

                    bool isRanged = record.UnitData.CombatRole == UnitCombatRole.Ranged;
                    HexCoordinates targetSlot;

                    if (isRanged && rangedIdx < rangedSlots.Length)
                    {
                        targetSlot = rangedSlots[rangedIdx++];
                    }
                    else if (meleeIdx < meleeSlots.Length)
                    {
                        targetSlot = meleeSlots[meleeIdx++];
                    }
                    else if (rangedIdx < rangedSlots.Length)
                    {
                        targetSlot = rangedSlots[rangedIdx++];
                    }
                    else
                    {
                        break; // Savaş tahtası sol kanat kapasitesi doldu
                    }

                    SpawnSingleUnit(
                        record.UnitData,
                        targetSlot,
                        true,
                        unitsRoot,
                        record.CurrentHealth,
                        record.CurrentMorale,
                        record.GuardId);
                }
            }
            else
            {
                // Doğrudan CombatScene'den test başlatıldıysa varsayılan 1 Muhafız + 1 Okçu yerleştir
                if (_defaultGuardData != null)
                {
                    SpawnSingleUnit(_defaultGuardData, meleeSlots[0], true, unitsRoot);
                }
                if (_defaultArcherData != null)
                {
                    SpawnSingleUnit(_defaultArcherData, rangedSlots[0], true, unitsRoot);
                }
            }

            // 2. Düşman Kadrosu: Oyuncu birliği büyüklüğüne göre dengeli haydut grubu (Sağ Kanat q = 6, 7)
            int enemyCount = Mathf.Clamp(_playerUnits.Count, 2, 5);
            HexCoordinates[] enemyMeleeSlots = new[]
            {
                new HexCoordinates(6, 2),
                new HexCoordinates(6, 3),
                new HexCoordinates(6, 1),
                new HexCoordinates(6, 4)
            };
            HexCoordinates[] enemyRangedSlots = new[]
            {
                new HexCoordinates(7, 2),
                new HexCoordinates(7, 3)
            };

            int eMelee = 0;
            int eRanged = 0;
            for (int i = 0; i < enemyCount; i++)
            {
                bool spawnArcher = (i % 2 == 1) && _banditArcherData != null && eRanged < enemyRangedSlots.Length;
                if (spawnArcher)
                {
                    SpawnSingleUnit(_banditArcherData, enemyRangedSlots[eRanged++], false, unitsRoot);
                }
                else if (_banditMeleeData != null && eMelee < enemyMeleeSlots.Length)
                {
                    SpawnSingleUnit(_banditMeleeData, enemyMeleeSlots[eMelee++], false, unitsRoot);
                }
            }
        }

        private CombatUnit SpawnSingleUnit(
            UnitDataSO unitData,
            HexCoordinates coords,
            bool isPlayerControlled,
            Transform parent,
            int overrideHealth = -1,
            int overrideMorale = -1,
            string guardId = null)
        {
            if (unitData == null || _grid == null) return null;

            HexTile tile = _grid.GetTile(coords);
            if (tile == null || !tile.IsWalkable)
            {
                return null;
            }

            GameObject unitObj = new GameObject($"Unit_{unitData.UnitName}_{coords.Q}_{coords.R}");
            unitObj.transform.SetParent(parent, false);
            unitObj.AddComponent<SpriteRenderer>();
            var combatUnit = unitObj.AddComponent<CombatUnit>();

            combatUnit.Initialize(
                unitData,
                coords,
                isPlayerControlled,
                _selectionRingSprite,
                overrideHealth,
                overrideMorale,
                guardId);

            tile.OccupyingUnit = combatUnit;
            RegisterUnit(combatUnit);
            return combatUnit;
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
                if (unit != null) unit.ResetTurn();
            }

            EventBus.Publish(new CombatTurnChangedEvent(CombatTurnPhase.PlayerTurn, _roundNumber));
            OnTurnChanged?.Invoke(CombatTurnPhase.PlayerTurn, _roundNumber);

            CombatUnit firstAlive = _playerUnits.Find(u => u != null && !u.IsDead);
            if (firstAlive != null)
            {
                SelectUnit(firstAlive);
            }
        }

        public void SelectUnit(CombatUnit unit)
        {
            if (_currentState != CombatState.PlayerTurn) return;
            if (unit == null || unit.IsDead) return;

            if (_selectedUnit != null && _selectedUnit != unit)
            {
                _selectedUnit.SetSelectedVisual(false);
            }

            _selectedUnit = unit;
            _selectedUnit.SetSelectedVisual(true);
            _previewTargetCoords = null;
            _grid.ClearAllHighlights();
            OnTargetEnemyUnfocused?.Invoke();

            OnUnitSelected?.Invoke(_selectedUnit);

            if (_selectedUnit.IsPlayerControlled)
            {
                UpdateMovementAndAttackHighlights();
            }
        }

        /// <summary>
        /// Seçili hibrit birimin (Alp) silah modunu (Kılıç <-> Yay) değiştirir ve menzil halkalarını günceller.
        /// </summary>
        public bool ToggleSelectedUnitWeaponMode()
        {
            if (_currentState != CombatState.PlayerTurn || _selectedUnit == null || _selectedUnit.IsDead)
            {
                return false;
            }

            bool isRanged = _selectedUnit.ToggleWeaponMode();
            _previewTargetCoords = null;
            OnTargetEnemyUnfocused?.Invoke();
            UpdateMovementAndAttackHighlights();
            return isRanged;
        }

        /// <summary>
        /// Seçili birimin özel taktik yeteneğini (Nara At: 3 Stamina ile dostlara +15 Moral ve Siper) çalıştırır.
        /// </summary>
        public bool ExecuteSelectedUnitSkill()
        {
            if (_currentState != CombatState.PlayerTurn || _selectedUnit == null || _selectedUnit.IsDead)
            {
                return false;
            }

            const int skillStaminaCost = 3;
            if (!_selectedUnit.SpendStamina(skillStaminaCost))
            {
                return false;
            }

            foreach (var ally in _playerUnits)
            {
                if (ally != null && !ally.IsDead)
                {
                    ally.AddMoraleBoost(15);
                }
            }

            _previewTargetCoords = null;
            OnTargetEnemyUnfocused?.Invoke();
            UpdateMovementAndAttackHighlights();
            return true;
        }

        private void UpdateMovementAndAttackHighlights()
        {
            if (_selectedUnit == null || _selectedUnit.IsDead) return;

            _grid.ClearAllHighlights();

            // 1. Kendi karesini altın rengi seçili yap
            _grid.HighlightTiles(new[] { _selectedUnit.Coordinates }, TileHighlightState.Selected);

            // 2. Yürüyebileceği altıgenleri hesapla (Turkuaz)
            _reachableTiles = HexPathfinder.GetReachableTiles(_grid, _selectedUnit.Coordinates, _selectedUnit.CurrentStamina);
            _grid.HighlightTiles(_reachableTiles.Keys, TileHighlightState.Reachable);

            // 3. Vurabileceği düşmanları kırmızı yap
            var attackableCoords = new List<HexCoordinates>();
            int attackCost = _selectedUnit.Data != null ? _selectedUnit.Data.StaminaCostAttack : 2;

            if (_selectedUnit.CurrentStamina >= attackCost)
            {
                int range = _selectedUnit.ActiveAttackRange;

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
        /// GDD §4 standardındaki iki dokunuşlu (Two-Tap Execution) girdi yönetimi.
        /// 1. Dokunuş: Hedef önizleme (Preview).
        /// 2. Dokunuş: Eylemi çalıştır (Execute).
        /// </summary>
        public void HandleTileClicked(HexCoordinates clickedCoords)
        {
            if (_currentState != CombatState.PlayerTurn || _selectedUnit == null) return;

            var clickedTile = _grid.GetTile(clickedCoords);
            if (clickedTile == null) return;

            // Tıklanan karede bir dost birim varsa onu seç
            if (clickedTile.OccupyingUnit != null && clickedTile.OccupyingUnit.IsPlayerControlled && !clickedTile.OccupyingUnit.IsDead)
            {
                SelectUnit(clickedTile.OccupyingUnit);
                return;
            }

            // Tıklanan karede düşman varsa: Saldırı akışı
            if (clickedTile.OccupyingUnit != null && !clickedTile.OccupyingUnit.IsPlayerControlled && !clickedTile.OccupyingUnit.IsDead)
            {
                var targetEnemy = clickedTile.OccupyingUnit;

                if (_selectedUnit.CanAttack(targetEnemy))
                {
                    if (_previewTargetCoords == clickedCoords)
                    {
                        _previewTargetCoords = null;
                        OnTargetEnemyUnfocused?.Invoke();
                        _selectedUnit.ExecuteAttack(targetEnemy);
                        UpdateMovementAndAttackHighlights();
                        CheckBattleOutcome();
                    }
                    else
                    {
                        _previewTargetCoords = clickedCoords;
                        UpdateMovementAndAttackHighlights();
                        clickedTile.SetHighlight(TileHighlightState.Selected);
                        OnTargetEnemyFocused?.Invoke(_selectedUnit, targetEnemy);
                    }
                }
                return;
            }

            // Tıklanan kare boş ve ulaşılabilir ise: Hareket akışı
            if (_reachableTiles.ContainsKey(clickedCoords))
            {
                OnTargetEnemyUnfocused?.Invoke();

                if (_previewTargetCoords == clickedCoords)
                {
                    _previewTargetCoords = null;
                    var path = HexPathfinder.FindPath(_grid, _selectedUnit.Coordinates, clickedCoords);
                    _selectedUnit.MoveAlongPath(path, _grid, () =>
                    {
                        UpdateMovementAndAttackHighlights();
                    });
                }
                else
                {
                    _previewTargetCoords = clickedCoords;
                    var path = HexPathfinder.FindPath(_grid, _selectedUnit.Coordinates, clickedCoords);
                    _grid.ClearAllHighlights();
                    _grid.HighlightTiles(new[] { _selectedUnit.Coordinates }, TileHighlightState.Selected);
                    _grid.HighlightTiles(_reachableTiles.Keys, TileHighlightState.Reachable);
                    _grid.HighlightTiles(path, TileHighlightState.PathPreview);
                }
            }
        }

        public void EndPlayerTurn()
        {
            if (CheckBattleOutcome()) return;
            if (_currentState != CombatState.PlayerTurn) return;

            if (_selectedUnit != null)
            {
                _selectedUnit.SetSelectedVisual(false);
            }

            _grid.ClearAllHighlights();
            _previewTargetCoords = null;
            _selectedUnit = null;
            OnTargetEnemyUnfocused?.Invoke();

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
                    enemy.SetSelectedVisual(true);
                    bool aiFinished = false;

                    yield return BanditAI.ExecuteAITurn(enemy, _grid, _playerUnits, () => aiFinished = true);

                    while (!aiFinished) yield return null;
                    enemy.SetSelectedVisual(false);
                    yield return new WaitForSeconds(0.3f);

                    if (CheckBattleOutcome()) yield break;
                }
            }

            if (CheckBattleOutcome()) yield break;

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
            if (_combatOutcomeProcessed)
            {
                return true;
            }

            bool allEnemiesDead = _enemyUnits.Count > 0 && _enemyUnits.TrueForAll(u => u == null || u.IsDead);
            if (allEnemiesDead)
            {
                TriggerCombatEnd(true);
                return true;
            }

            bool allPlayersDead = _playerUnits.Count > 0 && _playerUnits.TrueForAll(u => u == null || u.IsDead);
            if (allPlayersDead)
            {
                TriggerCombatEnd(false);
                return true;
            }

            return false;
        }

        private void TriggerCombatEnd(bool isVictory)
        {
            if (_combatOutcomeProcessed) return;
            _combatOutcomeProcessed = true;

            _currentState = isVictory ? CombatState.Victory : CombatState.Defeat;
            if (_selectedUnit != null)
            {
                _selectedUnit.SetSelectedVisual(false);
            }

            _grid.ClearAllHighlights();
            _previewTargetCoords = null;
            OnTargetEnemyUnfocused?.Invoke();

            _lastOutcomeSummary = ApplyCombatResultsToSession(isVictory);
            StartCoroutine(FinalizeCombatCoroutine(isVictory));
        }

        private IEnumerator FinalizeCombatCoroutine(bool isVictory)
        {
            // Son vuruş hamlesinin görsel olarak tamamlanması için kısa dramatik bekleme
            yield return new WaitForSeconds(0.55f);

            // Sahnede CombatResultUI kapalı (Inactive) bırakılmış olsa bile uyandır ve bağla
            var resultPanels = FindObjectsByType<CombatResultUI>(FindObjectsInactive.Include);
            for (int i = 0; i < resultPanels.Length; i++)
            {
                if (resultPanels[i] != null)
                {
                    resultPanels[i].ShowCombatResult(isVictory, _lastOutcomeSummary);
                }
            }

            OnCombatEnded?.Invoke(isVictory);
            EventBus.Publish(new CombatEndedEvent(isVictory));
            Debug.Log(isVictory
                ? "[CombatTurnManager] Zafer! Tüm haydutlar etkisiz hale getirildi."
                : "[CombatTurnManager] Yenilgi! Kervan muhafızları düştü.");
        }

        /// <summary>
        /// Savaş alanından geri çekilme (1.5s basılı tutma) tetiklendiğinde mevcut hasarları ve geri çekilme bedelini kervana işler.
        /// </summary>
        public void ApplyRetreatPenaltyToSession()
        {
            if (_combatOutcomeProcessed) return;
            _combatOutcomeProcessed = true;

            if (!Services.TryGet<GameSession>(out var session) || session.Caravan == null) return;
            var caravan = session.Caravan;

            for (int i = 0; i < _playerUnits.Count; i++)
            {
                var unit = _playerUnits[i];
                if (unit == null || string.IsNullOrWhiteSpace(unit.CaravanGuardId)) continue;
                caravan.SyncGuardAfterCombat(unit.CaravanGuardId, unit.CurrentHealth, unit.CurrentMorale, unit.IsDead);
            }

            caravan.ConsumeFood(1);
            if (Services.TryGet<SaveService>(out var saveService))
            {
                saveService.SaveSession(session);
            }
        }

        /// <summary>
        /// A5.4: Savaş sonucunu (sağ kalan muhafızların can/morali, şehitler, ganimet veya yağma) GameSession.Caravan'a işler.
        /// </summary>
        private string ApplyCombatResultsToSession(bool isVictory)
        {
            int defeatedEnemyCount = Mathf.Max(1, _enemyUnits.Count);
            int survivingGuards = 0;
            int fallenGuards = 0;

            GameSession session = null;
            Services.TryGet<GameSession>(out session);
            CaravanData caravan = session != null ? session.Caravan : null;

            for (int i = 0; i < _playerUnits.Count; i++)
            {
                var unit = _playerUnits[i];
                if (unit == null || string.IsNullOrWhiteSpace(unit.CaravanGuardId)) continue;

                if (unit.IsDead)
                {
                    fallenGuards++;
                }
                else
                {
                    survivingGuards++;
                }

                if (caravan != null)
                {
                    // Bozgun durumunda tüm muhafızlar silinmesin; ağır yaralı (5 Can) kurtulsun
                    bool removeGuard = isVictory && unit.IsDead;
                    int syncedHp = unit.IsDead ? 5 : unit.CurrentHealth;
                    int syncedMorale = unit.IsDead ? 15 : unit.CurrentMorale;
                    caravan.SyncGuardAfterCombat(unit.CaravanGuardId, syncedHp, syncedMorale, removeGuard);
                }
            }

            if (isVictory)
            {
                int lootAkce = defeatedEnemyCount * 25;
                int lootFood = Mathf.Clamp(defeatedEnemyCount, 1, 4);

                if (caravan != null)
                {
                    caravan.ModifyAkce(lootAkce);
                    caravan.AddFood(lootFood);
                }

                if (session != null && Services.TryGet<SaveService>(out var saveService))
                {
                    saveService.SaveSession(session);
                }

                string guardLine = fallenGuards > 0
                    ? $"Muhafız Durumu: {survivingGuards} Sağ, {fallenGuards} Kayıp"
                    : "Tüm kervan muhafızları mevzilerini korudu.";

                return $"Eşkıyalar püskürtüldü, kervan emniyete alındı!\n" +
                       $"Kazanılan Ganimet: +{lootAkce} Akçe · +{lootFood} Günlük Erzak\n" +
                       guardLine;
            }
            else
            {
                int currentAkce = caravan != null ? caravan.CurrentAkce : 0;
                int stolenAkce = Mathf.Min(currentAkce, 45);

                if (caravan != null)
                {
                    if (stolenAkce > 0) caravan.ModifyAkce(-stolenAkce);
                    caravan.ConsumeFood(2);
                }

                if (session != null && Services.TryGet<SaveService>(out var saveService))
                {
                    saveService.SaveSession(session);
                }

                string lossLine = stolenAkce > 0 ? $"-{stolenAkce} Akçe Yağmalandı · -2 Günlük Erzak" : "-2 Günlük Erzak Yağmalandı";
                return $"Kervan savunması yarıldı, muhafızlar ağır yaralı geri çekildi!\n{lossLine}";
            }
        }
    }
}
