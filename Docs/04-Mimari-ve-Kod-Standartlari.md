# 🏗️ Mimari ve Kod Standartları

> **Kullanım (AI):** Kod yazmadan veya incelemeden önce **ilgili başlığı** oku. Bu dosya "hedef mimariyi" tanımlar; mevcut kod buna adım adım yaklaştırılır (bkz. `05-Yol-Haritasi.md`). Mevcut bir dosyayı "standarda uymuyor" diye kendiliğinden yeniden yazma — sadece üzerinde çalışılan adımın kapsamında düzelt.

---

## 1. Hedef Mimari (Katmanlar)

```text
┌──────────────────────────────────────────────────────────────┐
│ GÖRÜNÜM (View)  — MonoBehaviour, ...UI / ...View            │
│  Sadece gösterir, kullanıcı girdisini olaya/komuta çevirir. │
│  İş kuralı, hesaplama, kayıt YOK.                           │
├──────────────────────────────────────────────────────────────┤
│ MANTIK (Systems) — saf C# sınıflar (MonoBehaviour değil)    │
│  TravelSystem, MarketSystem, RouteGraph, GameCalendar...    │
│  Test edilebilir. Unity sahnesine bağımlı değil.            │
├──────────────────────────────────────────────────────────────┤
│ DURUM (State)   — GameSession (çalışma anı) ↔ SaveData (JSON)│
├──────────────────────────────────────────────────────────────┤
│ TANIM (Data)    — ScriptableObject (değişmez içerik)        │
│  LocationNodeSO, TravelRouteSO, ItemDataSO, UnitDataSO...   │
└──────────────────────────────────────────────────────────────┘
İletişim: EventBus (olaylar) + Services (servis erişimi)
Bağımlılık yönü: View → Systems → State → Data   (tersi YASAK)
```

### 1.1 Boot ve Servisler
- `00_Boot` sahnesi Build Settings'te ilk sıradadır. `Bootstrapper` kalıcı servisleri **bir kez** kurar ve `MainMenuScene`'i yükler.
- Editörde herhangi bir sahneden Play'e basıldığında da servislerin kurulması için `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` güvencesi kullanılır.
- Kalıcı servisler: `AudioService` (AudioManager), `SaveService`, `GameSession`, `SceneLoader`, `ScreenFader`.
- Erişim: `Services.Get<T>()` (hafif servis bulucu). **Yeni `static Instance` Singleton açılmaz.** Mevcut Singleton'lar ilgili adımda servise taşınır.
- Sahneye özel kontrolcüler (ör. `WorldMapUI`) kalıcı değildir; ihtiyaç duyduğu servisleri `Start()`'ta alır.

### 1.2 GameSession (tek doğruluk kaynağı)
Tüm oyun durumu burada; sahneler kendi kopyasını **tutmaz**.
```text
GameSession
├── CurrentNodeId        (string, ASCII: "node_sogut")
├── DayCount             (int; tarih GameCalendar ile türetilir)
├── Caravan              (CaravanData: akçe, erzak, envanter, muhafızlar, hayvanlar)
├── ActiveTravel         (null | { RouteId, FromNodeId, ToNodeId, DaysElapsed })
├── Reputation           (FactionType → int)
├── Quests / Intel       (aktif görevler)
├── VisitedNodeIds       (keşif / fog of war)
└── Flags                (HasSeenIntro vb.)
```

### 1.3 Kayıt (Save/Load)
- Format: JSON, `Application.persistentDataPath/save_0.json`. `PlayerPrefs` **yalnızca** ayarlar (ses, dil) için kullanılır.
- `SaveData.saveVersion` zorunlu; sürüm değişince migrasyon fonksiyonu yazılır.
- Atomik yazma: önce `save_0.tmp`, sonra yeniden adlandırma (yarım kayıt bozulmasını önler).
- Otomatik kayıt noktaları: şehre/kervansaraya giriş, seyahat varışı, `OnApplicationPause(true)`.
- SO referansları kayda **ID ile** yazılır (asla obje referansı değil).
- `Dictionary` gerekiyorsa Newtonsoft JSON (KR-4 onayına bağlı); aksi halde `List<KeyValue>` ile `JsonUtility`.

