# 📌 Proje Durumu (Pano)

> **Her oturumun başında okunan tek dosya.** Kısa tutulur (~80 satır). Geçmiş detaylar → `07-Degisiklik-Gunlugu.md`, adım detayları → `05-Yol-Haritasi.md`, sorunlar → `06-Bilinen-Sorunlar.md`.

**Proje:** Uç Beyliği Kervanı · 14. yy Anadolu · Taktiksel RPG & Ticaret
**Motor:** Unity 6 (6000.6.3f1), URP 2D · **Platform:** Android, yatay (landscape)
**Sürüm etiketi:** v0.5.0 Alpha · **Son güncelleme:** 05.11.2026

---

## 1. Gerçek Durum (Tek Bakışta)

| Alan | Durum | Not |
|---|---|---|
| Görsel / UI | 🟢 | MainMenu, WorldMap, TravelScene, TownHubScene, CombatScene (Altıgen Taktik Savaş + UnitHUD + ActionCluster + CombatHeaderUI + CombatResultUI), TopBar HUD ve Sidebar güncel. |
| Veri tanımları (SO) | 🟢 | 15 harita düğümü (`LocationNodeSO`), 23 çift yönlü rota (`RouteDatabaseSO`), 10 ticaret malı (`ItemDataSO`), `CityMarketDataSO`, `RoadEventSO` ve `UnitDataSO` bağlı ve senkron. |
| Oyun döngüsü | 🟢 | `00_Boot` + `GameSession` + `RouteGraph` + `GameCalendar` + `TravelManager` + `TownHubUI` + `CityMarket` + `CaravanseraiInnUI` + Pusu/Taktik Savaş (`CombatTurnManager`) döngüsü tam devrede (A2–A5). |
| Kalıcı durum / Kayıt | 🟢 | `GameSession` + `SaveService` (`save_0.json` atomik JSON: kervan envanteri, muhafız kadrosu/can/moral senkronu, binek kapasitesi, pazar doygunlukları) devrede. |
| Mobil teknik | 🟢 | Input System (`EnhancedTouch`), Android Geri tuşu, SafeArea ve ses kalıcılığı tamam (A2.0, A2.8). |

**Özet:** A5 aşaması (A5.1–A5.4: Olay/Pusu kararları, Manuel/Otomatik savaş seçimi, `CombatScene` altıgen taktik savaş yenilemesi ve savaş sonuçlarının kervana/kayda yansıyıp seyahate dönüşü) tamamlandı (`v0.5.0-alpha`). Sırada A6 (Meta: İstihbarat, İtibar, Dükkân) var.

**Sahneler:** `00_Boot` ✅ güncel · `MainMenuScene` ✅ güncel · `WorldMapScene` ✅ güncel · `TravelScene` ✅ A5 güncel · `TownHubScene` ✅ A4 tamamlandı · `CombatScene` ✅ A5.3–A5.4 tamamlandı · `MarketScene` ⚠️ arşiv/eski prototip.

---

## 2. Aşama Çizelgesi

```text
A0 Hazırlık & Veri Omurgası ............ ✅
A1 Ana Menü & UI Temelleri ............. ✅
A2 Dünya Haritası & Oyun Omurgası ...... ✅
A3 Seyahat (TravelScene) ............... ✅
A4 Şehir: Pazar/Kervansaray/Divan ...... ✅
A5 Olay & Pusu → Savaş ................. ✅
A6 Meta: İstihbarat/İtibar/Dükkân ...... ⏳  ← ŞU AN (A6.1 sırada)
A7 Cila (Dil, Performans, Temizlik) .... ⬜
A8 Android Sürüm & Test ................ ⬜
```

### A5 Adımları (Olay & Pusu → Savaş ✅)
- [x] A5.1 Olay penceresi: **Kaç / Haraç Ver / Savaş** (`EventModal` 3'lü karar kümesi, `RoadEventSO` `FleeChance` & `TributeAkceCost`, anlık etki ertelemesi)
- [x] A5.2 Savaş seçimi: Manuel / Otomatik (otomatik çözümleme formülü, iki kanatlı taktik kartlar `CombatModul`)
- [x] A5.3 CombatScene yeniden (`HexCoordinates` izometrik kafes kalibrasyonu, `HexGrid` siper/engel üretimi, dinamik birim dizilimi, `UnitHUD`, `ActionCluster`, `CombatHeaderUI`)
- [x] A5.4 Sonuç → kayıp/yaralanma → kervana yansıma → seyahate dönüş (`CaravanData.SyncGuardAfterCombat`, `CombatTurnManager.ApplyCombatResultsToSession`, `CombatResultUI`)

---

## 3. ▶️ Sıradaki Adım: A6.1 — Meta Yöneticilerinin (`ReputationManager`, `QuestManager`, `ShopManager`) `GameSession` Servislerine Taşınması
- **Hedef:** Dağınık Singleton olan `ReputationManager`, `QuestManager` ve `ShopManager` yapılarının `GameSession` / `Services` mimarisine ve `SaveData` kalıcılığına bağlanması (`TD-08`, `TD-11`, `TD-17`).
- **Detay & test senaryosu:** `05-Yol-Haritasi.md` → `## A6` / `### A6.1`

---

## 4. ⛔ Açık Kararlar
- Bekleyen açık karar yok (`KR-1..KR-5` onaylandı ve `01-Oyun-Tasarimi.md` §12'ye işlendi).

---

## 5. Kalıcı Notlar
- **Tasarım yönetimi ilkesi:** Tasarım konularında değişikliklere doğrudan gidilmez; tasarım işlemleri tamamen proje sahibinin kontrolünde ilerler. Danışman olarak olması gerekenler ve profesyonellik önerileri sunulur; ayrıca hangi yapay zekâ araçlarının veya programların (ve iş akışlarının) kolaylık sağlayacağı belirtilir.
- **Hatırlatma (Arayüz & Görsel Temizliği):** A4.R1 ile ham `.ai` dosyaları ve mükerrer klasörler temizlendi; son aşamalara doğru (A7) kalan ufak arayüz temizlikleri (`kısicon.png` ASCII adı), `BG_Travel_03_MidHiils.png` yatay birleşim izi (`AST-09`) ve kervan konvoyu birimlerinin gerçek bacak yürüyüş animasyonu (`AST-10`) ele alınacaktır.
- **Esneklik ilkesi:** Tamamlanmış sahnelerde de her zaman revizyon yapılabilir; "bu aşama bitti, dönülmez" denmez. Revizyon yeni bir adım olarak eklenir (ör. `A1.R1`).
- **Mobil test:** Her UI adımında Device Simulator ile 16:9, 19.5:9, 20:9 kontrolü.
- **Açık kritik sorun sayısı:** 🔴 0 · 🟠 2 (bkz. `06`)
