# 🗺️ Uç Beyliği Kervanı — Profesyonel Geliştirme Yol Haritası (ROADMAP)

> **Tarihsel Arka Plan:** 14. Yüzyıl Anadolu & Rumeli | **Tür:** Taktiksel RPG & Ticaret Simülasyonu  
> **Motor:** Unity 6 (2D Isometric & Side-View) | **Hedef Platform:** Mobil (Landscape)  
> **Yöntem:** Aşama Aşama, Kontrollü, Modüler & Onay Odaklı Geliştirme

---

## 📌 Geliştirme Felsefesi ve Çalışma Kuralları
1. **Adım Adım İlerleme:** Her aşama tamamlanıp test edilmeden ve kullanıcı onayı alınmadan bir sonraki aşamaya geçilmez.
2. **Öğretici & Eşlikçi Yaklaşım:** Ne, neden ve nasıl yapıldığı detaylıca açıklanır. Kullanıcının Unity ve araçlar üzerinde pratik kazanması sağlanır.
3. **Agent Kaynak Tasarrufu:** Kodlar kontrolsüzce tek seferde yığılmaz; modüler mimariye (Core, Combat, Trade, Intel, UI) uygun küçük parçalar halinde inşa edilir.
4. **Varlık (Asset) Disiplini:** Geçici prototip çizimleri (Greyboxing/Placeholder) ile önce mekanikler doğrulanır, ardından nihai sanat varlıkları entegre edilir.

---

## 🧭 Aşama Çizelgesi (Milestones)

```
[Aşama 0: Hazırlık & Mimari]
         │
         ▼
[Aşama 1: Çekirdek Sistemler & Data Modelleri]
         │
         ▼
[Aşama 2: 2D İzometrik Altıgen Savaş Motoru (Combat Core)]
         │
         ▼
[Aşama 3: Mobil Dokunmatik Savaş Arayüzü (Combat UI/HUD)]
         │
         ▼
[Aşama 4: Harita, Rota & Kervan Seyahati (Node Travel)]
         │
         ▼
[Aşama 5: Dinamik Pazar Ekonomisi & Arbitraj Sistemi]
         │
         ▼
[Aşama 6: İstihbarat, Beylik İlişkileri & Han Dükkânı (Meta Game)]
         │
         ▼
[Aşama 7: Görsel Varlık Üretimi & Minyatür Sanat Entegrasyonu]
         │
         ▼
[Aşama 8: Ses, Müzik & Tarihsel Atmosfer (Audio Design)]
         │
         ▼
[Aşama 9: Cihaz Testleri, Performans & Mobil Yayın Paketi (Polishing & Release)]
```

---

## 📋 Detaylı Aşama Planı

### 🔹 Aşama 0: Proje Kurulumu, Unity 6 Yapılandırması & Dizin Hiyerarşisi
- [x] Unity 6 2D (URP) şablonu ile temiz proje yapısının oluşturulması.
- [x] Mobil 1920x1080 Landscape, Canvas Scaler ve Safe Area yapılandırması.
- [x] Dizin mimarisinin kurulması (`Assets/_Project/Scripts/`, `Art/`, `Audio/`, `Prefabs/`, `ScriptableObjects/`).
- [x] Kodlama ve naming standartları dokümantasyonu & Kervan.Runtime.asmdef oluşturulması.
- **Çıktı:** Boş ama tüm ayarları ve klasörleri eksiksiz yapılandırılmış çalışan bir Unity projesi. (Tamamlandı ✅)

### 🔹 Aşama 1: Veri Modelleri (ScriptableObjects) & Olay İletişim Hattı (EventBus)
- [x] Gevşek bağlı (Decoupled) `EventBus` mimarisi.
- [x] `UnitDataSO`: Can, Zırh, Stamina, Moral, Menzil, Silah tipi verileri.
- [x] `ItemDataSO`: Emtia türleri (İpek, Demir, Buğday vb.), ağırlık, taban fiyat.
- [x] `CaravanData`: Kervan kapasitesi, mevcut akçe, muhafız listesi ve erzak durumu.
- [x] Başlangıç ScriptableObject `.asset` dosyalarının üretilmesi (Alp, Okçu, Muhafız, Haydut, Emtialar).
- **Çıktı:** Kodların birbirine bağımlı olmadan haberleştiği veri temeli ve ScriptableObject kütüphanesi. (Tamamlandı ✅)

