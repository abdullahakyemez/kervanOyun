# 📌 Proje Durumu (Pano)

> **Her oturumun başında okunan tek dosya.** Kısa tutulur (~80 satır). Geçmiş detaylar → `07-Degisiklik-Gunlugu.md`, adım detayları → `05-Yol-Haritasi.md`, sorunlar → `06-Bilinen-Sorunlar.md`.

**Proje:** Uç Beyliği Kervanı · 14. yy Anadolu · Taktiksel RPG & Ticaret
**Motor:** Unity 6 (6000.6.3f1), URP 2D · **Platform:** Android, yatay (landscape)
**Sürüm etiketi:** v0.1.0 Alpha · **Son güncelleme:** 18.10.2026

---

## 1. Gerçek Durum (Tek Bakışta)

| Alan | Durum | Not |
|---|---|---|
| Görsel / UI | 🟢 | MainMenu ve WorldMap arayüzleri hazır ve cilalı. |
| Veri tanımları (SO) | 🟢 | 13 harita düğümü `MapNodeView` + `LocationNodeSO` (ASCII `NodeId`) ile bağlı (A2.1). |
| Oyun döngüsü | 🟡 | `00_Boot` + `GameSession` + `Services` kuruldu (A2.2); rota/seyahat bağlantısı sırada. |
| Kalıcı durum / Kayıt | 🟢 | `GameSession` + `SaveService` (`save_0.json` atomik JSON kayıt/yükleme ve otomatik kayıt) tamamlandı (A2.3). |
| Mobil teknik | 🟢 | Input System (`EnhancedTouch`), DevOverlay koruması ve ses kalıcılığı tamam (A2.0). |

**Özet:** Görsel, düğüm kimlik yapısı, çalışma anı oturumu (`00_Boot` + `GameSession` + `Services`) ve atomik JSON kayıt/yükleme (`SaveService`) hazır; sırada rota grafiği (`A2.4`) ve TopBar HUD (`A2.5`) var.

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
- [ ] **A2.4 Rota grafiği + gerçek mesafe/erzak** ← **SIRADAKİ**
- [ ] A2.5 TopBar HUD
- [ ] A2.6 Sahne geçiş servisi
- [ ] A2.7 Sidebar gerçek veri + boş durumlar
- [ ] A2.8 Cihaz testi & aşama kapanışı

---

## 3. ▶️ Sıradaki Adım: A2.4 — Rota Grafiği + Gerçek Mesafe/Erzak
- **Hedef:** Şehirler arası komşuluk ve mesafe/erzak/tehlike verilerinin `TravelRouteSO` + `RouteGraph` üzerinden gelmesi; seyahat başlarken konumun hemen değil varışta güncellenmesi.
- **Çözer:** TD-04, TD-02 (mesafe), BUG-04
- **Detay & test senaryosu:** `05-Yol-Haritasi.md` → `### A2.4`

---

## 4. ⛔ Açık Kararlar
- Bekleyen açık karar yok (`KR-1..KR-5` onaylandı ve `01-Oyun-Tasarimi.md` §12'ye işlendi).

---

## 5. Kalıcı Notlar
- **Esneklik ilkesi:** Tamamlanmış sahnelerde de her zaman revizyon yapılabilir; "bu aşama bitti, dönülmez" denmez. Revizyon yeni bir adım olarak eklenir (ör. `A1.R1`).
- **Mobil test:** Her UI adımında Device Simulator ile 16:9, 19.5:9, 20:9 kontrolü.
- **Açık kritik sorun sayısı:** 🔴 2 · 🟠 4 (bkz. `06`)
