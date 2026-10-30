# 📜 Değişiklik Günlüğü (Changelog)

> En yeni kayıt **en üstte**. Her kapanan adım için `gorev-kapat` tarafından eklenir.
> Format: `## [Tarih] Adım-ID — Başlık` + kısa maddeler + commit mesajı.
> AI bu dosyayı yalnızca geçmiş sorulduğunda veya kayıt eklerken açar.

---

## [31.10.2026] A5.1 — Olay Penceresi: Kaç / Haraç Ver / Savaş Karar Kümesi
- `RoadEventSO.cs`: `FleeChance` (%65 varsayılan), `TributeAkceCost` (30 akçe varsayılan) ve `CreateRuntimeInstance` parametreleri eklendi.
- `TravelManager.cs`: Pusularda anlık hasarın oyuncu seçimi öncesinde uygulanması engellendi; `AmbushResolutionResult` veri yapısı ile `ResolveAmbushFlee()`, `ResolveAmbushTribute()` ve `ResolveAmbushFight()` metotları oluşturuldu.
- `TravelUI.cs`: `EventModal` içine `_choiceContainer` 3'lü karar butonları (`_btnChoiceFlee`, `_btnChoiceTribute`, `_btnChoiceFight`), dinamik buton metinleri (% şans, akçe maliyeti, yabani hayvan koruması) ve sonuç bildirimleri bağlandı.
- Unity'de elle: `TravelScene` içinde `EventModal / ModalCard` altına `ChoiceContainer` (HorizontalLayoutGroup) ve 3 karar butonu yerleştirilip `TravelUI` bileşenine bağlandı.
- Commit: `feat(travel): A5.1 olay ve pusu penceresine Kac / Harac Ver / Savas karar kumesi eklendi`

---

## 🏆 [31.10.2026] A4 — Şehir: Pazar, Kervansaray ve Şehir Merkezi (Aşama Özeti)
- **Kapsam:** A4.1–A4.4 arasındaki 4 adım başarıyla tamamlandı.
- **Kazanımlar:**
  1. **3 Sekmeli Şehir Merkezi (`TownHubScene`):** Pazar, Kervansaray ve Divan için özel 14. yy minyatür arka planları (`UI_BG_*`), TopBar HUD, dinamik şehir sancağı ve haritaya güvenli dönüş akışı kuruldu.
  2. **10 Emtialı Dinamik Pazar (`CityMarket` & `MarketUI`):** Yerel üretim/kıtlık kademeleri, ortalama alış maliyetiyle anlık `+%Kâr ▲` / `-%Zarar ▼` hesabı, miktar slider'ı ve güvenli ticaret döngüsü bağlandı.
  3. **Pazar Doygunluğu & Kalıcı Piyasa:** Aynı şehre mal satıldıkça fiyatların düşmesi, azalan stokta kıtlık primi, `save_0.json` kalıcılığı ve günler geçtikçe (`DayPassedEvent`) pazarların başlangıç dengesine toparlanması (`ApplyDailyMarketRecovery`) sağlandı.
  4. **Kervansaray Hizmetleri (`CaravanseraiInnUI`):** Handa 1 gün konaklayarak gün atlatma (şifa + moral), erzak ikmali, muhafız/okçu kiralama, katır/deve ile yük kapasitesi artırma, dedikodu dinleme ve 50 Akçe karşılığı casus tutma işlevleri tamamlandı.
- **Önerilen Git Etiketi:** `v0.4.0-alpha`

---

