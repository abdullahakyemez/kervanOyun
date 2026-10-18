# 📜 Değişiklik Günlüğü (Changelog)

> En yeni kayıt **en üstte**. Her kapanan adım için `gorev-kapat` tarafından eklenir.
> Format: `## [Tarih] Adım-ID — Başlık` + kısa maddeler + commit mesajı.
> AI bu dosyayı yalnızca geçmiş sorulduğunda veya kayıt eklerken açar.

---

## [18.10.2026] A2.2 — Boot Sahnesi + GameSession
- `Services.cs` (yeni): Yeni Singleton açmadan kalıcı servisleri (`GameSession`, `AudioManager` vb.) kaydetmek ve sorgulamak için hafif servis bulucu eklendi (`TD-08`).
- `GameSession.cs` (yeni): Sahneler arası tek doğruluk kaynağı olan çalışma anı oyun durumu (`CurrentNodeId`, `DayCount`, `Caravan`, `ActiveTravel`, `VisitedNodeIds`) ve `EventBus` durum olayları (`LocationChangedEvent`, `DayPassedEvent`, `TravelStartedEvent`) eklendi (`BUG-05` temeli, `TD-09`, `TD-02` kısmen).
- `Bootstrapper.cs` (yeni): `SubsystemRegistration` ve `BeforeSceneLoad` nitelikleriyle Editör'de herhangi bir sahneden Play'e basıldığında `EventBus.ClearAll()`, `Services.Clear()` ve `GameSession` kurulumunu garanti eden; `00_Boot` sahnesinden `MainMenuScene`'e geçiş yapan başlatıcı bileşen eklendi (`TD-09`).
- `CaravanManager.cs`, `WorldMapUI.cs`: `CaravanManager.Caravan` doğrudan `GameSession.Caravan`'a yönlendirildi; `WorldMapUI` konum ve kaynak HUD işlemlerinde `GameSession` kullanacak şekilde güncellendi.
- Unity'de elle yapılanlar: `Assets/_Project/Scenes/00_Boot.unity` sahnesi oluşturuldu, içine `Bootstrapper` ve `AudioManager.prefab` eklendi, Build Profiles listesinde en üste (`index 0`) alındı.
- Commit: `feat(core): A2.2 boot sahnesi, Services ve GameSession altyapısı (TD-08, TD-09, BUG-05)`

---

## [16.10.2026] A2.1 — Kimlik Birliği (NodeId + MapNodeView)
- `MapNodeView.cs` (yeni): Harita düğümü butonlarını Inspector referansı ile `LocationNodeSO` varlığına bağlayan ve ASCII `NodeId` sunan görünüm bileşeni eklendi.
- `LocationNodeSO.cs`: Pazar özeti için `_marketGoodsSummary` alanı ve Türkçe fraksiyon adı döndüren `GetFactionDisplayName()` metodu eklendi.
- `WorldMapUI.cs`, `BottomNodeInfoCardUI.cs`, `MainMenuUI.cs`: İsim tabanlı düğüm arama (`name.StartsWith("Node_")`, `FindNodeRectByName`, `ToLower().Contains`) ve `Resources.Load` yedekleri kaldırıldı; mevcut konum ve düğüm seçimi tamamen `MapNodeView` + ASCII `NodeId` (`"node_sogut"`) üzerine taşındı (`BUG-02`, `BUG-03`, `TD-06`, `TD-05`). Eylem butonundaki font dışı Unicode ok karakteri (`\u2794`) temizlendi.
- Unity'de elle yapılanlar: Sahnedeki 13 düğüm için eksik 9 `LocationNodeSO` oluşturuldu ve 13 `Node_*` objesine `MapNodeView` eklenerek SO referansları bağlandı.
- Commit: `feat(map): A2.1 kimlik birliği - MapNodeView ve ASCII NodeId mimarisi (BUG-02, BUG-03, TD-06)`

---

## [16.10.2026] A2.0 — Hijyen (Input System, DevOverlay Koruması ve Ses Kalıcılığı)
- `MapPanZoomController.cs`: Eski `UnityEngine.Input` çağrıları kaldırıldı; `EnhancedTouchSupport` ve `Touch.activeTouches` ile yeni Input System'e geçirildi (`BUG-01`).
- `KervanSceneNavOverlay.cs`: Geliştirici sahne geçiş menüsü `#if UNITY_EDITOR || DEVELOPMENT_BUILD` koruması içine alındı (`BUG-06`).
- `AudioManager.cs`, `MainMenuUI.cs`, `WorldMapUI.cs`: Ses düzeyleri ve `IsMuted` durumu `PlayerPrefs` ile kalıcı hale getirildi; `ToggleMute()` merkezi API'si eklendi; `WorldMapUI` içindeki `AudioListener.pause`/`FindObjectsByType<AudioSource>` hack'leri ve `Resources.Load` yedekleri kaldırıldı (`BUG-07`, `TD-05`).
- Mimari/Tasarım kararları (`KR-1..KR-5`) onaylandı ve `01-Oyun-Tasarimi.md` §12 ile `03`/`05` dokümanlarına işlendi.
- Unity'de elle yapılanlar: Yok (mevcut sahnelerde test edildi).
- Commit: `fix(core,ui,audio): A2.0 hijyen - EnhancedTouch pinch-zoom, DevOverlay koruması ve ses kalıcılığı (BUG-01, BUG-06, BUG-07)`

---

