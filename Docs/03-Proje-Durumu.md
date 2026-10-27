# 📌 Proje Durumu (Pano)

> **Her oturumun başında okunan tek dosya.** Kısa tutulur (~80 satır). Geçmiş detaylar → `07-Degisiklik-Gunlugu.md`, adım detayları → `05-Yol-Haritasi.md`, sorunlar → `06-Bilinen-Sorunlar.md`.

**Proje:** Uç Beyliği Kervanı · 14. yy Anadolu · Taktiksel RPG & Ticaret
**Motor:** Unity 6 (6000.6.3f1), URP 2D · **Platform:** Android, yatay (landscape)
**Sürüm etiketi:** v0.2.0 Alpha · **Son güncelleme:** 27.10.2026

---

## 1. Gerçek Durum (Tek Bakışta)

| Alan | Durum | Not |
|---|---|---|
| Görsel / UI | 🟢 | MainMenu, WorldMap, TopBar HUD ve Kenar Çubuğu gerçek verilerle hazır. |
| Veri tanımları (SO) | 🟢 | 13 harita düğümü `MapNodeView` + `LocationNodeSO` (ASCII `NodeId`) ve `RouteDatabaseSO` ile bağlı (A2.1, A2.4). |
| Oyun döngüsü | 🟢 | `00_Boot` + `GameSession` + `Services` + `RouteGraph` + `GameCalendar` takvim/mevsim omurgası kuruldu (A2.2, A2.4, A2.5). |
| Kalıcı durum / Kayıt | 🟢 | `GameSession` + `SaveService` (`save_0.json` atomik JSON kayıt/yükleme ve otomatik kayıt) tamamlandı (A2.3). |
| Mobil teknik | 🟢 | Input System (`EnhancedTouch`), Android Geri tuşu, SafeArea ve ses kalıcılığı tamam (A2.0, A2.8). |

**Özet:** A2 (Dünya Haritası & Oyun Omurgası) aşaması başarıyla tamamlandı (`v0.2.0-alpha`). Oturum, kayıt, harita, rota grafiği, HUD, sahne geçişleri ve mobil uyumluluk mühürlendi; sırada A3 (Seyahat - TravelScene yeniden) aşaması var.

**Sahneler:** `00_Boot` ✅ güncel · `MainMenuScene` ✅ güncel · `WorldMapScene` ✅ güncel · `TravelScene` ✅ güncel · `MarketScene`, `TownHubScene`, `CombatScene` ⚠️ eski prototip (yeniden ele alınacak).

---

## 2. Aşama Çizelgesi

```text
A0 Hazırlık & Veri Omurgası ............ ✅
A1 Ana Menü & UI Temelleri ............. ✅
A2 Dünya Haritası & Oyun Omurgası ...... ✅
A3 Seyahat (TravelScene) ............... ⏳  ← ŞU AN
A4 Şehir: Pazar/Kervansaray/Divan ...... ⬜
A5 Olay & Pusu → Savaş ................. ⬜
A6 Meta: İstihbarat/İtibar/Dükkân ...... ⬜
A7 Cila (Dil, Performans, Temizlik) .... ⬜
A8 Android Sürüm & Test ................ ⬜
```

### A3 Adımları
- [x] A3.1 Seyahat tasarım önerisi & 4 katmanlı Parallax arka plan kurulumu (`TravelParallaxUI`)
- [x] A3.2 Kervan yürüyüş konvoyu (`CaravanConvoyUI`) & Seyahat HUD yerleşimi (TopBar, İlerleme, Mola/Tempo)
- [x] A3.3 `TravelManager` & `GameSession` bağlantısı (`BUG-05` çözümü), varış ve haritaya dönüş
- [ ] A3.4 Kamp/Mola penceresi & Yol olayları altyapısı (`RoadEventSO`)

---

## 3. ▶️ Sıradaki Adım: A3.4 — Kamp/Mola Penceresi & Yol Olayları Altyapısı (`RoadEventSO`)
- **Hedef:** Seyahat sırasında tetiklenen yol hadiseleri / varış penceresi (`EventModal`) ve A5'e zemin hazırlayan `RoadEventSO` veri yapısının kurulması, ardından A3 aşama kapanışı.
- **Detay & test senaryosu:** `05-Yol-Haritasi.md` → `### A3.4`

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
