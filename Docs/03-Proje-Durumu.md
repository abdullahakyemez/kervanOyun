# 📌 Proje Durumu (Pano)

> **Her oturumun başında okunan tek dosya.** Kısa tutulur (~80 satır). Geçmiş detaylar → `07-Degisiklik-Gunlugu.md`, adım detayları → `05-Yol-Haritasi.md`, sorunlar → `06-Bilinen-Sorunlar.md`.

**Proje:** Uç Beyliği Kervanı · 14. yy Anadolu · Taktiksel RPG & Ticaret
**Motor:** Unity 6 (6000.6.3f1), URP 2D · **Platform:** Android, yatay (landscape)
**Sürüm etiketi:** v0.4.0 Alpha · **Son güncelleme:** 31.10.2026

---

## 1. Gerçek Durum (Tek Bakışta)

| Alan | Durum | Not |
|---|---|---|
| Görsel / UI | 🟢 | MainMenu, WorldMap, TravelScene, TownHubScene (Pazar + Kervansaray + Divan mizanpajı), TopBar HUD ve Sidebar hazır. |
| Veri tanımları (SO) | 🟢 | 13 düğüm (`LocationNodeSO`), 10 ticaret malı (`ItemDataSO`), `CityMarketDataSO`, `RouteDatabaseSO` ve `RoadEventSO` bağlı. |
| Oyun döngüsü | 🟢 | `00_Boot` + `GameSession` + `RouteGraph` + `GameCalendar` + `TravelManager` + `TownHubUI` + `CityMarket` + `CaravanseraiInnUI` döngüsü tamamlandı (A2–A4). |
| Kalıcı durum / Kayıt | 🟢 | `GameSession` + `SaveService` (`save_0.json` atomik JSON: kervan envanteri, muhafız kadrosu, binek kapasitesi, pazar doygunlukları) devrede. |
| Mobil teknik | 🟢 | Input System (`EnhancedTouch`), Android Geri tuşu, SafeArea ve ses kalıcılığı tamam (A2.0, A2.8). |

**Özet:** A4 (Şehir: Pazar + Kervansaray + Divan) aşaması başarıyla tamamlandı. Dinamik pazar doygunluğu/kurtarma, handa konaklama (can/moral/gün geçişi), erzak ikmali, muhafız/binek kiralama ve casusluk istihbaratı çalışır durumda. Sırada A5 (Olay & Pusu → Savaş) aşaması var.

**Sahneler:** `00_Boot` ✅ güncel · `MainMenuScene` ✅ güncel · `WorldMapScene` ✅ güncel · `TravelScene` ✅ güncel · `TownHubScene` ✅ A4 tamamlandı · `CombatScene` ⚠️ A5'te yeniden ele alınacak · `MarketScene` ⚠️ arşiv/eski prototip.

---

## 2. Aşama Çizelgesi

```text
A0 Hazırlık & Veri Omurgası ............ ✅
A1 Ana Menü & UI Temelleri ............. ✅
A2 Dünya Haritası & Oyun Omurgası ...... ✅
A3 Seyahat (TravelScene) ............... ✅
A4 Şehir: Pazar/Kervansaray/Divan ...... ✅
A5 Olay & Pusu → Savaş ................. ⏳  ← ŞU AN
A6 Meta: İstihbarat/İtibar/Dükkân ...... ⬜
A7 Cila (Dil, Performans, Temizlik) .... ⬜
A8 Android Sürüm & Test ................ ⬜
```

### A4 Adımları (Tamamlandı ✅)
- [x] A4.1 Şehir Hub tasarımı & 3 sekmeli sahne iskeleti (`TownHubScene`: Pazar / Kervansaray / Divan özel arka planları ve sayfa bazlı UI)
- [x] A4.2 Pazar: 13 düğüm `CityMarketDataSO`, 4 yeni `ItemDataSO`, `CityMarket` ↔ `GameSession` bağlantısı, alış-satış UI, kâr/zarar göstergesi
- [x] A4.3 Pazar doygunluğu (`MarketSaturation`), kalıcı kayıt (`SaveData`) ve gün geçtikçe toparlanma (GDD §4)
- [x] A4.4 Kervansaray: dinlenme/iyileşme, erzak ikmali, muhafız ve yük hayvanı kiralama, casus istihbaratı

---

## 3. ▶️ Sıradaki Adım: A5.1 — Olay Penceresi: Kaç / Haraç Ver / Savaş (GDD §11/5)
- **Hedef:** Seyahat sırasında çıkan haydut pusularında ve tehlikeli yol olaylarında oyuncuya 3 seçenek sunan karar penceresinin (`EventModal`) kurulması: **Kaç** (hız/çeviklik kontrolü), **Haraç Ver** (akçe veya yük kaybı) ve **Savaş** (manuel taktik savaş veya otomatik çözümleme seçimine geçiş).
- **Önemli Hatırlatma:** Seyahatteki `EventModal` altyapısı genişletilecek, `RoadEventSO` olaylarına seçenek (`EventChoice`) desteği eklenecek.
- **Detay & test senaryosu:** `05-Yol-Haritasi.md` → `## A5` / `### A5.1`

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
