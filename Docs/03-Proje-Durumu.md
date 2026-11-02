# 📌 Proje Durumu (Pano)

> **Her oturumun başında okunan tek dosya.** Kısa tutulur (~80 satır). Geçmiş detaylar → `07-Degisiklik-Gunlugu.md`, adım detayları → `05-Yol-Haritasi.md`, sorunlar → `06-Bilinen-Sorunlar.md`.

**Proje:** Uç Beyliği Kervanı · 14. yy Anadolu · Taktiksel RPG & Ticaret
**Motor:** Unity 6 (6000.6.3f1), URP 2D · **Platform:** Android, yatay (landscape)
**Sürüm etiketi:** v0.4.0 Alpha · **Son güncelleme:** 02.11.2026

---

## 1. Gerçek Durum (Tek Bakışta)

| Alan | Durum | Not |
|---|---|---|
| Görsel / UI | 🟢 | MainMenu, WorldMap, TravelScene, TownHubScene (Pazar + Kervansaray + Divan), TopBar HUD ve Sidebar yeni görsel varlıklarla (`yeniAssets`) güncellendi; ~49 MB atıl dosya temizlendi. |
| Veri tanımları (SO) | 🟢 | 15 harita düğümü (`LocationNodeSO`), 23 çift yönlü rota (`RouteDatabaseSO`), 10 ticaret malı (`ItemDataSO`), `CityMarketDataSO` ve `RoadEventSO` bağlı ve senkron. |
| Oyun döngüsü | 🟢 | `00_Boot` + `GameSession` + `RouteGraph` + `GameCalendar` + `TravelManager` + `TownHubUI` + `CityMarket` + `CaravanseraiInnUI` döngüsü tamamlandı (A2–A4). |
| Kalıcı durum / Kayıt | 🟢 | `GameSession` + `SaveService` (`save_0.json` atomik JSON: kervan envanteri, muhafız kadrosu, binek kapasitesi, pazar doygunlukları) devrede. |
| Mobil teknik | 🟢 | Input System (`EnhancedTouch`), Android Geri tuşu, SafeArea ve ses kalıcılığı tamam (A2.0, A2.8). |

**Özet:** A4 (Şehir) tamamlandı. A4.R1 ara adımıyla tüm sahnelerin UI tasarımları yeni varlıklarla revize edildi, kart buton rengi koruması sağlandı, rota veritabanı senkronize edildi ve 49 MB atıl dosya temizlendi. Sırada A5.2 (Manuel / Otomatik savaş seçimi) var.

**Sahneler:** `00_Boot` ✅ güncel · `MainMenuScene` ✅ güncel · `WorldMapScene` ✅ güncel · `TravelScene` ✅ A5.1 güncel · `TownHubScene` ✅ A4 tamamlandı · `CombatScene` ⚠️ A5'te yeniden ele alınacak · `MarketScene` ⚠️ arşiv/eski prototip.

---

## 2. Aşama Çizelgesi

```text
A0 Hazırlık & Veri Omurgası ............ ✅
A1 Ana Menü & UI Temelleri ............. ✅
A2 Dünya Haritası & Oyun Omurgası ...... ✅
A3 Seyahat (TravelScene) ............... ✅
A4 Şehir: Pazar/Kervansaray/Divan ...... ✅
A5 Olay & Pusu → Savaş ................. ⏳  ← ŞU AN (A5.1 bitti)
A6 Meta: İstihbarat/İtibar/Dükkân ...... ⬜
A7 Cila (Dil, Performans, Temizlik) .... ⬜
A8 Android Sürüm & Test ................ ⬜
```

### A5 Adımları (Olay & Pusu → Savaş ⏳)
- [x] A5.1 Olay penceresi: **Kaç / Haraç Ver / Savaş** (`EventModal` 3'lü karar kümesi, `RoadEventSO` `FleeChance` & `TributeAkceCost`, anlık etki ertelemesi)
- [ ] A5.2 Savaş seçimi: Manuel / Otomatik (otomatik çözümleme formülü)
- [ ] A5.3 CombatScene yeniden (mevcut `DamageCalculator`, `HexPathfinder` korunur), GDD Unit HUD + Action Cluster yerleşimi
- [ ] A5.4 Sonuç → kayıp/yaralanma → kervana yansıma → seyahate dönüş

---

## 3. ▶️ Sıradaki Adım: A5.2 — Savaş Seçimi: Manuel / Otomatik (GDD §11/5)
- **Hedef:** Pusu veya tehlikeli olayda "Savaş" seçildiğinde oyuncuya iki seçenek sunulması: **Manuel Taktik Savaş** (`CombatScene` üzerinde sıra tabanlı altıgen çatışma) veya **Otomatik Çözümleme** (kervanın muhafız sayısı/canı ile düşman gücünü kıyaslayan hızlı sonuç algoritması).
- **Detay & test senaryosu:** `05-Yol-Haritasi.md` → `## A5` / `### A5.2`

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