### 🔹 Aşama 2: 2D İzometrik Altıgen Savaş Motoru (Combat Core)
- [x] Axial koordinat sistemi `(q, r)` ve World-to-Hex / Hex-to-World izometrik matematik matrisi (`HexCoordinates.cs`).
- [x] Engel haritası ve altıgen karo yapısı (`HexGrid.cs` & `HexTile.cs`).
- [x] Izgara tabanlı hareket menzili hesaplaması (Breadth-First Search / BFS ile Stamina bazlı yol bulma `HexPathfinder.cs`).
- [x] Sıra Tabanlı Durum Makinesi (PlayerTurn vs EnemyTurn FSM `CombatTurnManager.cs`).
- [x] Savaş hesaplayıcısı (`DamageCalculator.cs`): İsabet oranı zarı, zırh sönümlemesi ve moral kırılma kontrolü.
- [x] Canlı savaş birimi kontrolcüsü (`CombatUnit.cs`) ve iki dokunuşlu girdi sistemi (`CombatInputHandler.cs`).
- [x] Temel Düşman Yapay Zekası (`BanditAI.cs`): En yakın hedefe yaklaşma ve vurma mantığı.
- [x] Oynanabilir savaş test sahnesi (`CombatScene.unity`) kurulumu.
- **Çıktı:** Prototip sprite'larla oynanabilir, altıgen üzerinde hareket eden ve sırayla vuran 1v1 veya 3v3 savaş prototipi. (Tamamlandı ✅)

### 🔹 Aşama 3: Mobil Ergonomik Savaş Arayüzü (Combat UI / HUD)
- [x] Sol Başparmak: Aktif Birim Kartı (Dinamik Portre, HP/Zırh barları, Stamina altıgenleri, Hilal moral ikonu `UnitHUD.cs`).
- [x] Sağ Başparmak: Eylem Komuta Kümesi (Turu Bitir, Saldır/Mod Değiştir, Siper Al, Özel Yetenek `ActionCluster.cs`).
- [x] Üst Güvenli Alan: Tur sayacı ve 1.5 sn basılı tutmalı "Geri Çekil" düğmesi (`CombatHeaderUI.cs` & `HoldButton.cs`).
- [x] İki Dokunuşlu Onay (Two-Tap Execution) ve yol/menzil önizleme çizgileri ile Hedef Odak Balonu (`CombatPredictionUI.cs`).
- [x] Mobil Çentik & Safe Area uyumlayıcısı (`SafeAreaFitter.cs`) ve Zafer/Yenilgi paneli (`CombatResultUI.cs`).
- **Çıktı:** Mobil dokunmatik hissiyatı tam, yanlış basmayı önleyen, `EKRAN.md` standartlarında savaş arayüzü. (Tamamlandı ✅)

### 🔹 Aşama 4: Harita, Rota & Kervan Seyahati (Node Travel)
- [ ] 2D Harita Düğümleri: Kervansaraylar, Şehirler, Dağ Geçitleri.
- [ ] Seyahat Motoru: Rota uzunluğu, zemin tipi (Ova, Dağ, Orman), hava şartları.
- [ ] Yol Olayları (Random Encounters): Pusular, bozuk yollar, tüccar karşılaşmaları, kervansaray konaklama masrafı.
- [ ] 2D Side-View Seyahat Ekranı: Yürüyen kervan silüeti ve gün/gece döngüsü.
- **Çıktı:** Bir şehirden diğerine erzak harcayarak seyahat edilen, pusuya düşüldüğünde savaş ekranını tetikleyen harita döngüsü.

