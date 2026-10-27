# 🐞 Bilinen Sorunlar ve Teknik Borç Kaydı

> **Kullanım (AI):** Bu dosyayı baştan sona okuma. Dokunacağın dosyanın adını ara (ör. `WorldMapUI`). Kayıt varsa planına dahil et.
> **Kaynak:** 14.10.2026 salt okunur analiz (`Docs/Arsiv/2026-10-14-Analiz-Raporu.md`). Kod okumasıyla tespit edildi; "Doğrulama" sütunu `?` olanlar Unity'de teyit edilmeli.
> **Önem:** 🔴 Kritik · 🟠 Yüksek · 🟡 Orta · ⚪ Düşük | **Durum:** Açık / Çözüldü (tarih + adım)
> Yeni sorun bulunduğunda sona eklenir; ID'ler asla yeniden kullanılmaz.

---

## BUG — Hatalı davranış

| ID | Önem | Sorun | Dosya / Yer | Çözecek Adım | Doğrulama | Durum |
|---|---|---|---|---|---|---|
| BUG-01 | 🔴 | Proje yalnızca yeni Input System kullanıyor (`activeInputHandler: 1`), ama pinch-zoom eski `Input.touchCount/GetTouch` ile `Update()` içinde okunuyor → muhtemelen her karede `InvalidOperationException`, telefonda zoom çalışmaz. | `UI/MapPanZoomController.cs` (`HandleMobilePinchZoom`, `OnBeginDrag`, `OnDrag`) | A2.0 | Teyit | Çözüldü (16.10.2026) |
| BUG-02 | 🔴 | Harita düğümleri ile SO'lar eşleşmiyor: sahnede 13 düğüm (`Node_Ahlat, Ankara, Antalya, Bursa, Cakalli, Eskisehir, Izmir, Iznik, Karatay, Kutahya, Sogut, Tashan, Trabzon`), SO'da 6 (`Bilecik, Bursa, Domanic, Iznik, KaratayHan, Sogut`). Eşleştirme GameObject adı ↔ `NodeName` ("Söğüt", "Bursa (Prusa)") ile yapıldığından hiçbiri tutmuyor; bilgi kartı varsayılan veri gösteriyor. | `WorldMapUI.cs` (`OnNodeViewClicked`, `SetupNodeViews`), `MapNodeView.cs` | A2.1 | Teyit | Çözüldü (16.10.2026) |
| BUG-03 | 🔴 | Yeni oyunda `CurrentCity="Söğüt"`, tıklanan düğüm "Sogut" → `isCurrentLocation=false`. Kendi şehrine tıklayınca "Şehre Gir" yerine 2–5 günlük **rastgele** seyahat çıkabilir. Marker yalnızca `Node_Sogut` fallback'i sayesinde doğru yerde. | `WorldMapUI.cs`, `MainMenuUI.cs` (`OnNewGameClicked`) | A2.1 | Teyit | Çözüldü (16.10.2026) |
| BUG-04 | 🔴 | "Seyahat Et"te `CurrentCity` yola çıkmadan hedefe yazılıyor; pusu/yenilgi/uygulama kapanması durumunda oyuncu hedefte görünür. | `WorldMapUI.cs` (`OnNodeActionConfirmed`), `GameSession.cs` (`StartTravel`) | A2.4 / A3.4 | Teyit | Çözüldü (19.10.2026) |
| BUG-05 | 🔴 | `TravelManager` kendi `new CaravanData()` kopyasını tutuyor; seyahatte harcanan erzak/akçe `CaravanManager`'daki kervana yansımıyor (`GameSession` + `CaravanManager` köprüsü A2.2'de kuruldu; `TravelManager` entegrasyonu A3.2'de). | `TradeAndTravel/TravelManager.cs` | A3.2 | Kod | Açık |
| BUG-06 | 🟠 | Test gezinme menüsü `[RuntimeInitializeOnLoadMethod]` ile her sahnede ve **release build'de** açılıyor; sağ üstte Utility Drawer ile çakışıyor. | `Core/KervanSceneNavOverlay.cs` | A2.0 | Kod | Çözüldü (16.10.2026) |
| BUG-07 | 🟠 | Mute iki sahnede farklı: Menü sadece AudioManager'ı susturuyor; Harita ayrıca `AudioListener.pause=true` + tüm AudioSource'ları `mute` ediyor (sahne değişince tutarsız). Ses ayarları kalıcı değil. | `WorldMapUI.cs` (`OnVolumeToggleClicked`), `MainMenuUI.cs`, `Audio/AudioManager.cs` | A2.0 | Teyit | Çözüldü (16.10.2026) |

