# 📜 Değişiklik Günlüğü (Changelog)

> En yeni kayıt **en üstte**. Her kapanan adım için `gorev-kapat` tarafından eklenir.
> Format: `## [Tarih] Adım-ID — Başlık` + kısa maddeler + commit mesajı.
> AI bu dosyayı yalnızca geçmiş sorulduğunda veya kayıt eklerken açar.

---

## [25.10.2026] A2.6 — Sahne Geçiş Servisi (SceneLoader + ScreenFader)
- `SceneIds.cs` (yeni): Projedeki tüm Unity sahne adlarını tek merkezde toplayan sabit sınıfı eklendi (`TD-14`).
- `SceneLoader.cs` (yeni): `Services` üzerinden erişilen, çift dokunma ve eşzamanlı geçiş isteklerini engelleyen (`IsLoading`), `ScreenFader` ile kararma/açılma entegreli asenkron sahne yükleyici servisi eklendi (`TD-07`).
- `ScreenFader.cs`: Kalıcı singleton/servis yapısına (`DontDestroyOnLoad`) geçirildi; `sortingOrder = 32767` ve tam ekran UI Canvas'ı ile tüm sahnelerin üzerinde çalışması sağlandı, dinamik `EnsureInstance()` ve `FadeRoutine` coroutine desteği eklendi (`TD-07`).
- `Bootstrapper.cs`: `ScreenFader` ve `SceneLoader` servis kayıtları eklendi; `00_Boot`'tan menüye geçiş `SceneLoader.Load` ile asenkron ve yumuşak hale getirildi (`TD-07`).
- `MainMenuUI.cs`: Kırılgan `FallbackSceneLoadRoutine` zaman aşımı kaldırıldı; oyun sahnesine geçiş `SceneLoader.Load(SceneIds.WorldMap)` ile bağlandı (`TD-07`).
- `WorldMapUI.cs`: Şehre giriş, seyahat başlatma ve Ana Menü'ye dönüş geçişleri `SceneLoader.Load` ve `SceneIds` sabitlerine bağlandı (`TD-07`, `TD-14`).
- `KervanSceneNavOverlay.cs`: Geliştirici geçişleri `SceneLoader.Load` ve `SceneIds` ile güncellendi.
- Commit: `feat(core): A2.6 sahne geçiş servisi - SceneLoader, kalıcı ScreenFader ve SceneIds sabitleri (TD-07, TD-14)`

---

## [25.10.2026] A2.5 — TopBar HUD (Akçe, Erzak, Tarih, Mevsim)
- `GameCalendar.cs` (yeni): 14 Mayıs 1326 başlangıç tarihinden gün hesaplayan, `SeasonType` ve Türkçe mevsim adları ("Bahar", "Yaz", "Güz", "Kış") üreten saf C# takvim sınıfı eklendi (`TD-02`).
- `TopBarHUD.cs` (yeni): Akçe (sayaç akışı ve kapsül nabız animasyonu), Erzak (<= 3 gün için kırmızı uyarı rengi), Tarih ve Mevsim göstergelerini `GameSession` ve `EventBus` (`CaravanStatsChangedEvent`, `DayPassedEvent`) ile senkronize eden bağımsız HUD kontrolcüsü eklendi (`TD-02`).
- `WorldMapUI.cs`: HUD metin alanları (`_akceText`, `_erzakText`, `_dateText`) ve sabit kodlu `"14 Mayıs 1326"` tarihi kaldırıldı; `TopBarHUD` referansına devredildi (`TD-02`).
- Unity'de elle yapılanlar: `TopBar_HUD` içine `SeasonCapsule` (`SeasonBG`, `SeasonImg`, `SeasonText`) eklendi, yeni yüksek çözünürlüklü ikonlar (`Assets/_Project/Art/UI/Toolbar/`) projeye alındı, `TopBarHUD` bileşeni sahneye bağlanıp referansları tamamlandı.
- Commit: `feat(ui): A2.5 TopBar HUD - GameCalendar takvim/mevsim motoru, sayaç animasyonu ve WorldMapUI ayrımı (TD-02)`