### 🔹 Aşama 5: Dinamik Pazar Ekonomisi & Arbitraj Sistemi
- [ ] Şehir Pazar Verileri: Yerel üretim ve talep listeleri.
- [ ] Fiyat Formülü: Taban Fiyat + Mesafe Faktörü + Kıtlık/Bolluk + Doygunluk Çarpanı (Market Saturation).
- [ ] Alış-Satış Arayüzü: Parşömen tarzı envanter ve pazar listesi, kâr marjı renk göstergeleri.
- [ ] Enflasyon ve gün geçtikçe piyasanın kendini dengelemesi.
- **Çıktı:** Bursa'dan ucuz ipek alıp Konya'da kârla satabildiğimiz, aşırı satışta fiyatın düştüğü canlı bir ekonomi motoru.

### 🔹 Aşama 6: İstihbarat, Beylik İtibarı & Sabit Dükkân (Meta Game)
- [ ] Şehirlerde bilgi parçacıkları (IntelItem) toplama mekaniği.
- [ ] Beylik Nüfuzu (Osmanlı, Germiyanoğlu, Karaman, Bizans) ve itibar seviyeleri.
- [ ] Görev teslimi (Hand-in) ve ödül sistemi (Akçe, berat, özel zırh).
- [ ] Güvenli Osmanlı şehirlerinde "Han Dükkânı / Pazar Tezgâhı" açma ve pasif gelir sistemi.
- **Çıktı:** Tüccar-alp kimliğinin tam oturduğu, pasif gelir ve görev zincirlerinin çalıştığı derin meta döngü.

### 🔹 Aşama 7: Minyatür Sanat Tarzı Varlık Üretimi & Entegrasyon
- [ ] Yapay Zeka Promptları (FLUX/Midjourney) ile tarihsel minyatür tarzında 2D sprite ve UI dokuları üretimi.
- [ ] Çizim ve rötuş araçları (Photoshop / Krita / Aseprite) ile temizleme, transparan yapma ve 9-slice ayarları.
- [ ] Karakter Spritesheet'leri (256x384 px, 4 izometrik açı, Idle/Move/Attack/Hit/Down animasyonları).
- [ ] 256x128 Hex zemin karoları (Taş, toprak, çimen, çamur).
- [ ] Parşömen gürültüsü ve UI çerçevelerinin eklenmesi.
- **Çıktı:** Prototip küpler yerine `TASARİM.md` ve `BOYUTLANDİRMA.md` standartlarında büyüleyici minyatür görseller.

### 🔹 Aşama 8: Ses, Müzik & Tarihsel Atmosfer (Audio)
- [ ] Dönem enstrümanları (Kopuz, ney, kudüm, def, bağlama) ile arka plan müzik döngüleri.
- [ ] Ortam sesleri (Kervan çıngırakları, rüzgar uğultusu, han kalabalığı, kılıç-kalkan şakırtısı).
- [ ] FMOD veya Unity Audio Mixer ile dinamik ses geçişleri (Seyahat -> Pazar -> Savaş).
- **Çıktı:** Atmosferi tamamlayan tarihsel ses tasarımı.

### 🔹 Aşama 9: Optimizasyon, Kayıt Sistemi & Mobil Paket (Release)
- [ ] JSON / Binary tabanlı sağlam Kayıt & Yükleme (Save/Load) mimarisi.
- [ ] Mobil bellek (Texture Compression: ASTC, Sprite Atlas) ve batarya optimizasyonu.
- [ ] Android APK / AAB paketleme ve dokunmatik ekran son testleri.
- **Çıktı:** Mobil cihaza yüklenip baştan sona oynanabilen nihai sürüm.

---

## 🚦 Mevcut Durum: **Aşama 0'a Başlamaya Hazır**
Herhangi bir koda veya dosya değişikliğine geçmeden önce bu yol haritası ve geliştirme ilkeleri doğrultusunda onayınız beklenmektedir.
