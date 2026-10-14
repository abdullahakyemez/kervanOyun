# 🗺️ Uç Beyliği Kervanı — Profesyonel Geliştirme Yol Haritası (ROADMAP)

> **Tarihsel Arka Plan:** 14. Yüzyıl Anadolu & Rumeli | **Tür:** Taktiksel RPG & Ticaret Simülasyonu  
> **Motor:** Unity 6 (2D Isometric & Side-View) | **Hedef Platform:** Mobil (Landscape)  
> **Yöntem:** Aşama Aşama, Kontrollü, Modüler & Onay Odaklı Geliştirme  
> **Son Güncelleme:** 14.10.2026 (Dünya Haritası, SFX & Arayüz Cilaları Tamamlandı)

---

## 📌 GERÇEK DURUM ANALİZİ

- **Aşama 0 (Mimari & Veri Tablosu):** ✅ Tamamlandı (ScriptableObject yapıları, savaş matematiği, pazar ve envanter omurgası).
- **Aşama 1 (Ana Menü & UI Temelleri):** ✅ **TAMAMLANDI + CİLALANDI.**
  - `UI_BG_MainMenu` zemin üzerine `UI_Logo_GameTitle` ve ana eylem butonları (Yeni Oyun, Devam Et) yerleştirildi.
  - Alt orta login seçenekleri (Google, Apple, Mail) eklendi.
  - Sağ üst açılır yardımcı çekmece (`UtilityDrawer`) kapalı başlangıç ve okla açılma mantığıyla sorunsuz çalışıyor.
  - Ayarlar penceresi (`Popup_Settings`), `UI_Frame_Market` üzerinde `SFX_PaperRustle.wav` açılış sesi, canlı ses ayar slider'ları ve `Icon_Util_Close.png` kapatma butonu ile aktif.
  - Fon müziği (`BGM_Main.mp3`) ve buton sesleri (`SFX_ButtonClick.wav`) AudioManager üzerinden bağlandı.
  - **Eklenen Cilalar & Mobil Koruma:**
    - `ScreenFader.cs`: Açılışta aydınlanma, butonlara tıklandığında yumuşak kararma (Fade to Scene) sistemi kuruldu.
    - `QuitConfirmPopupUI.cs`: Kazara oyundan çıkmayı önleyen parşömen onay penceresi ("Oyundan çıkmak istiyor musunuz?") hazırlandı.
    - `SafeAreaFitter.cs`: Mobil cihazlarda çentik (Notch) ve kör alanlara karşı güvenli kenetleme sistemi entegre edildi.
    - Sürüm etiketi (`v0.1.0 Alpha — Uç Beyliği Kervanı`) ve kayıt durumu kontrolü (Kayıt yoksa "Devam Et" butonunu devre dışı bırakma) bağlandı.
    - Settings ve pencerelerde kullanılacak standart ahşap buton zemini `UI_Btn_Standard.png` olarak merkezi kütüphaneye (`Assets/_Project/Art/Sprites/UI/Buttons/`) dahil edildi.
  - Tüm varlıklar merkezi ve kurumsal klasör yapısına taşındı (`Assets/_Project/Art/...` ve `Audio/...`).
- **Görsel Varlık Standardı:** `Docs/02-Teknik-ve-Gorsel.md` Madde 7 altına kalıcı anayasa olarak eklendi.

---

## 🧭 Aşama Çizelgesi (Milestones)

```text
[Aşama 0: Hazırlık, Mimari & Veri Tablosu (ScriptableObjects)] ✅
         │
[Aşama 1: Ana Menü & UI (Kullanıcı Arayüzü) Temellerinin Atılması] ✅
         │
[Aşama 2: Dünya Haritası, Rota ve Seyahat Entegrasyonu] ⏳ (Sıradaki Aşama)
         │
[Aşama 3: Kervan, Envanter ve Pazar (Ticaret) Entegrasyonu] ❌
         │
[Aşama 4: Savaş Alanı (Combat) ve Birim Prefab'larının Kurulması] ❌
         │
[Aşama 5: Pusu (Encounter) Sistemi ve Meta Oyun (Görevler/İtibar)] ❌
         │
[Aşama 6: Kayıt & Yükleme (Save/Load) Sistemi ve Optimizasyon] ❌
         │
[Aşama 7: Android Mobil Çıkış (Release) Paketi ve Testler] ❌
```

---