## TD — Teknik Borç / Mimari

| ID | Önem | Sorun | Dosya / Yer | Çözecek Adım | Durum |
|---|---|---|---|---|---|
| TD-01 | 🟠 | Kayıt sistemi yok; sadece `PlayerPrefs` bayrakları (`HasSavedGame`, `CurrentNodeId`, `HasSeenIntro_v1`). Kervan her açılışta 500 akçe/20 erzak ile sıfırlanır. | `MainMenuUI.cs`, `WorldMapUI.cs`, `CaravanData.cs` | A2.3 | Çözüldü (18.10.2026) |
| TD-02 | 🟠 | Sabit kodlu oyun değerleri: tarih `"14 Mayıs 1326"` ve varsayılan akçe/erzak UI fallback'leri (TopBar HUD A2.5'te GameCalendar ve TopBarHUD'a taşınarak çözüldü; kalan: `InventorySidebarUI` `RefreshData`). | `InventorySidebarUI.cs` (`RefreshData`) | A2.7 | Açık (TopBar çözüldü) |
| TD-03 | 🟠 | Sidebar kervan boşken sahte satırlar basıyor ("3 Muhafız, 10 İpek…"); görevler sabit metin. | `InventorySidebarUI.cs` (`RefreshData`) | A2.7 | Çözüldü (25.10.2026) |
| TD-04 | 🟠 | Rota grafiği yok: 13 düğüm, 4 rota (`Sogut_Bilecik`, `Sogut_Domanic`, `Bilecik_Iznik`, `Iznik_Bursa`). Her düğüme her yerden gidilebiliyor. | `ScriptableObjects/Routes`, `RouteDatabaseSO`, `RouteGraph` | A2.4 | Çözüldü (19.10.2026) |
| TD-05 | 🟠 | `Resources.Load` 7 yerde çağrılıyor ama `_Project` altında `Resources` klasörü yok → fallback'ler ölü kod (`AudioManager`, `WorldMapUI`, `BottomNodeInfoCardUI`, `InventorySidebarUI` A2.0–A2.7'de temizlendi; kalan: `UIButtonSound`). | `UIButtonSound` | A7 | Açık (çoğu temizlendi) |
| TD-06 | 🟡 | İsim tabanlı bağlama: `GameObject.Find("Btn_Inventory") ?? ...`, `name.StartsWith("Node_")`. Obje adı değişince sessizce bozulur. | `WorldMapUI.cs` (`FindMissingReferences`, `SetupNodeViews`) | A2.1 | Çözüldü (16.10.2026) |
| TD-07 | 🟡 | Sahne geçişleri tutarsız: sadece MainMenu'de `ScreenFader` var; diğer geçişler sert `LoadScene`, async/yükleme ekranı yok. | `ScreenFader.cs`, tüm `SceneManager.LoadScene` çağrıları | A2.6 | Çözüldü (25.10.2026) |
| TD-08 | 🟡 | Singleton yoğunluğu: `AudioManager`, `CaravanManager`, `QuestManager`, `ReputationManager`, `ShopManager`, `ScreenFader`. (`00_Boot` + `Services` + `GameSession` A2.2'de kuruldu; kalan yöneticiler A6.1'de servise taşınacak.) | Çoklu | A6.1 | Açık |
| TD-09 | 🟡 | EventBus sadece 7 olay tipiyle az kullanılıyor; `TravelManager` C# `event` + EventBus karışık. `EventBus.ClearAll()` hiç çağrılmıyor (Domain Reload kapalıysa eski abonelik riski). | `Core/EventBus.cs`, `Core/Bootstrapper.cs`, `Core/GameSession.cs` | A2.2 | Çözüldü (18.10.2026) |
| TD-10 | ⚪ | Eski dosyalarda yorumlar Türkçe karaktersiz (`Kervanin seyahatini...`). Dosyaya dokunuldukça düzeltilir. | A0 dönemi script'ler | Fırsatçı | Açık |
| TD-11 | 🟡 | `FactionType` enum'u (Ottoman, Steppe, SeljukLegacy, Byzantine, Bandit) GDD'deki 4 fraksiyonla (Osmanlı, Bizans, Eşkıya, Vahşi Hayvan) uyuşmuyor (`LocationNodeSO.GetFactionDisplayName` eklendi; enum sadeleştirmesi `ReputationManager`/`TownHubUI` ile A6.1'de). | `Core/GameEnums.cs` | A6.1 (KR-2 onaylandı) | Açık |
| TD-12 | 🟡 | "Yeni Oyun" mevcut kaydı onaysız eziyor; intro/kervan gibi durumları sıfırlamıyor. | `MainMenuUI.cs` (`OnNewGameClicked`) | A2.3 | Çözüldü (18.10.2026) |
| TD-13 | 🟡 | `InventorySidebarUI.cs` ~1084 satır "god class": sekme, layout, prosedürel `new GameObject` UI üretimi, safe-area, ses bir arada. | `UI/InventorySidebarUI.cs` | A2.7 (başlangıç), A7 | Açık |
| TD-14 | ⚪ | Sahne adları string olarak 13 yerde dağınık. | Çoklu | A2.6 | Çözüldü (25.10.2026) |
| TD-15 | ⚪ | Çok sayıda `Debug.Log`; release'de kapatılmıyor. | Çoklu | A8 | Açık |
| TD-16 | ⚪ | Sürüm iki kaynakta: `PlayerSettings.bundleVersion=1.0`, UI'da sabit `v0.1.0 Alpha`. | `MainMenuUI.cs` (`_versionString`) | A8 | Açık |
| TD-17 | ⚪ | `QuestManager`, `ReputationManager`, `ShopManager` hiçbir sahnede yok → TownHub özellikleri çalışmaz. | `IntelSystem/*` | A6.1 | Açık |

## AST — Varlık (Asset) Hijyeni

| ID | Önem | Sorun | Çözecek Adım | Durum |
|---|---|---|---|---|
| AST-01 | ⚪ | Standart dışı klasörler: `Art/altmenu`, `Art/Icon`, `Art/Sidebar`, `Art/Toolbar`, `Art/Settings`; boş `Art/Tiles`, boş `Scripts/Core/Editor`. | A7 (veya ilgili ekranın adımında) | Açık |
| AST-02 | ⚪ | Mükerrer dosyalar: `exitIcon.png` ×3; `Item_Akce`/`Item_Erzak` hem `Items/` hem `UI/Icons/`; `UI_BG_Button` ≈ `UI_Btn_Standard`; `Icon_MusicOn` / `Icon_Util_VolumeOn` / `Icon/MusicOn`. | A7 | Açık |
| AST-03 | ⚪ | Yazım hataları: `Item_Healt`, `Mauhafiz_ProfileCard`, `UI_TollbarItemBG`, `BG_Travel_03_MidHiils`, (kod) `SeperatorContainer`. | A7 | Açık |
| AST-04 | ⚪ | Önek kuralına uymayanlar: `BottomMenuBtn`, `Btn_Exit`, `sidebarBtnOnClick`, `SidebarBtnOffClick`, `toolbarAkceIcon`, `toolbarErzakIcon`, `BottomMenuIconBG`. | İlgili ekranın adımı (ör. A2.5 toolbar) | Açık |
| AST-05 | 🟡 | Ham dosyalar `Assets/` içinde: `Art/SourceFiles/*.ai` (~14 MB), `Art/UI/Mockups` (~10 MB). Git LFS yok (repo pack ~70 MB). | A7 (KR-5 onaylandı) | Açık |
| AST-06 | 🟡 | Büyük dokular: `UI_WorldMapBackground` (5 MB), `UI_BG_MainMenu` (3.8 MB), `UI_BG_Paper` (2.6 MB) — Max Size/ASTC ayarları kontrol edilmeli. | A2.8 / A7 | Açık |
| AST-07 | ⚪ | Şablon kalıntıları: `Assets/Scenes/SampleScene.unity`, `Assets/Welcome/`, `TextMesh Pro/Examples & Extras` (~30 örnek sahne). | A7 | Açık |
| AST-08 | ⚪ | `WorldMapScene.unity` 1.6 MB (diğer sahnelerin ~12 katı); prefab'lanmamış çok sayıda UI objesi olabilir — merge çakışması riski. | A2.5–A2.7 (parçaları prefab'a alarak) | Açık |
| AST-09 | ⚪ | `BG_Travel_03_MidHiils.png` parallax katmanında yatay döngü (seamless loop) birleşim izi belli oluyor; son aşamada (A7) kenar geçişi (offset/inpaint) düzeltilecek veya görsel yenilenecek. | A7 | Açık |