## [31.10.2026] A4.4 — Kervansaray: Konaklama, Erzak İkmali, Muhafız/Hayvan Kiralama ve Casus Tutma
- `CaravanseraiInnUI.cs`: Kervansarayın 3 hizmet kartını (`Card_RestAndSupply`, `Card_HireGuards`, `Card_HireMounts`) ve alt istihbarat barını (`BottomBar_InnMeta`) yöneten arayüz kontrolcüsü oluşturuldu; derin arama (`FindDeepChild`) ile buton ve metinlerin otomatik bağlanması, ses/kayıt entegrasyonu sağlandı.
- `CaravanData.cs`: `PackAnimalType` enum'u, `RestAtInn` (gün atlatma + şifa + moral), `BuySupplies` (+10 erzak), `AddPackAnimal` (katır/deve ile `MaxWeightCapacity` artışı), `RecruitGuardWithCost`, `ClearGuards` ve `RestoreGuardRecord` API'leri eklendi.
- `GameSession.cs`: `ApplySaveData` içine diskten yüklenen muhafız kayıtlarının (`CaravanSaveData.guards`) `_caravan.Guards` listesine eksiksiz geri yüklenmesi bağlandı.
- `UnitDataSO.cs`: Boş veya dinamik birim oluşturulmasında `UnitName` ve `UnitId` için güvenli non-null fallback'ler ve `Initialize` metodu eklendi.
- `InventorySidebarUI.cs`: Muhafız gruplama sözlüğünde `uName` için `StringComparer.Ordinal` ve `null` koruması eklenerek `ArgumentNullException` giderildi.
- `TownHubUI.cs`: `EnsureCaravanseraiComponent` ile `CaravanseraiInnUI` bileşeninin `Panel_Caravanserai` üzerinde çalışma anında otomatik güvenceye alınması ve sekme geçişlerinde yenilenmesi sağlandı.
- Commit: `feat(townhub): A4.4 Kervansaray konaklama, erzak, muhafiz-hayvan kiralama ve casusluk entegrasyonu ile A4 asama kapanisi`

---

## [31.10.2026] A4.3 — Pazar Doygunluğu (`MarketSaturation`), Kalıcı Kayıt ve Günlük Toparlanma
- `SaveData.cs`: Şehir ve emtia bazlı pazar durumlarını saklayan `CityMarketNodeSaveData` ve `CityMarketItemSaveData` modelleri eklendi; `SaveData.cityMarkets` alanı tanımlandı.
- `SaveService.cs`: `ValidateAndMigrate` içine eski kayıt dosyalarıyla geriye dönük uyumluluk için `data.cityMarkets` null koruması eklendi.
- `GameSession.cs`: `_cityMarketStates` oturum sözlüğü (`TryGetMarketItemState`, `SetMarketItemState`) eklendi; `AdvanceDays` içine seyahat ve konaklamalarda tüm şehir pazarlarının doygunluğunu eriten ve stoklarını başlangıç dengesine yaklaştıran `ApplyDailyMarketRecovery` bağlandı; `ToSaveData` ve `ApplySaveData` kalıcı kayda entegre edildi.
- `CityMarket.cs`: `LiveMarketItem.InitialStock` alanı eklendi; şehir pazarı kurulurken `GameSession`'dan kayıtlı stok/doygunluk verisi çekildi; alış/satış işlemlerinde anlık pazar durumu oturuma yazıldı; stok azaldıkça devreye giren kıtlık primi (`CalculateScarcityBonus`) ve gün geçişlerini anlık yakalayan `DayPassedEvent` aboneliği eklendi.
- `MarketUI.cs`: Pazar emtia satırlarına başlangıç stoğu bilgisi (`Mevcut/Başlangıç Adet`) ve dinamik durum rozetleri (`[Arz Fazlası · -%X]`, `[Pazar Doygun · -%X]`, `[Azalan Stok · +%X]`) eklendi.
- Unity'de elle yapılanlar: Yok (saf C# mantık ve mevcut pazar arayüzü güncellemesi).
- Commit: `feat(market): A4.3 pazar doygunlugu, dinamik stok ve kalici kayit sistemi eklendi`

---