### 1.4 Sahne Geçişi
- Tüm geçişler `SceneLoader.Load(SceneId)` üzerinden: fade-out → async yükleme → fade-in. Doğrudan `SceneManager.LoadScene` çağrısı yazılmaz.
- Sahne adları `SceneIds` sabitlerinde tutulur.
- Geçiş sırasında girdi kilitlenir (çift tıklama koruması).

## 2. EventBus Kuralları
- Olaylar `readonly struct : IGameEvent`, adları geçmiş zaman: `TravelStartedEvent`, `DayPassedEvent`, `LocationChangedEvent`, `CaravanStatsChangedEvent`, `GameSavedEvent`.
- Abone olan her sınıf `OnEnable`'da `Subscribe`, `OnDisable`'da `Unsubscribe` eder (veya `Start`/`OnDestroy` çifti — tutarlı olsun).
- Olaylar **durum bildirir**, komut değildir. "Bir şey oldu" → dinleyen karar verir.
- Yeni olay `Core/EventBus.cs` içindeki `#region` altına veya ilgili modülün `Events.cs` dosyasına eklenir.
- C# `event Action` sadece aynı modül içi, sahne ömrüyle sınırlı bağlantılarda kullanılabilir.

## 3. Kimlik (ID) Disiplini — KRİTİK
- Her SO'nun benzersiz, **ASCII, küçük harf, alt çizgili** ID'si vardır: `node_sogut`, `route_sogut_bilecik`, `item_ipek`, `unit_alp`.
- Görünen isim (`"Söğüt"`, `"İznik (Nikaia)"`) **yalnızca gösterim** içindir; karşılaştırma, sözlük anahtarı, kayıt, PlayerPrefs için asla kullanılmaz.
- Sahnedeki harita düğümleri `MapNodeView` bileşeni ile SO'ya **Inspector referansı** üzerinden bağlanır. `GameObject.Find`, `name.StartsWith`, `name.Contains` ile eşleştirme yasaktır.
- Türkçe metin karşılaştırması gerekirse `StringComparison.Ordinal` + ID kullan; `ToLower()` / `ToLowerInvariant()` ile Türkçe isim eşleştirme yapma (İ/ı sorunu).

## 4. Girdi (Input)
- Proje **yalnızca yeni Input System** kullanır. `UnityEngine.Input.*` (touchCount, GetTouch, GetMouseButton, GetAxis…) yasaktır.
- Çoklu dokunma (pinch): `UnityEngine.InputSystem.EnhancedTouch` (`EnhancedTouchSupport.Enable()` + `Touch.activeTouches`).
- UI etkileşimi: EventSystem + `InputSystemUIInputModule`; `IPointer*/IDrag*` arayüzleri.
- Android Geri tuşu: tek bir `BackButtonRouter` yığını (popup → çekmece → sahne) — A7'de.