---

## ✅ Çözülenler
> Çözülen kayıt tablodan buraya taşınır: `BUG-01 — 15.10.2026 — A2.0 — kısa not`

- `BUG-01` — 16.10.2026 — A2.0 — `MapPanZoomController` içindeki eski `UnityEngine.Input` çağrıları `EnhancedTouch.Touch.activeTouches` ile değiştirildi.
- `BUG-06` — 16.10.2026 — A2.0 — `KervanSceneNavOverlay` `#if UNITY_EDITOR || DEVELOPMENT_BUILD` koruması içine alındı.
- `BUG-07` — 16.10.2026 — A2.0 — `AudioManager` üzerine `PlayerPrefs` ses/mute kalıcılığı ve `ToggleMute` API'si eklendi; `MainMenuUI` ve `WorldMapUI` tek merkezden senkronize edildi.
- `BUG-02` — 16.10.2026 — A2.1 — 13 harita düğümü için `LocationNodeSO` tamamlandı ve `MapNodeView` üzerinden doğrudan referansla bağlandı.
- `BUG-03` — 16.10.2026 — A2.1 — Konum eşleştirmesi görünen isim yerine ASCII `NodeId` (`"node_sogut"`) ve `StringComparison.Ordinal` üzerine taşındı.
- `TD-06` — 16.10.2026 — A2.1 — `WorldMapUI` içindeki `GameObject.Find` ve `name.StartsWith("Node_")` isim taramaları kaldırıldı.
- `TD-09` — 18.10.2026 — A2.2 — `Bootstrapper` içinde `SubsystemRegistration` aşamasında `EventBus.ClearAll()` ve `Services.Clear()` çağrısı eklendi; `GameSession` durum değişim olayları (`LocationChangedEvent`, `DayPassedEvent`, `TravelStartedEvent`) EventBus'a bağlandı.
- `TD-01` — 18.10.2026 — A2.3 — `SaveData` (`saveVersion = 1`) ve atomik yazma (`save_0.tmp` → `save_0.json`) yapan `SaveService` kuruldu; `PlayerPrefs` oyun durumu bayrakları kaldırıldı ve durum değişimlerinde / `OnApplicationPause` / `OnApplicationQuit` anlarında otomatik kayıt bağlandı.
- `TD-12` — 18.10.2026 — A2.3 — `MainMenuUI` içinde mevcut kayıt varken "Yeni Oyun" tıklandığında onay penceresi (`ShowCustomConfirm`) açılması ve onaylandığında `GameSession.StartNewGame` ile tüm durumun sıfırlanması sağlandı.
- `BUG-04` — 19.10.2026 — A2.4 — `GameSession.StartTravel` içindeki erken `SetCurrentNode(toNodeId)` çağrısı kaldırıldı; seyahat başlarken `CurrentNodeId` korunup yalnızca `ActiveTravel` kaydediliyor, konum varışta (`CompleteTravel`) güncelleniyor.
- `TD-04` — 19.10.2026 — A2.4 — `RouteDatabaseSO` ve saf C# `RouteGraph` (çift yönlü komşuluk + Dijkstra en kısa yol) eklendi; `WorldMapUI` ve `BottomNodeInfoCardUI` gerçek `TravelRouteSO` verilerine bağlandı, komşu olmayan düğümlere doğrudan seyahat kapatıldı.
- `TD-02` (kısmen) — 25.10.2026 — A2.5 — `WorldMapUI` içindeki sabit kodlu tarih ve HUD varsayılanları kaldırıldı; `GameCalendar` ve `TopBarHUD` ile dinamik `GameSession` verisine bağlandı (sayaç animasyonu ve mevsim desteği eklendi).
- `TD-07` — 25.10.2026 — A2.6 — Kalıcı `ScreenFader` (`DontDestroyOnLoad`, `sortingOrder = 32767`) ve `SceneLoader` asenkron sahne yükleyicisi kuruldu; tüm sahne geçişleri tek merkezden fade ve çift dokunma korumasıyla bağlandı.
- `TD-14` — 25.10.2026 — A2.6 — `SceneIds` sabit sınıfı oluşturuldu; `Bootstrapper`, `MainMenuUI`, `WorldMapUI` ve `KervanSceneNavOverlay` içindeki dağınık string sahne adları sabitlere taşındı.
- `TD-03` — 25.10.2026 — A2.7 — Sidebar kervan ve yük sekmeleri GameSession verisine bağlandı; sahte muhafız/emtia satırları kaldırıldı, boş durumlar ve esnek görev kartı eklendi.