## [30.10.2026] A4.2 — Pazar Sistemi, 4 Yeni `ItemDataSO`, `GameSession` Bağlantısı & Alış-Satış UI
- `CaravanData.cs`: `FindSlot`, `ClearInventory` ve `RestoreInventorySlot` metotları eklendi; tüm `ItemId` karşılaştırmaları `StringComparison.Ordinal` standardına çekildi.
- `GameSession.cs`: Diskten okunan `InventorySlotSaveData` kayıtlarının korunması ve `ResolveSavedInventoryItems` ile `CaravanData.Inventory` listesine dönüştürülmesi sağlandı.
- `CityMarket.cs`: `GameSession.CurrentNodeId` üzerinden aktif şehrin pazarını (`_allCityMarkets` + `_allTradeItems` 10 emtia kataloğuyla) otomatik kuran ve `MarketSupplyTier` (`LocalAbundant`, `Standard`, `HighDemand`) sınıflandırması yapan yapıya geçirildi.
- `MarketUI.cs`: İzole `CaravanData` ve sahte `+400 Akçe` kodu kaldırılıp doğrudan `GameSession.Caravan`'a bağlandı; kurumsal çerçevelerle (`UI_BottomMenuInfoBG`, `UI_BG_Itemicon`) sol ambar (`+%Kâr ▲` / `-%Zarar ▼` rozeti + boş durum) ve sağ şehir pazarı satırlarının dinamik üretimi, `[-]`, `Slider`, `[+]`, `[MAKS]` ve `SATIN AL` / `SAT` akışı tamamlandı.
- Unity'de elle yapılanlar: `Item_Zeytinyagi`, `Item_Deri`, `Item_Hali`, `Item_Cini` (`ItemDataSO`) oluşturuldu; `TownHubScene` içinde `Panel_Market`, `Panel_Caravanserai`, `Panel_Divan` içerikleri kuruldu ve `CityMarket` / `MarketUI` bileşenleri bağlandı.
- Commit: `feat(market): A4.2 sehir pazari GameSession entegrasyonu, 4 yeni emtia ve kar-zarar rozetli MarketUI`

---

## [29.10.2026] A4.1 — Şehir Hub Tasarımı & 3 Sekmeli Sahne İskeleti (`TownHubScene`)
- `RouteDatabaseSO.cs`: Kayıtlı rotalar üzerinden ASCII `nodeId` (`StringComparison.Ordinal`) ile `LocationNodeSO` çözümleyen `FindNodeById(string nodeId)` metodu eklendi.
- `TownHubUI.cs`: 3 sekmeli (`Market`, `Caravanserai`, `Divan`) merkez yapısına taşındı; sekmeye göre tam ekran arka plan (`_bgMarket`, `_bgCaravanserai`, `_bgDivan`) ve aktif/pasif sekme görseli değişimi, `GameSession.CurrentNodeId` üzerinden dinamik şehir/fraksiyon başlığı, kervansaray düğümlerinde tesis kilitleme (`!HasMarket`), `SceneLoader.Load(SceneIds.WorldMap)` ve Android Geri tuşu (`Escape`) desteği eklendi.
- `MenuDrawerController.cs`: `TopBar_HUD` içindeki `UtilityDrawer` alt butonlarının (`Btn_Volume`, `Btn_Settings`, `Btn_Exit`) `WorldMapScene` dışındaki sahnelerde de otomatik çözümlenip çalışması sağlandı.
- Varlıklar: `UI_BG_Market.png`, `UI_BG_Caravanserai.jpg`, `UI_BG_Divan.jpg` arka planları; `Icon_Tab_Market.png`, `Icon_Tab_Inn.png`, `Icon_Tab_Divan.png` sekme ikonları; `Item_Cini.png`, `Item_Hali.png`, `Item_Deri.png`, `Item_Zeytinyagi.png` ticaret malı ikonları projeye eklendi.
- Unity'de elle yapılanlar: `TownHubScene` içinde 3 kuşaklı mizanpaj (`TopBar_HUD` + `Btn_ReturnToMap`, `CityBanner` + `TabsMenu`, `ContentArea` altında `Panel_Market`/`Panel_Caravanserai`/`Panel_Divan`), `AudioManager`, `Popup_Settings` ve `Main Camera` `AudioListener` bileşeni kuruldu; sahne müziği `BGM_Market` olarak ayarlandı.
- Commit: `feat(townhub): A4.1 3 sekmeli sehir merkezi iskeleti, ozel arka planlar ve TopBar cekmece entegrasyonu`

---

## 🏆 [28.10.2026] A3 — Seyahat (`TravelScene` Yeniden) (Aşama Özeti)
- **Kapsam:** A3.1–A3.4 arasındaki 4 adım başarıyla tamamlandı.
- **Kazanımlar:**
  1. **Minyatür Parallax Sahnesi:** 4 katmanlı (`Sky`, `Mountains`, `MidHills`, `Roads`) `RawImage` UV-scroll akışı (`TravelParallaxUI`) kuruldu.
  2. **Kervan Konvoyu & HUD:** 5 yan profil birim (`Unit_Travel_*`) faz farklı adımlama salınımıyla (`CaravanConvoyUI`) sahneye yerleştirildi; `TopBar_HUD` ve `BottomTravelBar` (`1x/2x Tempo`, `Mola Ver`, `ProgressSlider`) entegre edildi.
  3. **Oturum & Kayıt Bütünlüğü (`BUG-05`):** `TravelManager` doğrudan `GameSession.Caravan` ve `GameSession.ActiveTravel` ile senkronize edildi; çift yönlü `RouteDatabaseSO` çözümlemesi ve varışta `CompleteTravel()` döngüsü tamamlandı.
  4. **Yol Olayları & Saldırı Altyapısı (`RoadEventSO`):** Lojistik hadiseler, olumlu karşılaşmalar, haydut pusuları ve yabani hayvan saldırıları için `RoadEventSO` veri yapısı ve `EventModal` penceresi kuruldu.