---

## [19.10.2026] A2.4 — Rota Grafiği + Gerçek Mesafe/Erzak
- `RouteDatabaseSO.cs` (yeni): Haritadaki tüm `TravelRouteSO` varlıklarını tek bir listede toplayan ScriptableObject veri tabanı eklendi (`TD-04`).
- `RouteGraph.cs` (yeni): `RouteDatabaseSO` üzerinden çift yönlü (undirected) komşuluk grafiği kuran, doğrudan komşuluk (`TryGetDirectRoute`) ve Dijkstra ile en kısa aktarmalı yol (`TryFindPath` / `RoutePathResult`) hesaplayan saf C# sınıfı eklendi (`TD-04`).
- `GameSession.cs`: `StartTravel` içindeki geçici `SetCurrentNode(toNodeId)` çağrısı kaldırıldı; seyahat başlarken oyuncu konumu (`CurrentNodeId`) korunup yalnızca `ActiveTravel` kaydediliyor (`BUG-04`).
- `WorldMapUI.cs`, `BottomNodeInfoCardUI.cs`: Sabit kodlu `CalculateEstimatedDays` ve `%15` tehlike metni kaldırıldı (`TD-02`); bilgi kartı doğrudan komşuda gerçek gün/erzak/tehlike değerlerini, komşu olmayan düğümlerde *"Doğrudan yol yok / X üzerinden Toplam N Gün"* bilgisini gösterecek ve eylem butonunu pasifleştirecek şekilde güncellendi.
- `MapPanZoomController.cs`: `EnsureMapBackgroundFit` metodunun Play modunda `MapBackground` boyutunu genişletip kenardaki düğümleri kaydırması düzeltildi; `MapContent` çerçevesi `MapBackground`'un Editör boyutuna eşitlendi.
- Unity'de elle yapılanlar: 13 düğümü birbirine bağlayan `TravelRouteSO` varlıkları ve `RouteDatabase.asset` oluşturularak `WorldMapUI` bileşenine bağlandı; harita düğüm konumları düzenlendi.
- Commit: `feat(map): A2.4 rota grafiği, gerçek mesafe/erzak hesabı ve harita hizalaması (TD-04, TD-02, BUG-04)`

---

## [18.10.2026] A2.3 — Kayıt / Yükleme (JSON)
- `SaveData.cs` (yeni): `saveVersion = 1` alanlı, SO referanslarını yalnızca ASCII ID olarak tutan serileştirilebilir kayıt veri modelleri (`SaveData`, `CaravanSaveData`, `InventorySlotSaveData`, `GuardSaveData`, `ActiveTravelSaveData`) eklendi (`TD-01`).
- `SaveService.cs` (yeni): `Application.persistentDataPath/save_0.json` yoluna atomik yazma (`save_0.tmp` → `.bak` → `save_0.json`), bozuk JSON/sürüm doğrulama (`TryLoad`) ve `GameSavedEvent` yayını yapan servis eklendi (`TD-01`).
- `GameSession.cs`, `CaravanData.cs`: `ToSaveData()`, `ApplySaveData()`, `IsSessionActive` ve `CaravanData.RestoreFromSave()` eklendi; eski `PlayerPrefs` konum/kayıt bayrakları temizlendi (`TD-01`, `TD-10`).
- `Bootstrapper.cs`: `SaveService` kaydı, `DontDestroyOnLoad` kalıcılığı ve konum/gün/kaynak değişim olayları ile `OnApplicationPause(true)` / `OnApplicationQuit()` anlarında otomatik kayıt tetiklemesi eklendi.
- `MainMenuUI.cs`, `QuitConfirmPopupUI.cs`: "Devam Et" butonu `SaveService.HasSaveFile()` ve `LoadIntoSession()` ile bağlandı (bozuk dosyada çökmeden uyarı + yeni oyun); mevcut kayıt varken "Yeni Oyun"a basıldığında onay penceresi (`ShowCustomConfirm`) açılması sağlandı (`TD-12`).
- Unity'de elle yapılanlar: `MainMenuScene` içinde `Popup_NewGameConfirm` onay penceresi oluşturuldu ve `MainMenuUI` bileşenine bağlandı.
- Commit: `feat(save): A2.3 atomik JSON kayıt/yükleme servisi ve yeni oyun onay akışı (TD-01, TD-12)`

