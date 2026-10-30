# 📌 Proje Durumu (Pano)

> **Her oturumun başında okunan tek dosya.** Kısa tutulur (~80 satır). Geçmiş detaylar → `07-Degisiklik-Gunlugu.md`, adım detayları → `05-Yol-Haritasi.md`, sorunlar → `06-Bilinen-Sorunlar.md`.

**Proje:** Uç Beyliği Kervanı · 14. yy Anadolu · Taktiksel RPG & Ticaret
**Motor:** Unity 6 (6000.6.3f1), URP 2D · **Platform:** Android, yatay (landscape)
**Sürüm etiketi:** v0.3.0 Alpha · **Son güncelleme:** 30.10.2026

---

## 1. Gerçek Durum (Tek Bakışta)

| Alan | Durum | Not |
|---|---|---|
| Görsel / UI | 🟢 | MainMenu, WorldMap, TravelScene, TownHubScene (Pazar alış-satış UI + Kervansaray/Divan mizanpajı), TopBar HUD ve Sidebar hazır. |
| Veri tanımları (SO) | 🟢 | 13 düğüm (`LocationNodeSO`), 10 ticaret malı (`ItemDataSO`), `CityMarketDataSO`, `RouteDatabaseSO` ve `RoadEventSO` bağlı. |
| Oyun döngüsü | 🟢 | `00_Boot` + `GameSession` + `RouteGraph` + `GameCalendar` + `TravelManager` + `TownHubUI` + `CityMarket` döngüsü kuruldu (A2–A4.2). |
| Kalıcı durum / Kayıt | 🟢 | `GameSession` + `SaveService` (`save_0.json` atomik JSON kayıt/yükleme, kervan envanteri `InventorySlotSaveData` geri yükleme) tamamlandı. |
| Mobil teknik | 🟢 | Input System (`EnhancedTouch`), Android Geri tuşu, SafeArea ve ses kalıcılığı tamam (A2.0, A2.8). |

**Özet:** A4.2 (Pazar Sistemi, 4 Yeni Ticaret Malı & Alış-Satış UI) tamamlandı. 10 ticaret malı (`ItemDataSO`), `CityMarket` ↔ `GameSession` bağlantısı, ortalama alış maliyeti (`AveragePurchasePrice`) üzerinden `+%Kâr ▲` / `-%Zarar ▼` rozetli Pazar arayüzü (`MarketUI`) devreye alındı; sırada A4.3 (Pazar Doygunluğu, Kalıcı Kayıt ve Günlük Toparlanma) adımı var.

**Sahneler:** `00_Boot` ✅ güncel · `MainMenuScene` ✅ güncel · `WorldMapScene` ✅ güncel · `TravelScene` ✅ güncel · `TownHubScene` ⏳ A4.1–A4.2 hazır · `MarketScene`, `CombatScene` ⚠️ eski prototip (yeniden ele alınacak).

---

## 2. Aşama Çizelgesi

```text
A0 Hazırlık & Veri Omurgası ............ ✅
A1 Ana Menü & UI Temelleri ............. ✅
A2 Dünya Haritası & Oyun Omurgası ...... ✅
A3 Seyahat (TravelScene) ............... ✅
A4 Şehir: Pazar/Kervansaray/Divan ...... ⏳  ← ŞU AN
A5 Olay & Pusu → Savaş ................. ⬜
A6 Meta: İstihbarat/İtibar/Dükkân ...... ⬜
A7 Cila (Dil, Performans, Temizlik) .... ⬜
A8 Android Sürüm & Test ................ ⬜
```

### A3 Adımları (Tamamlandı ✅)
- [x] A3.1 Seyahat tasarım önerisi & 4 katmanlı Parallax arka plan kurulumu (`TravelParallaxUI`)
- [x] A3.2 Kervan yürüyüş konvoyu (`CaravanConvoyUI`) & Seyahat HUD yerleşimi (TopBar, İlerleme, Mola/Tempo)
- [x] A3.3 `TravelManager` & `GameSession` bağlantısı (`BUG-05` çözümü), varış ve haritaya dönüş
- [x] A3.4 Kamp/Mola penceresi (`EventModal`) & Yol olayları altyapısı (`RoadEventSO`)