## 📌 BİR SONRAKİ OTURUM İÇİN KRİTİK HATIRLATICILAR & NOTLAR

### 📱 1. Mobil Ekran Uyumluluğu & Kör Alan (Safe Area / Notch) Testi
- **Soru / Test İhtiyacı:** Oyunun ekran özellikleri stabil mi? Telefonların hepsinde yatay (Landscape) açıldığında kör alan (çentik, kamera deliği, home bar) veya UI taşması/bozulması oluyor mu?
- **Yapılacak İşlem:** Unity Game penceresinde farklı ekran oranları (`16:9`, `18:9`, `19.5:9`, `20:9 Device Simulator`) seçilerek test edilecek; gerekirse `SafeAreaFitter.cs` bileşeni ile sol ve sağ kenarlara güvenli pay bırakılacak.

### 🔄 2. Esnek Tasarım ve Sahne Düzenleme İlkesi
- **Kural:** Kullanıcı her aşamada ve herhangi bir sahnede (önceden tamamlanmış kabul edilse dahi) istediği zaman değişiklik, revizyon veya ekleme yapabilir. Sistem asla "bu aşama bitti, geri dönemeyiz" demeyecek, esnek kalacaktır.

### 💡 3. Ana Menü Cila ve Koruma Sistemleri Durumu (Tamamlandı):
- [x] **Sürüm Numarası:** `MainMenuUI` üzerinden sol alt köşeye `v0.1.0 Alpha — Uç Beyliği Kervanı` etiketi bağlandı.
- [x] **Çıkış Onay Kutusu (Quit Modal):** `QuitConfirmPopupUI.cs` ile kazara kapanmayı önleyen parşömen onay penceresi ("Oyundan çıkmak istiyor musunuz?") kodlandı.
- [x] **Sahneler Arası Yumuşak Kararma (Screen Fade In/Out):** `ScreenFader.cs` ile siyah CanvasGroup üzerinden yumuşak geçiş hazırlandı.
- [x] **Kayıt Kontrolü:** `PlayerPrefs` tabanlı kayıt kontrolü eklendi; kayıt yoksa "Devam Et" butonu inaktif kalır.
- [x] **Mobil Safe Area (Kör Alan / Çentik):** `SafeAreaFitter.cs` hazırlandı.

### 🎯 4. Bir Sonraki Oturum Açılışında Hatırlatılacak Yol Haritası Önerileri:
Kullanıcının tercihine sunulan ve yeni oturum başlar başlamaz masaya yatırılacak 3 seçenek:
1. **Seçenek 1 (Tavsiye Edilen - Aşama 2 Kapanışı): `TravelScene` (Seyahat Sahnesi) Entegrasyonu & Akışı**
   - Haritada "Seyahat Et" butonuna tıklandığında `TravelScene` sahnesine geçiş, 2D yandan kervan yürüyüşü, gün/erzak tüketimi, yol olayları (`EventModal`), pusu uyarısı (`AmbushPanel`) ve hedef şehre varışta harita konumunun güncellenmesi döngüsünün tamamlanması.
2. **Seçenek 2: Harita Üst Kaynak Çubuğu (TopBar HUD) Görsel Yenilemesi**
   - Projeye yeni eklenen `UI_ToolbarBG.png`, `toolbarAkceIcon.png` ve `toolbarErzakIcon.png` varlıklarıyla haritanın üst kısmındaki Akçe, Erzak ve Tarih göstergelerinin parşömen/ahşap çerçeveye oturtulması.
3. **Seçenek 3 (Aşama 3 Başlangıcı): Şehir İçi ve Pazar Arayüzü (`TownHubScene` & `MarketScene`)**
   - Haritada bulunulan merkezde "Şehre Gir" dendiğinde açılan pazar ekranında emtia alım-satımı, kâr arbitrajı, kervansaray ve divan etkileşimleri.

---

## 📋 Aşama 2: Dünya Haritası ve Seyahat Durumu (İlerliyor ⏳)
- [x] **Varlık Organizasyonu (Emtialar & Portreler):**
  - Tüm ticaret emtiaları `Assets/_Project/Art/Sprites/Items/` klasöründe toplandı.
  - Tüm profil kartları (`Mauhafiz_ProfileCard`, `Okcu_ProfileCard`, `Ahi_ProfileCard`, `Horse_ProfileCard`, `Mule_ProfileCard`, `Camel_ProfileCard`, `Erzak_ProfileCard`) ve UI parçaları `Assets/_Project/Art/Sprites/UI/Portraits/`, `Backgrounds/`, `Buttons/`, `Decorations/` altında standardize edildi.
