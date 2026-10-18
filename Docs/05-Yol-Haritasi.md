# 🗺️ Yol Haritası — Aşamalar ve Adımlar

> **Kullanım:** AI bu dosyanın tamamını okumaz; yalnızca üzerinde çalışılan adımın bölümünü okur (ör. `### A2.1`).
> **Adım kuralı:** 1 adım = 1 oturum = en fazla ~5 dosya = tek commit. Büyürse `.a/.b` diye bölünür.
> **Durum simgeleri:** ⬜ Bekliyor · ⏳ Devam ediyor · ✅ Tamam · ⛔ Karar bekliyor (bkz. `03` Açık Kararlar)
> **Sıra mantığı:** *Önce omurga, sonra yeni ekran.* Kalıcı oyun durumu (GameSession + Save) kurulmadan yeni sahne yazılmaz; aksi halde her sahne sahte veriyle yazılıp sonra sökülür.

---

## Genel Bakış

```text
A0 Hazırlık & Veri Omurgası (prototip) ............ ✅
A1 Ana Menü & UI Temelleri ......................... ✅
A2 Dünya Haritası & Oyun Omurgası .................. ⏳  ← ŞU AN
   A2.0 Hijyen (Input, DevOverlay, Ses kalıcılığı)  ✅
   A2.1 Kimlik birliği (NodeId + MapNodeView)        ✅
   A2.2 Boot sahnesi + GameSession                   ✅
   A2.3 Kayıt / Yükleme (JSON)
   A2.4 Rota grafiği + gerçek mesafe/erzak
   A2.5 TopBar HUD (Akçe/Erzak/Tarih)
   A2.6 Sahne geçiş servisi (SceneLoader + Fade)
   A2.7 Sidebar gerçek veri + boş durumlar
   A2.8 Cihaz testi (Device Simulator) & Aşama kapanışı
A3 Seyahat (TravelScene yeniden) ................... ⬜
A4 Şehir: Pazar + Kervansaray + Divan .............. ⬜
A5 Olay & Pusu → Savaş (Combat) .................... ⬜
A6 Meta: İstihbarat, İtibar, Dükkân ................ ⬜
A7 Cila: Yerelleştirme, Performans, Android Back ... ⬜
A8 Android Sürüm (Release) & Test .................. ⬜
```

> [!NOTE]
> Eski yol haritasında Kayıt/Yükleme 6. aşamadaydı. Analiz sonucu **A2.3'e çekildi**: tüm sahneler aynı kalıcı veriyi kullanmalı, mobilde uygulama her an kapatılabilir.

---

## A2 — Dünya Haritası & Oyun Omurgası

### A2.0 — Hijyen ✅
- **Amaç:** Telefonda zoom'un çalışması, release'te test menüsü çıkmaması, ses ayarının kalıcı olması.
- **Çözer:** BUG-01, BUG-06, BUG-07
- **Muhtemel dosyalar:** `UI/MapPanZoomController.cs` (EnhancedTouch'a geçiş), `Core/KervanSceneNavOverlay.cs` (`#if UNITY_EDITOR || DEVELOPMENT_BUILD`), `Audio/AudioManager.cs` (ses seviyelerini PlayerPrefs'e yaz/oku, tek `SetMuted` API), `UI/MainMenuUI.cs` + `TradeAndTravel/WorldMapUI.cs` (mute'u AudioManager'a devret).
- **Unity'de elle:** Yok (sadece test).
- **Test:** (1) WorldMap Play → Console temiz. (2) Device Simulator'da iki parmak zoom. (3) Menüde sesi kapat → haritaya geç → kapalı kalıyor → oyunu kapat/aç → hâlâ kapalı. (4) Development Build kapalı bir build'de test menüsü görünmüyor.