## [14.10.2026] DOC — AI çalışma düzeni ve dokümantasyon yeniden yapılandırıldı
- Salt okunur proje analizi yapıldı (rapor: `Docs/Arsiv/2026-10-14-Analiz-Raporu.md`).
- `AGENTS.md` sadeleştirildi; doküman yönlendirme tablosu, token ekonomisi, Bitti Tanımı eklendi.
- Yeni dokümanlar: `04-Mimari-ve-Kod-Standartlari.md`, `05-Yol-Haritasi.md`, `06-Bilinen-Sorunlar.md`, `07-Degisiklik-Gunlugu.md`, `AI-Kullanim-Rehberi.md`.
- `03-Proje-Durumu.md` her oturum okunacak kısa panoya dönüştürüldü; geçmiş detaylar bu dosyaya taşındı.
- Yeni skill'ler: `asama-planla`, `arayuz-tasarimi`, `varlik-entegrasyonu`, `hata-ayikla`, `kod-inceleme`. Mevcut 3 skill güncellendi.
- **Projede (Assets/ProjectSettings) hiçbir dosya değiştirilmedi.**
- Commit: `docs(ai): AI çalışma düzeni, yol haritası ve bilinen sorunlar kaydı`

---

## [14.10.2026] A2-UI — Dünya Haritası arayüzü, SFX ve arayüz cilaları
- **Varlık organizasyonu:** Ticaret emtiaları `Art/Sprites/Items/`; profil kartları (`Mauhafiz_ProfileCard`, `Okcu_ProfileCard`, `Ahi_ProfileCard`, `Horse_ProfileCard`, `Mule_ProfileCard`, `Camel_ProfileCard`, `Erzak_ProfileCard`) ve UI parçaları `Art/Sprites/UI/Portraits|Backgrounds|Buttons|Decorations/` altında toplandı.
- **Harita Pan & Zoom:** `MapPanZoomController.cs` (pinch zoom, drag pan, sınır kilitleme, Söğüt'e odaklanma).
- **Kervan & Yük yan çekmecesi:** `InventorySidebarUI.cs` — safe area (`ApplySafeArea()`), responsive satır motoru, erzak bilgisi TopBar'a bırakıldı, Muhafız/Okçu/Alp portreleri bağlandı.
- **Harita arayüzü:** `WorldMapUI.cs`, `BottomNodeInfoCardUI.cs`, `CurrentNodeMarkerUI.cs`:
  - Sol alt Kervan & Yük butonu; sağ üst Utility Drawer (Ayarlar, Ses aç/kapa, Ana Menü).
  - `BGM_Travel.mp3` çakışması ve mute kontrolü (`AudioListener.volume/pause`), kameradaki çift AudioSource durduruldu.
  - Aktif düğüm göstergesi: altın nabız halkası + süzülen konum pini (`Icon_Location`/`Konumicon`), `raycastTarget = false`.
  - Sidebar renkleri: parşömen kremi `#F5E6D3`, antik altın `#D9B359`; ayraç `UI_BG_ayirac_2`.
  - Alttan kayan şehir bilgi kartı: Konum/Tehlike/Pazar/Erzak rozetleri, TMP Truncate, `GetMarketGoodsSummary`.
  - MainMenu → WorldMap geçişine zaman aşımı emniyeti (fallback timeout).
  - Ham `.ai`/`.jpg` dosyaları `Art/SourceFiles/` altına arşivlendi; mükerrer `Unit_Muhafız.png` temizlendi.
- **SFX:** `SFX_Parchment.wav` kaldırıldı; bilgi kartı ve çekmeceye `SFX_PaperRustle.wav`; `UIButtonSound.cs` özel klip/ses/perde varyasyonu; `AudioManager` kendini onaran yapı.
- Commit: `6105b35 feat(ui, audio): dunya haritasi arayuzu, sidebar, sfx ve ses sistemi gelistirmeleri`

## [~13.10.2026] A1 — Ana Menü & UI temelleri (tamamlandı + cilalandı)
- `UI_BG_MainMenu` + `UI_Logo_GameTitle`, Yeni Oyun / Devam Et butonları.
- Alt orta login seçenekleri (Google, Apple, Mail) — henüz işlevsiz (log basıyor).
- Sağ üst `UtilityDrawer` (kapalı başlangıç, okla açılma).
- `Popup_Settings` (`UI_Frame_Market` zemini, `SFX_PaperRustle` açılış, ses slider'ları, `Icon_Util_Close`).
- `BGM_Main.mp3` ve `SFX_ButtonClick.wav` AudioManager'a bağlandı.
- Cilalar: `ScreenFader.cs` (fade), `QuitConfirmPopupUI.cs` (çıkış onayı), `SafeAreaFitter.cs`, sürüm etiketi `v0.1.0 Alpha`, `PlayerPrefs` tabanlı "Devam Et" kontrolü, `UI_Btn_Standard.png`.
- Görsel Varlık Standardı `02-Teknik-ve-Gorsel.md` §7'ye eklendi.
- Commit: `6a5270d feat(ui): Asama 1 tamamlandi - Ana Menu, Cekmece, Ayarlar Popupu ve Varlik Organizasyonu`

## [≤07.10.2026] A0 — Hazırlık, mimari ve prototip sahneler
- ScriptableObject veri yapıları (Item, Unit, Node, Route, Market, Intel, Audio).
- Savaş matematiği (`DamageCalculator`, `HexGrid`, `HexPathfinder`, `CombatTurnManager`, `BanditAI`).
- Pazar ve envanter omurgası (`CityMarket`, `CaravanData`), istihbarat/itibar/dükkân (`QuestManager`, `ReputationManager`, `ShopManager`).
- Prototip sahneler: `TravelScene`, `MarketScene`, `TownHubScene`, `CombatScene` (**eski/prototip** kabul edilir — bkz. `03` Açık Kararlar KR-3).
- Önceki commit'ler: `e88c0c5`, `fcf8ce9`, `350ee3f`, `0f1bb47`, `d8bc3c4`, `63f203b`, `b760f83`, `0d55886`.