- [x] **Harita Pan & Zoom:** `MapPanZoomController.cs` (Pinch zoom, drag pan, sınır kilitleme ve Söğüt düğümüne hatasız odaklanma) kodlandı ve doğrulandı.
- [x] **Kervan & Yük Yan Çekmecesi:** `InventorySidebarUI.cs` güncellendi:
  - Canlı Mobil Safe Area (Çentik / Notch) koruması entegre edildi (`ApplySafeArea()` ile Device Simulator ve farklı telefon modellerinde kör alandan kaçınma).
  - Satır taşma ve hizalama sorunları çözüldü (İster eski prefab kullanılsın ister prosedürel üretilsin, konteynere %100 oturan responsive satır motoru).
  - Erzak bilgisi, planlanan TopBar üzerinde yer alacağı için Kervan sekmesinden kaldırıldı.
  - Muhafız, Okçu ve Alp için doğru profil portreleri (`Mauhafiz_ProfileCard`, `Okcu_ProfileCard`, `Ahi_ProfileCard`) otomatik bağlandı.
- [x] **Dünya Haritası Arayüzü & Mizanpaj:** `WorldMapUI.cs`, `BottomNodeInfoCardUI.cs` ve `CurrentNodeMarkerUI.cs` tamamlandı:
  - Sol alt Kervan & Yük butonu entegrasyonu sağlandı.
  - Sağ üst açılır yardımcı çekmece (Utility Drawer: Ayarlar, Ses açma/kapama, Ana Menüye dönüş) entegre edildi.
  - Harita fon müziği (`BGM_Travel.mp3`) çakışması ve ses açma/kapama (Mute Toggle) kontrolü `AudioListener.volume` / `AudioListener.pause` ile global düzeyde güvenceye alındı; kameradaki çift AudioSource durduruldu.
  - Aktif düğüm (Söğüt / Oyuncunun konumu) `CurrentNodeMarkerUI.cs` ile altın ışıltılı nabız halkası, süzülen konum pini (`Icon_Location` / `Konumicon`) ve tıklamaları engellemeyen (`raycastTarget = false`) koruma ile yenilendi.
  - Koyu bordo/siyah sidebar arka planına (`UI_SidebarScrollBG.png`) uygun parşömen kremi (`#F5E6D3`) ve antik altın (`#D9B359`) tipografi renkleri entegre edildi, Viewport tam esnetildi; satır ayraçları orantılı ve zarif `UI_BG_ayirac_2` Selçuklu rozet motifi ile ortalandı.
  - Şehir ve hanlara tıklandığında alttan yumuşakça kayarak açılan detaylı bilgi kartı (`BottomNodeInfoCardUI.cs`); Konum, Tehlike, Pazar ve Erzak rozet kutuları için taşmayı önleyen kompakt metin formatı, TMP Truncate modu ve şehre özel ticaret malları özeti (`GetMarketGoodsSummary`) ile standardize edildi.
  - MainMenu -> WorldMap geçişinde donma ve takılmaları önleyen zaman aşımı emniyeti (Fallback Timeout) eklendi.
  - Ham `.ai` ve `.jpg` tasarım dosyaları `Assets/_Project/Art/SourceFiles/` altına arşivlendi, mükerrer Türkçe karakterli dosya (`Unit_Muhafız.png`) temizlendi.
- [x] **Ses & Arayüz Efektleri (SFX Entegrasyonu & Cila):**
  - Uzun ve hantal `SFX_Parchment.wav` dosyası projeden tamamen temizlendi.
  - Alttan açılan şehir bilgi kartı (`BottomNodeInfoCardUI`) ve sol alttan açılan Kervan & Yük çekmecesine (`InventorySidebarUI`) tok ve doğal parşömen açılış sesi (`SFX_PaperRustle.wav`) bağlandı.
  - `UIButtonSound.cs` bileşeni özel klip (`CustomClickClip`), ses şiddeti ve perde varyasyonuyla güçlendirildi; `WorldMapUI` üzerindeki dinleyici silme hatası giderildi ve `AudioManager` kendini onaran (Self-Healing) yapıya kavuşturuldu.
- [ ] Unity `WorldMapScene.unity` sahnesinde Device Simulator testlerinin tamamlanması.
