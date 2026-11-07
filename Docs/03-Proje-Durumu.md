# 📌 Proje Durumu (Pano)

> **Her oturumun başında okunan tek dosya.** Kısa tutulur (~80 satır). Geçmiş detaylar → `07-Degisiklik-Gunlugu.md`, adım detayları → `05-Yol-Haritasi.md`, sorunlar → `06-Bilinen-Sorunlar.md`.

**Proje:** Uç Beyliği Kervanı · 14. yy Anadolu · Taktiksel RPG & Ticaret
**Motor:** Unity 6 (6000.6.3f1), URP 2D · **Platform:** Android, yatay (landscape)
**Sürüm etiketi:** v0.5.0 Alpha · **Son güncelleme:** 08.11.2026

---

## 1. Gerçek Durum (Tek Bakışta)

| Alan | Durum | Not |
|---|---|---|
| Görsel / UI | 🟢 | MainMenu, WorldMap, TravelScene, TownHubScene, CombatScene (Altıgen Taktik Savaş + UnitHUD + ActionCluster + CombatHeaderUI + CombatResultUI), TopBar HUD ve Sidebar güncel. |
| Veri tanımları (SO) | 🟢 | 15 harita düğümü (`LocationNodeSO`), 23 çift yönlü rota (`RouteDatabaseSO`), 10 ticaret malı (`ItemDataSO`), `CityMarketDataSO`, `RoadEventSO`, `UnitDataSO` ve `IntelDataSO` bağlı ve senkron. |
| Oyun döngüsü | 🟢 | `00_Boot` + `GameSession` + `RouteGraph` + `GameCalendar` + `TravelManager` + `TownHubUI` + `CityMarket` + `CaravanseraiInnUI` + Pusu/Taktik Savaş + Meta servisleri (`ReputationManager`, `QuestManager`, `ShopManager`) devrede (A2–A6.1). |
| Kalıcı durum / Kayıt | 🟢 | `GameSession` + `SaveService` (`save_0.json` atomik JSON: kervan envanteri, muhafız kadrosu/can/moral, binek kapasitesi, pazar doygunlukları, fraksiyon itibarları, istihbaratlar ve han dükkânları) devrede. |
| Mobil teknik | 🟢 | Input System (`EnhancedTouch`), Android Geri tuşu, SafeArea ve ses kalıcılığı tamam (A2.0, A2.8). |

**Özet:** A6.1 adımı (`ReputationManager`, `QuestManager`, `ShopManager` saf C# servislerine dönüşümü, `GameSession` ve `SaveData` kalıcılığı) tamamlandı. Sırada A6.2 (Divan: İtibar göstergeleri, istihbarat toplama/teslimi ve ödüller) var.

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
A6 Meta: İstihbarat/İtibar/Dükkân ...... ⏳  ← ŞU AN (A6.2 sırada)
A7 Cila (Dil, Performans, Temizlik) .... ⬜
A8 Android Sürüm & Test ................ ⬜
```

### A6 Adımları (Meta: İstihbarat, İtibar, Dükkân ⏳)
- [x] A6.1 Meta yöneticilerinin (`ReputationManager`, `QuestManager`, `ShopManager`) `GameSession` servislerine ve `SaveData` kalıcılığına taşınması (`TD-08`, `TD-11`, `TD-17`)
- [ ] A6.2 Divan: beylik itibar göstergeleri, istihbarat toplama/teslimi ve ödüller
- [ ] A6.3 Sabit han dükkânı / pazar tezgâhı satın alma ve pasif gelir tahsilatı

---

## 3. ▶️ Sıradaki Adım: A6.2 — Divan: Beylik İtibar Göstergeleri, İstihbarat Toplama/Teslimi ve Ödüller
- **Hedef:** `TownHubScene` içindeki Divan sekmesinin (`Panel_Divan`) ve Kervansaray casusluk (`Btn_HireSpy`) akışının bağlanması; oyuncunun şehirlerden istihbarat (`IntelDataSO`) toplayıp Divan'da arz ederek Akçe ve Beylik İtibarı kazanması, itibar kademesine göre pazar vergi indirimi/cezasının uygulanması.
- **Detay & test senaryosu:** `05-Yol-Haritasi.md` → `## A6` / `### A6.2`

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