- **Önerilen Git Etiketi:** `v0.3.0-alpha`

---

## [28.10.2026] A3.4 — Kamp/Mola Penceresi (`EventModal`) & Yol Olayları Altyapısı (`RoadEventSO`)
- `RoadEventSO.cs`: ASCII `EventId`, 4 kategori (`Incident`, `Positive`, `BanditAmbush`, `WildBeastAttack`), akçe/erzak/şifa etkileri, ağırlıklı seçim ve A5 savaş hazırlık alanlarını içeren ScriptableObject eklendi.
- `TravelRouteSO.cs`: Rotaya özel `_roadEvents` (`List<RoadEventSO>`) alanı eklendi; Türkçe karaktersiz açıklamalar düzeltildi (`TD-10`).
- `TravelManager.cs`: Gömülü olay sabitleri kaldırılıp `RoadEventSO` ağırlıklı seçim sistemine taşındı; `GameSession.ActiveTravel.DaysElapsed` senkronizasyonu ve 5'li çalışma anı yedek olay havuzu eklendi.
- `TravelUI.cs`: `EventModal` açılışında parşömen sesi, renkli etki özeti ve `"Yola Devam Et"` / `"Haritaya Dön"` buton akışı bağlandı.
- Unity'de elle yapılanlar: `TravelScene > SafeAreaContainer` altına `UI_BottomMenuBG` ve `BottomMenuBtn` ile `EventModal` kuruldu ve `TravelUI` bileşenine bağlandı.
- Commit: `feat(travel): A3.4 RoadEventSO yol olaylari altyapisi ve EventModal penceresi ile A3 asama kapanisi`

---

## [27.10.2026] A3.2 & A3.3 — Kervan Konvoyu, Seyahat HUD ve `GameSession` Bağlantısı (`BUG-05`)
- `CaravanConvoyUI.cs`: Yol üzerinde ilerleyen kervan birimlerine (`Unit_Travel_AlpMounted`, `Camel`, `Mule`, `Muhafiz`, `Okcu`) faz farkıyla adımlama yaylanması (`bobbing`), yürüyüş eğimi (`tilt`) ve yere basma esnemesi (`squash`) uygulayan görünüm bileşeni eklendi. Gerçek bacak adımlama animasyonu Cila aşaması (A7) için `AST-10` olarak not edildi.
- `TravelManager.cs`: Ayrı `_caravanData` kopyası kaldırılıp doğrudan `GameSession.Caravan`'a bağlandı (`BUG-05` çözüldü); `RouteDatabaseSO` çift yönlü rota çözümleme, `1x/2x` hız çarpanı (`1.2s` varsayılan gün süresi) ve varışta `GameSession.CompleteTravel()` entegrasyonu tamamlandı.
- `TravelUI.cs`: `TravelParallaxUI` ve `CaravanConvoyUI` akış senkronizasyonu, `1x/2x Tempo` ve `Mola Ver` kontrolleri, Türkçe arazi/hava etiketleri, `SceneLoader` sahne geçişleri ve Android Geri tuşu eklendi.
- Unity'de elle yapılanlar: `TravelScene` içinde `CaravanConvoy`, `SafeAreaContainer`, `TopBar_HUD.prefab`, `BottomTravelBar` (`Btn_Speed`, `ProgressSlider`, `OriginText`, `RouteInfoText`, `DestText`, `Btn_PauseResume`) ve `TravelManager` kuruldu.
- Commit: `feat(travel): A3.2-A3.3 kervan konvoyu, seyahat HUD ve GameSession baglantisi (BUG-05)`

---