### A2.1 — Kimlik Birliği (NodeId + MapNodeView) ✅
- **Amaç:** Haritadaki her düğüm kendi `LocationNodeSO`'sunu Inspector referansıyla bilsin; isimle arama tamamen kalksın.
- **Çözer:** BUG-02, BUG-03, TD-06 (kısmen), TD-05 (`WorldMapUI` ve `BottomNodeInfoCardUI` `Resources.Load` temizliği)
- **Karar (KR-1 & KR-2):** Sahnedeki 13 şehir korundu; `LocationNodeSO.GetFactionDisplayName()` ile Türkçe fraksiyon adları bağlandı (`GameEnums.FactionType` enum sadeleştirmesi `ReputationManager`/`TownHubUI` ile birlikte `A6.1`'de tamamlanacak).
- **Muhtemel dosyalar:** Yeni `TradeAndTravel/MapNodeView.cs` (Button + `LocationNodeSO` referansı + `NodeId`), `TradeAndTravel/LocationNodeSO.cs`, `WorldMapUI.cs` (`Node_` isim taraması ve `FindNodeRectByName` kaldırıldı), `BottomNodeInfoCardUI.cs` (SO'dan beslenir), `MainMenuUI.cs` (`CurrentNodeId` → `node_sogut`).
- **Unity'de elle:** Eksik `LocationNodeSO`'lar oluşturuldu; her harita düğümüne `MapNodeView` eklenip SO sürüklenerek bağlandı.
- **Test:** Yeni oyun → Söğüt'e tıkla → "Şehre Gir" çıkar. Her şehrin kartında kendi açıklaması/fraksiyonu görünür. Console'da "bulunamadı" uyarısı yok.

### A2.2 — Boot Sahnesi + GameSession ✅
- **Amaç:** Sahneler arası tek, kalıcı oyun durumu (kervan, konum, gün, aktif seyahat). Singleton dağınıklığını azaltmak.
- **Çözer:** BUG-05 (temel), TD-08 (kısmen), TD-09, TD-02 (kısmen)
- **Muhtemel dosyalar:** Yeni `Core/Bootstrapper.cs` (servisleri kurar), `Core/Services.cs` (hafif servis bulucu), `Core/GameSession.cs` (saf C# durum), `TradeAndTravel/CaravanManager.cs` (`GameSession.Caravan`'a devreder), `WorldMapUI.cs`.
- **Unity'de elle:** `00_Boot` sahnesi oluşturup Build Settings'te en üste kondu; `AudioManager.prefab` Boot sahnesine eklendi.
- **Test:** Herhangi bir sahneden Play'e basınca da servisler kuruluyor (editör kolaylığı). Menü → Harita → Menü → Harita: tek AudioManager, tek GameSession.

### A2.3 — Kayıt / Yükleme (JSON) ⬜
- **Amaç:** "Devam Et" gerçek kaydı yüklesin; uygulama arka plana atılınca otomatik kayıt.
- **Çözer:** TD-01, TD-12
- **Muhtemel dosyalar:** Yeni `Core/Save/SaveData.cs` (`saveVersion` alanlı), `Core/Save/SaveService.cs` (`persistentDataPath`, atomik yazma: `.tmp` → rename), `MainMenuUI.cs` (kayıt kartı + Yeni Oyun onayı), `GameSession.cs`.
- **Unity'de elle:** Yeni Oyun onayı için `Popup_QuitConfirm` benzeri bir popup kopyası (veya mevcut popup'ın metin parametreli hali).
- **Test:** Oyna (akçe değişsin) → uygulamayı kapat → Devam Et → aynı akçe/konum. Kayıt varken Yeni Oyun → onay sorar. Bozuk kayıt dosyası → çökme yok, uyarı + yeni oyun.

### A2.4 — Rota Grafiği + Gerçek Mesafe/Erzak ⬜
- **Amaç:** Hangi şehirden hangisine gidilebileceği `TravelRouteSO`'lardan gelsin; mesafe/erzak/tehlike gerçek veri olsun.
- **Çözer:** TD-04, TD-02 (mesafe), BUG-04 (konum, varışta güncellenir)
- **Muhtemel dosyalar:** Yeni `TradeAndTravel/RouteGraph.cs` (saf C#, komşuluk + en kısa yol), `WorldMapUI.cs` (`CalculateEstimatedDays` kaldırılır), `BottomNodeInfoCardUI.cs` ("Yol yok" durumu), opsiyonel `UI/RouteLineView.cs` (rota çizgisi).
- **Unity'de elle:** Eksik `TravelRouteSO`'ları oluşturmak; `RouteGraph` için tüm rotaları içeren bir `RouteDatabaseSO`.
- **Test:** Komşu olmayan şehre tıklayınca "doğrudan yol yok / X üzerinden N gün". Seyahat başlatınca `CurrentCity` **değişmez**, sadece `ActiveTravel` kaydedilir.

### A2.5 — TopBar HUD ⬜
- **Amaç:** Akçe, Erzak, Tarih göstergeleri yeni görsellerle ve gerçek veriyle.
- **Çözer:** TD-02 (tarih)
- **Varlıklar:** `UI_ToolbarBG.png`, `toolbarAkceIcon.png`, `toolbarErzakIcon.png` (önce `varlik-entegrasyonu` ile isim standardı).
- **Muhtemel dosyalar:** Yeni `UI/TopBarHUD.cs` (EventBus dinler), `Core/GameCalendar.cs` (gün sayısı → "14 Mayıs 1326" biçimi), `WorldMapUI.cs` (HUD kodu çıkarılır).
- **Unity'de elle:** TopBar prefab'ı (`Prefabs/UI/TopBarHUD.prefab`).
- **Test:** Akçe değişince sayı animasyonla güncellenir; tarih GameSession gününden hesaplanır.

### A2.6 — Sahne Geçiş Servisi ⬜
- **Amaç:** Tüm sahne geçişleri tek noktadan, fade + async yükleme ile.
- **Çözer:** TD-07, TD-14
- **Muhtemel dosyalar:** Yeni `Core/SceneLoader.cs`, `Core/SceneIds.cs` (sahne adı sabitleri), `UI/ScreenFader.cs` (Boot'ta kalıcı), `MainMenuUI.cs` (fallback timeout kaldırılır), `WorldMapUI.cs`.
- **Test:** Menü ↔ Harita ↔ (eski) sahneler arası her geçişte fade-out/fade-in; çift tıklamada iki kez yükleme yok.

### A2.7 — Sidebar Gerçek Veri + Boş Durumlar ⬜
- **Amaç:** Kervan/Yük/Görevler sekmeleri GameSession'dan beslensin, sahte satırlar kalksın.
- **Çözer:** TD-03, TD-13 (başlangıç)
- **Muhtemel dosyalar:** `UI/InventorySidebarUI.cs` (sadece `RefreshData` ve satır üretimi — dosyanın tamamı okunmaz), opsiyonel yeni `UI/Sidebar/SidebarRowView.cs`.
- **Test:** Yeni oyunda yük sekmesi "Henüz yük yok" der. Başlangıç kadrosu `GameSession`'daki gerçek kadrodur.

### A2.8 — Cihaz Testi & Aşama Kapanışı ⬜
- **Amaç:** 16:9, 19.5:9, 20:9 ve çentikli cihazlarda MainMenu + WorldMap kontrolü; A2 kapanış raporu.
- **Kontrol listesi:** Safe area, min 48dp dokunma hedefi, Android Back (en azından popup kapatma), FPS (Profiler), Console temiz.
- **Çıktı:** `03`'te A2 ✅, `07`'de aşama özeti, Git tag `v0.2.0-alpha`.

---

## A3 — Seyahat (TravelScene yeniden) ⬜
> **Karar (KR-3):** Plan mantığına uygun temiz kodlar korunacak, diğerleri baştan yazılacak; tüm arayüz/tasarım sıfırdan yapılacak. Başlarken `asama-planla` ile adımlara bölünür. Taslak:
- A3.1 Seyahat tasarım önerisi (`arayuz-tasarimi`): GDD §6 yerleşimi — üstte kaynaklar, ortada parallax kervan, altta ilerleme + "Mola Ver".
- A3.2 `TravelSystem` (saf C#): gün gün ilerleme, erzak tüketimi, `ActiveTravel` GameSession'da. `TravelManager`'ın ayrı `CaravanData`'sı kaldırılır (BUG-05).
- A3.3 Kervan yürüyüş görseli (2D Animation / PSD Importer) + parallax arka plan.
- A3.4 Varış: konum güncelleme, otomatik kayıt, haritaya dönüş.
- A3.5 Yol olayları için `RoadEventSO` altyapısı (A5'e zemin).

## A4 — Şehir: Pazar + Kervansaray + Divan ⬜
- A4.1 Şehir hub tasarımı (sekme: Pazar / Kervansaray / Divan).
- A4.2 Pazar: `CityMarket` mantığının GameSession'a bağlanması, alış-satış UI, kâr/zarar göstergesi.
- A4.3 Pazar doygunluğu ve gün geçtikçe toparlanma (GDD §4).
- A4.4 Kervansaray: dinlenme, iyileşme, erzak ikmali, muhafız kiralama.

## A5 — Olay & Pusu → Savaş ⬜
- A5.1 Olay penceresi: **Kaç / Haraç Ver / Savaş** (GDD §11/5).
- A5.2 Savaş seçimi: Manuel / Otomatik (otomatik çözümleme formülü).
- A5.3 CombatScene yeniden (mevcut `DamageCalculator`, `HexPathfinder` korunur), GDD Unit HUD + Action Cluster yerleşimi.
- A5.4 Sonuç → kayıp/yaralanma → kervana yansıma → seyahate dönüş.

## A6 — Meta: İstihbarat, İtibar, Dükkân ⬜
- A6.1 `ReputationManager`/`QuestManager`/`ShopManager` → GameSession servislerine taşınması.
- A6.2 Divan: istihbarat teslimi, ödüller. A6.3 Sabit dükkân / pasif gelir.

## A7 — Cila ⬜
- Unity Localization (TR/EN), UI tema token'ları (`UIThemeSO`), Sprite Atlas + ASTC, Android Back tuşu tam akış, haptic, erişilebilirlik (metin boyutu), varlık temizliği (AST-xx kayıtları).

## A8 — Android Sürüm ⬜
- `applicationIdentifier`, keystore, IL2CPP ARM64, AAB, Build Profiles (Dev/Release), log temizliği (TD-15), sürüm tek kaynak (TD-16), crash raporlama, Google Play Internal Testing.
