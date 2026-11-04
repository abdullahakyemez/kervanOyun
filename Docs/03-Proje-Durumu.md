# 📌 Proje Durumu (Pano)

> **Her oturumun başında okunan tek dosya.** Kısa tutulur (~80 satır). Geçmiş detaylar → `07-Degisiklik-Gunlugu.md`, adım detayları → `05-Yol-Haritasi.md`, sorunlar → `06-Bilinen-Sorunlar.md`.

**Proje:** Uç Beyliği Kervanı · 14. yy Anadolu · Taktiksel RPG & Ticaret
**Motor:** Unity 6 (6000.6.3f1), URP 2D · **Platform:** Android, yatay (landscape)
**Sürüm etiketi:** v0.4.0 Alpha · **Son güncelleme:** 04.11.2026

---

## 1. Gerçek Durum (Tek Bakışta)

| Alan | Durum | Not |
|---|---|---|
| Görsel / UI | 🟢 | MainMenu, WorldMap, TravelScene, TownHubScene (Pazar + Kervansaray + Divan), TopBar HUD, Sidebar ve Savaş Tarzı Seçim Kartları (`CombatModul`) güncel. |
| Veri tanımları (SO) | 🟢 | 15 harita düğümü (`LocationNodeSO`), 23 çift yönlü rota (`RouteDatabaseSO`), 10 ticaret malı (`ItemDataSO`), `CityMarketDataSO` ve `RoadEventSO` bağlı ve senkron. |
| Oyun döngüsü | 🟢 | `00_Boot` + `GameSession` + `RouteGraph` + `GameCalendar` + `TravelManager` + `TownHubUI` + `CityMarket` + `CaravanseraiInnUI` + Savaş Seçimi döngüsü devrede (A2–A5.2). |
| Kalıcı durum / Kayıt | 🟢 | `GameSession` + `SaveService` (`save_0.json` atomik JSON: kervan envanteri, muhafız kadrosu, binek kapasitesi, pazar doygunlukları) devrede. |
| Mobil teknik | 🟢 | Input System (`EnhancedTouch`), Android Geri tuşu, SafeArea ve ses kalıcılığı tamam (A2.0, A2.8). |

**Özet:** A5.2 (Manuel / Otomatik savaş seçimi ve taktik çözümleme formülü) tamamlandı. Sırada A5.3 (CombatScene yeniden: Hex Combat HUD + Action Cluster) var.

**Sahneler:** `00_Boot` ✅ güncel · `MainMenuScene` ✅ güncel · `WorldMapScene` ✅ güncel · `TravelScene` ✅ A5.2 güncel · `TownHubScene` ✅ A4 tamamlandı · `CombatScene` ⏳ A5.3'te yeniden ele alınacak · `MarketScene` ⚠️ arşiv/eski prototip.

---

## 2. Aşama Çizelgesi

```text
A0 Hazırlık & Veri Omurgası ............ ✅
A1 Ana Menü & UI Temelleri ............. ✅
A2 Dünya Haritası & Oyun Omurgası ...... ✅
A3 Seyahat (TravelScene) ............... ✅
A4 Şehir: Pazar/Kervansaray/Divan ...... ✅
A5 Olay & Pusu → Savaş ................. ⏳  ← ŞU AN (A5.2 bitti)
A6 Meta: İstihbarat/İtibar/Dükkân ...... ⬜
A7 Cila (Dil, Performans, Temizlik) .... ⬜
A8 Android Sürüm & Test ................ ⬜
```

### A5 Adımları (Olay & Pusu → Savaş ⏳)
- [x] A5.1 Olay penceresi: **Kaç / Haraç Ver / Savaş** (`EventModal` 3'lü karar kümesi, `RoadEventSO` `FleeChance` & `TributeAkceCost`, anlık etki ertelemesi)
- [x] A5.2 Savaş seçimi: Manuel / Otomatik (otomatik çözümleme formülü, iki kanatlı taktik kartlar `CombatModul`)
- [ ] A5.3 CombatScene yeniden (mevcut `DamageCalculator`, `HexPathfinder` korunur), GDD Unit HUD + Action Cluster yerleşimi
- [ ] A5.4 Sonuç → kayıp/yaralanma → kervana yansıma → seyahate dönüş

---

## 3. ▶️ Sıradaki Adım: A5.3 — CombatScene Yeniden (Hex Combat HUD & Action Cluster)
- **Hedef:** Altıgen savaş tahtasının (`CombatScene`) GDD §6 ve §11 standartlarına göre düzenlenmesi; sıra tabanlı çatışma arayüzü, Unit HUD (can/moral/stamina), Action Cluster (saldır/savun/menzilli) yerleşimi.
- **Detay & test senaryosu:** `05-Yol-Haritasi.md` → `## A5` / `### A5.3`

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