## [27.10.2026] A3.1 — Seyahat Tasarım Önerisi & Parallax Arka Plan Kurulumu
- `TravelParallaxUI.cs`: 4 katmanlı minyatür seyahat arka planını `RawImage.uvRect` üzerinden sıfır GC ile kaydıran, mola (`SetPaused`) ve hız çarpanı (`SetSpeedMultiplier`) destekli UI bileşeni oluşturuldu.
- Varlıklar: `BG_Travel_01_Sky.png`, `BG_Travel_02_Mountains.png`, `BG_Travel_03_MidHiils.png`, `BG_Travel_04_Roads.png` eklendi (`Sprite Mode: Single`, `Wrap Mode: Repeat`). `MidHiils` katmanındaki yatay döngü birleşim izi Cila aşaması (A7) için `AST-09` olarak kayıt altına alındı.
- Unity'de elle yapılanlar: `TravelScene` içindeki eski prototip objeler temizlendi; `TravelCanvas` ve `ParallaxBackground` altında 4 `RawImage` katmanı oluşturulup `TravelParallaxUI` bileşenine bağlandı.
- Commit: `feat(travel): A3.1 seyahat 4 katmanli parallax arka plan kurulumu ve TravelParallaxUI`

---

## [25.10.2026] A2.8 — Cihaz Testi & Aşama Kapanışı
- `MainMenuUI.cs`: Yeni Input System (`Keyboard.current.escapeKey`) ile Android Geri tuşu dinlemesi eklendi; açık olan Ayarlar, Yeni Oyun onayı veya Çıkış onay popup'ları ve sağ alt çekmece sırayla kapatılıyor, pencereler kapalıyken çıkış onayı tetikleniyor.
- `WorldMapUI.cs`: Yeni Input System (`Keyboard.current.escapeKey`) ile Android Geri tuşu dinlemesi eklendi; açık olan Ayarlar penceresi, Kervan kenar çubuğu (Sidebar), Şehir/Han bilgi kartı veya sağ üst çekmece sırayla kapatılıyor.
- `InventorySidebarUI.cs`: Çekmecenin açık/kapalı durumunu bildiren `IsOpen` özelliği eklendi.
- Unity'de elle yapılanlar: Device Simulator'da 16:9, 19.5:9 (çentikli) ve 20:9 ekran oranlarında test edildi. Giriş ekranındaki tam ekran arka plan görseli, çentik ve kenar boşluklarında kararma olmaması için `SafeAreaContainer` dışına taşındı.
- Commit: `feat(input): A2.8 Android Geri tusu destegi ve cihaz testi ile A2 asama kapanisi`

---

## 🏆 [25.10.2026] A2 — Dünya Haritası & Oyun Omurgası (Aşama Özeti)
- **Kapsam:** A2.0–A2.8 arasındaki 9 adım başarıyla tamamlandı.
- **Kazanımlar:**
  1. **Oyun Döngüsü & Oturum:** `00_Boot` sahnesi, `Bootstrapper`, `Services` servis bulucu ve oturum durumunu yöneten saf C# `GameSession` kuruldu.
  2. **Kayıt Sistemi:** JSON tabanlı atomik kayıt/yükleme (`SaveService`, `SaveData`, `save_0.json`) ve otomatik kayıt döngüsü tamamlandı.
  3. **Harita & Kimlik Birliği:** 13 şehir düğümü `MapNodeView` ve `LocationNodeSO` ile ASCII kimlikler (`NodeId`) üzerinden bağlandı; isim tabanlı aramalar kaldırıldı.
  4. **Rota Motoru:** `RouteDatabaseSO` ve saf C# `RouteGraph` (Dijkstra) ile gerçek mesafe, komşuluk ve aktarmalı seyahat altyapısı inşa edildi.
  5. **TopBar HUD:** `GameCalendar` (14 Mayıs 1326, mevsim döngüsü) ve bağımsız `TopBarHUD` (akçe animasyonu, erzak uyarısı) kuruldu.
  6. **Sahne Geçişleri:** Kalıcı `ScreenFader` ve asenkron `SceneLoader` mimarisi kuruldu, string sahne adları `SceneIds` sabitlerine taşındı.
  7. **Kenar Çubuğu (Sidebar):** Sahte veriler kaldırıldı; Kervan/Yük/Görevler sekmeleri canlı `GameSession` verisine ve boş durumlara bağlandı.
  8. **Mobil Uyumluluk:** Yeni Input System (`EnhancedTouch`, `Keyboard.current.escapeKey`), `SafeAreaFitter` ve Device Simulator testleri tamamlandı.
