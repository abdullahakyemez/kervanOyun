# **Oyun Tasarım Belgesi (GDD): Uç Beyliği Kervanı (14. Yüzyıl Anadolu \& Rumeli)**

## 

## 

#### **1. Yönetici Özeti \& Çekirdek Kimlik**



* **Proje Türü:** Tarihi Taktiksel RPG / Kervan \& Ticaret Simülasyonu.
* **Tarihsel Arka Plan:** 14. Yüzyıl; Osmanlı Beyliği'nin kuruluşu, Anadolu Türk Beylikleri, Doğu Roma (Bizans) ve Trabzon Rum İmparatorluğu coğrafyası.
* **Karakter Profili:** Eski Uç Alpi, günümüz tüccarı. Kılıç, yay ve binicilikte usta; Osmanlı idaresine gizli istihbarat ağı ile bağlı kıdemli bir gazi-tüccar (35–45 yaş bandı).
* **Platform \& Ekran Oryantasyonu:** Mobil, **Landscape (Yatay)**.
* **Görsel \& Projeksiyon Ayrımı:**

  * *Seyahat, Karar Ekranları, Olaylar ve Yükleme:* **2D Side-View** (Yandan minyatür/illüstrasyon estetiği).
  * *Pazar, Kervansaray Etkileşimleri ve Altıgen Savaş:* **2D İzometrik Projeksiyon.**



#### **2. Sistem Döngüleri (Loops)**



###### **A. Core Gameplay Loop (Sefer \& Savaş)**

Rota Seçimi ---> Yol Koşulları \& Erzak Tüketimi ---> Olay/Pusu Karşılaşması (Altıgen Izgarada Takım Bazlı Taktik Çatışma) ---> Kayıp/Yaralanma Çözümlemesi ---> Kervansaray/Şehir Girişi.

###### **B. Meta Game Loop (Ekonomi, Casusluk \& Genişleme)**

Bölgesel Emtia Ticareti (Arz-Talep Arbitrajı) ---> İstihbarat Toplama \& İdarecilere İletme---> Siyasi Nüfuz \& Sermaye Birikimi ---> Osmanlı Mülkünde Sabit Pazar/Dükkân Açma (Pasif Gelir) ---> Daha Büyük Kervan \& Ağır Teçhizatlı Muhafızlar.



#### **3. Coğrafya, Düğümler ve Rota Sistemi**



Oyun haritası düğüm (Node-based) yapısında çalışır.



###### **Düğüm Türleri**

1. **Kervansaraylar:**

   1. Dinlenme ve gün atlatma merkezleridir. Oyuncu dilediği gün kadar kalabilir.
   2. Gün başına sabit konaklama masrafı ödenir; karşılığında **Moral** ve **Yaralı İyileşmesi** hızlanır.
   3. Temel erzak ve yerel muhafız ikmali yapılır.
2. **Şehirler (Osmanlı, Beylikler, Doğu Roma, Pontus):**

   1. Pazar etkileşimi, idareci/tekfur/bey görüşmeleri, istihbarat teslimatı ve casusluk faaliyetleri yapılır.
   2. Şehrin bağlı olduğu siyasi güçle olan ilişki (İtibar/Reputation) pazar vergilerini ve güvenliği doğrudan etkiler.

### 

###### **Rota Değişkenleri \& Fiziksel Şartlar**

Her rota iki düğüm arasında aşağıdaki teknik değişkenleri taşır:

* TerrainType (Dağlık Geçit, Orman, Ova, Bataklık): Hareket hızı ve erzak harcama katsayısını belirler.
* WeatherCondition (Kar Fırtınası, Aşırı Sıcak, Sağanak): Kondisyon ve dayanıklılık (Stamina) cezası verir.
* DangerLevel (0.0 - 1.0): Pusuya düşme olasılığını ve düşman kademesini belirler.
* RoadCondition (Bozuk Yol, Taş Döşeli Roma Yolu): Araba aksı kırılması, hayvan sakatlanması gibi lojistik risk olaylarını tetikler.

## 

## 

#### **4. Dinamik Pazar Ekonomisi \& Pasif Gelir**

##### 