---

## [18.10.2026] A2.2 — Boot Sahnesi + GameSession
- `Services.cs` (yeni): Yeni Singleton açmadan kalıcı servisleri (`GameSession`, `AudioManager` vb.) kaydetmek ve sorgulamak için hafif servis bulucu eklendi (`TD-08`).
- `GameSession.cs` (yeni): Sahnelerden bağımsız, oyun durumunu (akçe, erzak, gün sayısı, mevcut düğüm, seyahat durumu) tutan çalışma anı oturumu oluşturuldu; `EventBus` olayları (`LocationChangedEvent`, `DayPassedEvent`, `TravelStartedEvent`) ile bağlandı.
- `Bootstrapper.cs` (yeni): `00_Boot.unity` sahnesinde çalışan, `[RuntimeInitializeOnLoadMethod]` ile her sahneden Play basıldığında dahi çekirdek servisleri (`SaveService`, `GameSession`, `EventBus`, `AudioManager`) güvenceye alan kalıcı başlatıcı eklendi.
- `MainMenuUI.cs`, `WorldMapUI.cs`: `GameSession` oturumu üzerinden akçe/erzak ve konum okumaya geçirildi; sahneler arası statik veri bağımlılıkları temizlendi.
- Unity'de elle yapılanlar: `00_Boot.unity` sahnesi oluşturuldu, Build Settings'te ilk sıraya (Index 0) yerleştirildi ve `Bootstrapper` objesi eklendi.
- Commit: `feat(core): A2.2 00_Boot sahnesi, Services servis bulucu ve GameSession calisma ani omurgasi`

---

## [16.10.2026] A2.1 — Kimlik Birliği (NodeId + MapNodeView)
- `MapNodeView.cs` (yeni): Haritadaki 13 düğüm nesnesini `LocationNodeSO` referansına ve ASCII `NodeId`'ye bağlayan sahne görünüm bileşeni eklendi (`BUG-02`).
- 13 harita düğümü için `LocationNodeSO` eksikleri (`Ahlat, Ankara, Antalya, Cakalli, Eskisehir, Izmir, Karatay, Kutahya, Tashan, Trabzon`) tamamlandı (`BUG-02`).
- `WorldMapUI.cs`, `BottomNodeInfoCardUI.cs`: Türkçe görünen isim ve `GameObject.name` eşleştirmeleri kaldırıldı; tüm karşılaştırmalar `NodeId` ve `StringComparison.Ordinal` üzerine taşındı (`BUG-03`, `TD-06`).
- Unity'de elle yapılanlar: `WorldMapScene`'deki 13 düğüme `MapNodeView` bileşeni eklendi ve ilgili `LocationNodeSO` referansları Inspector'dan bağlandı.
- Commit: `fix(map): A2.1 harita dugumlerinde NodeId kimlik birligi ve LocationNodeSO baglantisi (BUG-02, BUG-03, TD-06)`

---

## [16.10.2026] A2.0 — Hijyen (Input System, DevOverlay, Ses Kalıcılığı)
- `MapPanZoomController.cs`: Eski `UnityEngine.Input` çağrıları yeni Input System'e (`EnhancedTouch.Touch.activeTouches`) taşındı (`BUG-01`).
- `KervanSceneNavOverlay.cs`: `#if UNITY_EDITOR || DEVELOPMENT_BUILD` koruması içine alındı (`BUG-06`).
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
