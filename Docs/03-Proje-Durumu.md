# 📌 Proje Durumu (Pano)

> **Her oturumun başında okunan tek dosya.** Kısa tutulur (~80 satır). Geçmiş detaylar → `07-Degisiklik-Gunlugu.md`, adım detayları → `05-Yol-Haritasi.md`, sorunlar → `06-Bilinen-Sorunlar.md`.

**Proje:** Uç Beyliği Kervanı · 14. yy Anadolu · Taktiksel RPG & Ticaret
**Motor:** Unity 6 (6000.6.3f1), URP 2D · **Platform:** Android, yatay (landscape)
**Sürüm etiketi:** v0.1.0 Alpha · **Son güncelleme:** 25.10.2026

---

## 1. Gerçek Durum (Tek Bakışta)

| Alan | Durum | Not |
|---|---|---|
| Görsel / UI | 🟢 | MainMenu, WorldMap ve TopBar HUD (Akçe, Erzak, Tarih, Mevsim) hazır. |
| Veri tanımları (SO) | 🟢 | 13 harita düğümü `MapNodeView` + `LocationNodeSO` (ASCII `NodeId`) ve `RouteDatabaseSO` ile bağlı (A2.1, A2.4). |
| Oyun döngüsü | 🟢 | `00_Boot` + `GameSession` + `Services` + `RouteGraph` + `GameCalendar` takvim/mevsim omurgası kuruldu (A2.2, A2.4, A2.5). |
| Kalıcı durum / Kayıt | 🟢 | `GameSession` + `SaveService` (`save_0.json` atomik JSON kayıt/yükleme ve otomatik kayıt) tamamlandı (A2.3). |
| Mobil teknik | 🟢 | Input System (`EnhancedTouch`), DevOverlay koruması ve ses kalıcılığı tamam (A2.0). |

**Özet:** Görsel, düğüm kimlik yapısı, çalışma anı oturumu (`00_Boot` + `GameSession` + `Services`), atomik JSON kayıt/yükleme (`SaveService`), rota grafiği (`RouteGraph` + `RouteDatabaseSO`) ve TopBar HUD (`TopBarHUD` + `GameCalendar` + `SeasonCapsule`) hazır; sırada sahne geçiş servisi (`A2.6`) var.

**Sahneler:** `00_Boot` ✅ güncel · `MainMenuScene` ✅ güncel · `WorldMapScene` ✅ güncel · `TravelScene`, `MarketScene`, `TownHubScene`, `CombatScene` ⚠️ eski prototip (yeniden ele alınacak).

---

## 2. Aşama Çizelgesi

```text
A0 Hazırlık & Veri Omurgası ............ ✅
A1 Ana Menü & UI Temelleri ............. ✅
A2 Dünya Haritası & Oyun Omurgası ...... ⏳  ← ŞU AN
A3 Seyahat (TravelScene) ............... ⬜
A4 Şehir: Pazar/Kervansaray/Divan ...... ⬜
A5 Olay & Pusu → Savaş ................. ⬜
A6 Meta: İstihbarat/İtibar/Dükkân ...... ⬜
A7 Cila (Dil, Performans, Temizlik) .... ⬜
A8 Android Sürüm & Test ................ ⬜
```

### A2 Adımları
- [x] A2-UI Harita arayüzü, sidebar, bilgi kartı, SFX (bkz. 07)
- [x] A2.0 Hijyen — Input System fix, DevOverlay koruması, ses kalıcılığı
- [x] A2.1 Kimlik birliği (NodeId + MapNodeView)
- [x] A2.2 Boot sahnesi + GameSession
- [x] A2.3 Kayıt / Yükleme (JSON)
- [x] A2.4 Rota grafiği + gerçek mesafe/erzak
- [x] A2.5 TopBar HUD (Akçe, Erzak, Tarih, Mevsim)
- [ ] **A2.6 Sahne geçiş servisi** ← **SIRADAKİ**
- [ ] A2.7 Sidebar gerçek veri + boş durumlar
- [ ] A2.8 Cihaz testi & aşama kapanışı

---

## 3. ▶️ Sıradaki Adım: A2.6 — Sahne Geçiş Servisi (SceneLoader + ScreenFader)
- **Hedef:** Tüm sahne geçişlerinin tek merkezden (`SceneLoader`), ekran kararma/açılma (`ScreenFader`), çift tıklama koruması ve asenkron yükleme ile yapılması.
- **Çözer:** TD-07 (tutarsız sahne geçişleri), TD-14 (string sahne adları dağınıklığı)
- **Detay & test senaryosu:** `05-Yol-Haritasi.md` → `### A2.6`

---

## 4. ⛔ Açık Kararlar
- Bekleyen açık karar yok (`KR-1..KR-5` onaylandı ve `01-Oyun-Tasarimi.md` §12'ye işlendi).

---

## 5. Kalıcı Notlar
- **Tasarım yönetimi ilkesi:** Tasarım konularında değişikliklere doğrudan gidilmez; tasarım işlemleri tamamen proje sahibinin kontrolünde ilerler. Danışman olarak olması gerekenler ve profesyonellik önerileri sunulur; ayrıca hangi yapay zekâ araçlarının veya programların (ve iş akışlarının) kolaylık sağlayacağı belirtilir.
- **Hatırlatma (Arayüz Temizliği):** İlerleyen süreçte son aşamalara doğru ufak tefek arayüz temizlikleri (`kısicon.png` ASCII adı, mükerrer `Toolbar` klasörü, ham `Toolbar.ai` dosyası) yapılacaktır.
- **Esneklik ilkesi:** Tamamlanmış sahnelerde de her zaman revizyon yapılabilir; "bu aşama bitti, dönülmez" denmez. Revizyon yeni bir adım olarak eklenir (ör. `A1.R1`).
- **Mobil test:** Her UI adımında Device Simulator ile 16:9, 19.5:9, 20:9 kontrolü.
- **Açık kritik sorun sayısı:** 🔴 1 · 🟠 3 (bkz. `06`)