##### **Dinamik Arz-Talep \& Arbitraj Sistemi**

* Her şehrin yerel üretim listesi (LocalGoods) ve ihtiyaç listesi (DemandedGoods) bulunur:

  * *Örnek:* İpek/Dokuma Doğu Roma ve Bursa'da bol ve ucuzken, iç Anadolu beyliklerinde yüksek kâr marjıyla satılır. Demir/Silah veya Tahıl ise kıtlık bölgelerinde katlanarak değer kazanır.
* **Enflasyon \& Doygunluk Çarpanı:** Aynı şehre tek seferde aşırı miktarda aynı mal satılırsa yerel fiyat düşer (MarketSaturation). Şehrin malı tüketmesi için birkaç oyun günü geçmesi gerekir.

##### **Sabit Pazar / Han Dükkânı (End-Game / Pasif Gelir)**

* Osmanlı kontrolündeki güvenli şehirlerde (Bursa, Söğüt, İznik vb.) belirli bir sermaye ve beylik itibarına ulaşıldığında pazar tezgâhı/dükkân açılabilir.
* Dükkâna bırakılan mallar ve tahsis edilen çırak/muhafız sayesinde oyuncu seferdeyken arka planda pasif akçe ve bölgesel ticaret istihbaratı üretilir.



## 

#### **5. Çift Yönlü İstihbarat \& Casusluk Sistemi**



Karakterin eski alp kimliği, tüccar kamuflajıyla birleşir:

* **İstihbarat Toplama (Gathering):** Yabancı şehirlerin pazarlarında, meyhanelerinde veya idarecilerle görüşmelerde pasif/aktif bilgi parçacıkları (IntelItem) toplanır (Askeri yığınak bilgisi, kıtlık haberi, yerel isyan planı, tekfurun zayıflığı).
* **Teslimat \& Ödül (Hand-in):** Toplanan veriler Osmanlı uç beylerine ulaştırıldığında:

  * Yüksek miktarda **Akçe**,
  * Özel beylik zırhı/silahları,
  * Kervansaraylarda bedava konaklama veya resmi geçiş beratı (vergi muafiyeti) gibi stratejik avantajlar kazanılır.
* **Risk:** Başarısız casusluk veya düşman şehirde yakalanma; kervanın tüm mallarına el konulmasına veya zindandan kılıçla kaçış çatışmasına yol açar.



#### **6. Altıgen Taktik Savaş Sistemi (Hex Combat)**



###### **Savaş Alanı \& Giriş**

Pusuya düşüldüğünde veya avlanma kararı alındığında ekran 2D İzometrik Altıgen Tahtaya (HexGrid) geçer. Karşılaşma türleri:

* **Haydut Pususu:** Kervan ortada savunmada başlar; haydutlar çevreden saldırır.
* **Yabani Hayvan Saldırısı (Kurt sürüsü, ayı):** Düşük zırh, yüksek hareket hızı ve moral kırma odaklı saldırgan sürü davranışı.

###### **Birim Sınıfları (Units)**

1. **Kılıç/Kalkan Birimi (Piyade \& Alp):** Yüksek zırh (Armor), dar alanda alan savunması (Zone of Control), ön safta darbe emici rol.
2. **Okçu Birimi (Gazi / Yabancı Paralı Asker):** Uzun menzil saldırı, düşük zırh; yüksek tepelerde isabet bonusu, yakın dövüşe girince ağır hasar dezavantajı.
3. **Karakter (Tüccar Alp):** Hibrit birim; hem yay hem kılıç kullanabilir. Yüksek moral aurası ile çevresindeki muhafızların paniklemesini önler.

###### **Çatışma Algoritması**

* **Takım Bazlı Tur:** Oyuncu Fazı $\\rightarrow$ Düşman Fazı.
* **Kaynaklar:**

  * Stamina (Dayanıklılık): Hex hareketi ve vuruşlar stamina harcar.
  * Morale: Can azaldıkça veya yolda aç kalındıkça düşer; moral kritik seviyenin altına inerse muhafızlar kontrol dışı kaçar (Routing).
  * RNG \& Zar: İsabet oranı mesafe, çevre engelleri (ağaç, kaya) ve moral ile dinamik hesaplanır.