### A4 Adımları
- [x] A4.1 Şehir Hub tasarımı & 3 sekmeli sahne iskeleti (`TownHubScene`: Pazar / Kervansaray / Divan özel arka planları ve sayfa bazlı UI)
- [x] A4.2 Pazar: 13 düğüm `CityMarketDataSO`, 4 yeni `ItemDataSO`, `CityMarket` ↔ `GameSession` bağlantısı, alış-satış UI, kâr/zarar göstergesi
- [ ] A4.3 Pazar doygunluğu (`MarketSaturation`), kalıcı kayıt (`SaveData`) ve gün geçtikçe toparlanma (GDD §4)
- [ ] A4.4 Kervansaray: dinlenme/iyileşme, erzak ikmali, muhafız ve yük hayvanı kiralama

---

## 3. ▶️ Sıradaki Adım: A4.3 — Pazar Doygunluğu (`MarketSaturation`), Kalıcı Kayıt ve Günlük Toparlanma
- **Hedef:** Şehir pazarlarındaki anlık stok ve doygunluk (`Saturation`) değerlerinin `GameSession` ve `SaveData` (`save_0.json`) içinde şehir bazlı kalıcı saklanması; gün geçtikçe (`DayPassedEvent`) piyasaların kademeli olarak başlangıç dengesine toparlanması (GDD §4 & §10).
- **Önemli Hatırlatma (Sonraki Oturum İçin):** `TownHubScene` içinde `Panel_Market`, `Panel_Caravanserai` ve `Panel_Divan` görsel sahne mizanpajları hazırlandı. **A4.3** (saf C# pazar kalıcılığı) tamamlanır tamamlanmaz **A4.4** adımında kurulan `Panel_Caravanserai` (Konaklama/Erzak, Muhafız Ocağı, Ahır, Dükkân/Casus) ve `Panel_Divan` (İtibar & Görevler) panellerinin kod ve Inspector bağlantıları yapılacak.
- **Detay & test senaryosu:** `05-Yol-Haritasi.md` → `### A4.3` ve `### A4.4`

---

## 4. ⛔ Açık Kararlar
- Bekleyen açık karar yok (`KR-1..KR-5` onaylandı ve `01-Oyun-Tasarimi.md` §12'ye işlendi).

---

## 5. Kalıcı Notlar
- **Tasarım yönetimi ilkesi:** Tasarım konularında değişikliklere doğrudan gidilmez; tasarım işlemleri tamamen proje sahibinin kontrolünde ilerler. Danışman olarak olması gerekenler ve profesyonellik önerileri sunulur; ayrıca hangi yapay zekâ araçlarının veya programların (ve iş akışlarının) kolaylık sağlayacağı belirtilir.
- **Hatırlatma (Arayüz & Görsel Temizliği):** Son aşamalara doğru (A7) ufak arayüz temizlikleri (`kısicon.png` ASCII adı, mükerrer `Toolbar` klasörü, ham `Toolbar.ai` dosyası), `BG_Travel_03_MidHiils.png` yatay birleşim izi (`AST-09`) ve kervan konvoyu birimlerinin gerçek bacak yürüyüş animasyonu (`AST-10`) ele alınacaktır.
- **Esneklik ilkesi:** Tamamlanmış sahnelerde de her zaman revizyon yapılabilir; "bu aşama bitti, dönülmez" denmez. Revizyon yeni bir adım olarak eklenir (ör. `A1.R1`).
- **Mobil test:** Her UI adımında Device Simulator ile 16:9, 19.5:9, 20:9 kontrolü.
- **Açık kritik sorun sayısı:** 🔴 0 · 🟠 2 (bkz. `06`)