## 5. UI Kuralları
- Yatay (landscape) 1920×1080 referans, `Canvas Scaler: Scale With Screen Size`, Match 0.5 (veya ekrana göre karar).
- Kritik UI `SafeAreaFitter` içinde; kenarlardan min 32 px.
- Dokunma hedefi min 48 dp (~72 px @1080p).
- Statik ve dinamik içerik ayrı Canvas'larda (sık güncellenen HUD harita arka planını yeniden çizdirmesin).
- Renkler/fontlar kodda gömülü olmaz: `UIThemeSO` (A7'ye kadar geçici olarak `02-Teknik-ve-Gorsel.md` §2 HEX değerleri sabit bir `UIColors` sınıfında toplanır).
- Liste satırları prefab olarak üretilir; `new GameObject` + `AddComponent<Image>` ile prosedürel UI yazılmaz.
- Boş durum (empty state) zorunlu: veri yoksa sahte veri değil açıklayıcı metin.
- Popup'lar ortak davranış: açılış SFX, arka plan karartma, dışına dokununca/geri tuşunda kapanma.

## 6. Ses
- Tüm ses `AudioService` üzerinden: `PlayMusic`, `PlaySFX`, `SetMasterVolume/Music/SFX`, `SetMuted`.
- Ses ayarları PlayerPrefs'e kaydedilir ve açılışta yüklenir.
- `AudioListener.pause`, sahnedeki tüm AudioSource'ları tarama (`FindObjectsByType<AudioSource>`) gibi global hack'ler yasak.
- Klipler `AudioDataSO` (AudioLibrary) üzerinden; `Resources.Load` fallback'i yazılmaz.

## 7. Kod Stili
- Namespace: `Kervan.<Modül>` — `Core`, `Audio`, `UI`, `TradeAndTravel`, `Combat`, `IntelSystem`, `DevTools`.
- Alanlar: `[SerializeField] private Type _camelCase;` · Özellikler/metotlar: `PascalCase`.
- Inspector düzeni: `[Header("Türkçe Başlık")]`, `[Tooltip("Türkçe açıklama")]`.
- Her public sınıf ve önemli metot için `/// <summary>` (Türkçe).
- Null-kontrol yerine **Inspector doğrulaması** tercih edilir: `OnValidate()` veya `Awake()`'te eksik referans için tek, açıklayıcı `Debug.LogError` (sessiz `FindAnyObjectByType` fallback zinciri yazılmaz).
- `Update()` içinde `Find*`, `GetComponent`, LINQ, string birleştirme yok.
- Sihirli sayı yok: `const` veya SO alanı.
- Debug/test kodu: `#if UNITY_EDITOR || DEVELOPMENT_BUILD` veya `Kervan.DevTools` namespace + koruma.
- Log: tutarlı ön ek `[SınıfAdı]`; A8'de `KLog` sarmalayıcısına geçilir.
- Bir dosya ~400 satırı geçiyorsa bölme önerisi sunulur (zorla bölünmez).

## 8. Klasör Yapısı (Scripts)
```text
Assets/_Project/Scripts/
├── Core/            Bootstrapper, Services, GameSession, EventBus, SceneLoader, GameEnums
│   ├── Save/        SaveData, SaveService
│   └── Editor/      Editör araçları (veri doğrulayıcı vb.) — sahne üretmez
├── Audio/
├── UI/              Genel UI bileşenleri (Fader, SafeArea, Popup, Drawer, TopBar)
│   └── Sidebar/     Sidebar parçaları
├── TradeAndTravel/  Harita, rota, seyahat, pazar
├── Combat/
├── IntelSystem/
└── DevTools/        Sadece geliştirme (korumalı)
```
Assembly Definition: şimdilik tek `Kervan.Runtime`. Ayrıştırma (`Core/Gameplay/UI/Editor/Tests`) A7'de değerlendirilir; ilk `Editor` aracı yazıldığında `Kervan.Editor.asmdef` (Editor-only) zorunludur.

## 9. Test
- EditMode testleri `Assets/_Project/Tests/EditMode/` (Unity Test Framework yüklü).
- Öncelikli: `CaravanData` (ağırlık/akçe), `RouteGraph` (komşuluk, en kısa yol), `SaveData` gidiş-dönüş (serialize → deserialize eşitliği), `DamageCalculator`.
- Saf C# sistemler test edilebilir yazılır (Unity API'ye bağımlılık minimum).

## 10. Onaylı / Önerilen Paketler
| Paket | Durum |
|---|---|
| Input System, TextMeshPro, uGUI, 2D Animation, PSD Importer, Aseprite, SpriteShape, Timeline, Test Framework, Device Simulator | ✅ Yüklü |
| Unity Localization, Addressables, Newtonsoft JSON, DOTween | ⛔ KR-4 (kullanıcı onayı gerekir) |
| Google Play Games, Firebase, UGS (Analytics/Cloud Save) | A8 sonrası değerlendirilir |