* **Kalıcı Yaralanma:** Ölen muhafızlar kervandan eksilir. Karakter canı sıfırlanırsa savaş kaybedilir; esir düşme, fidye ödeme ve kervan yükünü kaybetme mekanikleri tetiklenir.





#### **7. C# / Unity İçin Veri ve Kod Mimarisi**



**Yapay zekaya kod yazdırırken modülerliği sağlayacak teknik omurga:**

**\[Scripts]**

**├── Core**

**│   ├── GameManager.cs              // Oyun durumları (Travel, Market, Combat, CaravanRest)**

**│   ├── TimeManager.cs               // Gün atlatma, kervansaray bekleme süreleri**

**│   └── EventBus.cs                 // Sistemler arası gevşek bağlı (decoupled) iletişim**

**├── Combat**

**│   ├── HexGridController.cs        // Axial (q, r) koordinatları, hücre engel haritası**

**│   ├── TeamCombatManager.cs        // PlayerTurn vs EnemyTurn durum makinesi (FSM)**

**│   ├── CombatUnit.cs               // Health, Armor, Stamina, Morale bileşenleri**

**│   └── DamageCalculator.cs         // RNG, isabet ve zırh sönümleme matematiği**

**├── TradeAndTravel**

**│   ├── CaravanData.cs              // Yük, erzak, akçe, muhafız listesi**

**│   ├── CityMarket.cs               // Arz, talep, enflasyon ve alış-satış algoritmaları**

**│   └── TravelRouteEngine.cs        // Rota maliyeti, tehlike zarı, yol şartı kontrolleri**

**└── IntelSystem**

`    `**├── IntelDataSO.cs              // İstihbarat tipi, hedef bölge, teslim ödülü**

`    `**└── QuestManager.cs             // Osmanlı beylik görev takipçisi**

---

#### **8. Tarihsel Düğümler & Coğrafi Harita Ağı**
* **Osmanlı Uç Bölgesi:** Söğüt (Ana Üs/Merkez), Domaniç (Yayla/Moral Kampı), Bilecik (Demir/Zırh Merkezi), Karacahisar (İlk Pazar), İznik (Kültür/Çini), Bursa (İpek Yolu Metropolü).
* **Bizans & Rum Sınırı:** Nikaia (İznik Bizans kalıntıları), Prusa (Bursa Surları), Konstantinopolis (Elit Mallar/Casusluk Yuvası), Trabzon (Karadeniz Deniz Ticareti & Pontus Sarayı).
* **Anadolu Beylikleri:** Germiyanoğulları (Kütahya - At/Deri Ticareti), Karamanoğulları (Konya - Selçuklu Başkenti, Tahıl/Miras).

#### **9. Ses ve Müzik Mimarisi (Audio Design)**
* **Müzik (Tarihsel Çalgılar):** Kopuz, Dombra, Ney, Kudüm, Def ve Çeng.
  * *Seyahat Teması:* Durgun, rüzgar uğultulu ve hafif kopuz ezgileri.
  * *Pazar/Kervansaray:* Canlı, uğultulu esnaf sesleri, ney ve kudüm ritimleri.
  * *Savaş Teması:* Savaş davulları (kös), sert yaylılar ve gerilimli perküsyon.
* **Ses Efektleri (SFX):**
  * Kervan adım sesleri, at/deve kişnemeleri, çıngırak tınıları.
  * Kılıç sıyrılması, kalkan bloklama, ok vınlaması, zırh delinme sesleri.
  * UI: Taş oyması tıklaması, parşömen hışırtısı, akçe şıngırtısı.

#### **10. Veri Kayıt Mimarisi (Save/Load & State Persistence)**
* **Format:** Güvenli, yerel JSON tabanlı `SaveData` yapısı.
* **Kayıt Noktaları:** Şehir ve kervansaray girişlerinde otomatik kayıt (Auto-save); savaş ortasında durum dondurma (Suspend-save).
* **Saklanan Veriler:** Kervan envanteri, akçe, muhafızların sağlık/moral durumu, şehirlerdeki piyasa doygunluk çarpanları, beylik itibar puanları ve aktif istihbarat görevleri.