- **Git Etiketi:** `v0.2.0-alpha`

---

## [25.10.2026] A2.7 — Sidebar Gerçek Veri + Boş Durumlar
- `InventorySidebarUI.cs`: Çekmece verileri `Services.TryGet<GameSession>` üzerinden gerçek `Caravan` oturumuna bağlandı; `EventBus.Subscribe<CaravanStatsChangedEvent>` canlı aboneliği eklendi (`TD-03`).
- Sahte satır temizliği: Kervan sekmesinde sahte muhafızlar kaldırılıp lider (Gazi Alp) ve gerçek hayvanlar listelendi; Yük sekmesinde sahte emtialar kaldırılıp "Kervan ambarında henüz ticaret emtiası bulunmuyor..." boş durum metni eklendi (`TD-03`).
- Görevler (Quests) sekmesi: Aktif vazife yokken parşömen temalı boş durum metni eklendi; `CreateQuestRow` sabit 86 px yükseklikten esnek yüksekliğe (`minHeight = 96f`), negatif satır aralığı kaldırılıp `textWrappingMode = Normal` ve `lineSpacing = 1.5f` rahat tipografiye kavuşturuldu; `UI_BG_Itemicon` rozeti bağlandı; `AddQuest` API'si eklendi.
- Kapatma ve temizlik: `exitIcon` ve `close` isimli kapatma butonlarının otomatik bağlanması sağlandı; `PlayOpenSound` içindeki ölü `Resources.Load` temizlendi (`TD-05`); Inspector için `[ContextMenu]` eklendi.
- Unity'de elle yapılanlar: `InventorySidebar` içine `exitIcon.png` görseliyle `Btn_Exit` butonu eklendi, referanslar bağlandı.
- Commit: `feat(ui): A2.7 sidebar gerçek GameSession verisine bağlandı ve boş durumlar eklendi (TD-03, TD-05)`

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

---

## [~13.10.2026] A1 — Ana Menü & UI temelleri (tamamlandı + cilalandı)
- `UI_BG_MainMenu` + `UI_Logo_GameTitle`, Yeni Oyun / Devam Et butonları.
- Alt orta login seçenekleri (Google, Apple, Mail) — henüz işlevsiz (log basıyor).
- Sağ üst `UtilityDrawer` (kapalı başlangıç, okla açılma).
- `Popup_Settings` (`UI_Frame_Market` zemini, `SFX_PaperRustle` açılış, ses slider'ları, `Icon_Util_Close`).
- `BGM_Main.mp3` ve `SFX_ButtonClick.wav` AudioManager'a bağlandı.
- Cilalar: `ScreenFader.cs` (fade), `QuitConfirmPopupUI.cs` (çıkış onayı), `SafeAreaFitter.cs`, sürüm etiketi `v0.1.0 Alpha`, `PlayerPrefs` tabanlı "Devam Et" kontrolü, `UI_Btn_Standard.png`.
- Görsel Varlık Standardı `02-Teknik-ve-Gorsel.md` §7'ye eklendi.
- Commit: `6a5270d feat(ui): Asama 1 tamamlandi - Ana Menu, Cekmece, Ayarlar Popupu ve Varlik Organizasyonu`

---

## [≤07.10.2026] A0 — Hazırlık, mimari ve prototip sahneler
- ScriptableObject veri yapıları (Item, Unit, Node, Route, Market, Intel, Audio).
- Savaş matematiği (`DamageCalculator`, `HexGrid`, `HexPathfinder`, `CombatTurnManager`, `BanditAI`).
- Pazar ve envanter omurgası (`CityMarket`, `CaravanData`), istihbarat/itibar/dükkân (`QuestManager`, `ReputationManager`, `ShopManager`).
- Prototip sahneler: `TravelScene`, `MarketScene`, `TownHubScene`, `CombatScene` (**eski/prototip** kabul edilir — bkz. `03` Açık Kararlar KR-3).
- Önceki commit'ler: `e88c0c5`, `fcf8ce9`, `350ee3f`, `0f1bb47`, `d8bc3c4`, `63f203b`, `b760f83`, `0d55886`.
