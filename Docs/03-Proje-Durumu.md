# 🗺️ Uç Beyliği Kervanı — Profesyonel Geliştirme Yol Haritası (ROADMAP)

> **Tarihsel Arka Plan:** 14. Yüzyıl Anadolu & Rumeli | **Tür:** Taktiksel RPG & Ticaret Simülasyonu  
> **Motor:** Unity 6 (2D Isometric & Side-View) | **Hedef Platform:** Mobil (Landscape)  
> **Yöntem:** Aşama Aşama, Kontrollü, Modüler & Onay Odaklı Geliştirme  
> **Son Güncelleme:** 12.10.2026 (Ana Menü UI Tamamlandı)

---

## 📌 GERÇEK DURUM ANALİZİ

- **Aşama 0 (Mimari & Veri Tablosu):** ✅ Tamamlandı (ScriptableObject yapıları, savaş matematiği, pazar ve envanter omurgası).
- **Aşama 1 (Ana Menü & UI Temelleri):** ✅ **TAMAMLANDI.**
  - `UI_BG_MainMenu` zemin üzerine `UI_Logo_GameTitle` ve ana eylem butonları (Yeni Oyun, Devam Et) yerleştirildi.
  - Alt orta login seçenekleri (Google, Apple, Mail) eklendi.
  - Sağ üst açılır yardımcı çekmece (`UtilityDrawer`) kapalı başlangıç ve okla açılma mantığıyla sorunsuz çalışıyor.
  - Ayarlar penceresi (`Popup_Settings`), `UI_Frame_Market` üzerinde `SFX_PaperRustle.wav` açılış sesi, canlı ses ayar slider'ları ve `Icon_Util_Close.png` kapatma butonu ile aktif.
  - Fon müziği (`BGM_Main.mp3`) ve buton sesleri (`SFX_ButtonClick.wav`) AudioManager üzerinden bağlandı.
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

### 💡 3. Ana Menü İçin Masadaki Bekleyen Cila Önerileri (Opsiyonel):
- **Sürüm Numarası:** Sol alt köşeye `v0.1.0 Alpha — Uç Beyliği Kervanı` etiketi.
- **Çıkış Onay Kutusu (Quit Modal):** Çıkış butonuna basıldığında kazara kapanmayı önleyen parşömen onay penceresi.
- **E-Posta Form Popup'ı:** Mail butonuna basıldığında E-posta/Şifre giriş kutusu.
- **Sahneler Arası Yumuşak Kararma (Screen Fade In/Out):** Yeni Oyun'a basıldığında kararak geçiş.
- **Kayıt Kontrolü:** Kayıtlı oyun yoksa "Devam Et" butonunun inaktif (disabled) olması.

---

## 📋 Sıradaki Ana Görev: Aşama 2 (Dünya Haritası ve Seyahat)
- [ ] `WorldMapScene.unity` sahnesinde `UI_BG_WorldMap.png` haritasının kurulması.
- [ ] Şehir ve Kervansaray/Han düğümlerinin (`Icon_MapNodeCity`, `Icon_MapNodeHan`) koordinatlara yerleştirilmesi.
- [ ] Rota çizgilerinin (Segments) ScriptableObject verileriyle eşleştirilmesi.
- [ ] Seyahat HUD'ı (Akçe, Erzak, Gün sayacı ve Kervan ilerleme çubuğu) panellerinin Prefab olarak üretilmesi.
