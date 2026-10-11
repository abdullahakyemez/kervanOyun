# **Oyun Tasarım Belgesi (GDD): Uç Beyliği Kervanı (14. Yüzyıl Anadolu \& Rumeli)**

## 

## 

#### **1. Yönetici Özeti \& Çekirdek Kimlik**



* **Proje Türü:** Tarihi Taktiksel RPG / Kervan - Ticaret Simülasyonu.
* **Tarihsel Arka Plan:** 14. Yüzyıl; Osmanlı Beyliği'nin kuruluşu, Anadolu Türk Beylikleri, Doğu Roma (Bizans) ve Trabzon Rum İmparatorluğu coğrafyası.
* **Karakter Profili:** Eski Uc Alpi, günümüz tüccarı. Kılıç, yay ve binicilikte usta; Osmanlı idaresine gizli istihbarat ağı ile bağlı kıdemli bir gazi-tüccar (35–45 yaş bandı).
* **Platform \& Ekran Oryantasyonu:** Mobil, **Landscape (Yatay)**.
* **Görsel \& Projeksiyon Ayrımı:**

  * *Seyahat, Karar Ekranları, Olaylar ve Yükleme, Pazar ve Kervan Etkileşimleri:* **2D Side-View** (Yandan minyatür/illüstrasyon estetiği).
  * *Altıgen Savaş:* **2D İzometrik Projeksiyon.**



#### **2. Sistem Döngüleri (Loops)**



###### **A. Core Gameplay Loop (Sefer & Savaş)**

Rota Seçimi ---> Yol Koşulları & Erzak Tüketimi ---> Olay/Pusu & Yabani Hayvan(Ayı-Kurt) Karşılaşması (Altıgen Izgarada Takım Bazlı Taktik Çatışma) ---> Kayıp/Yaralanma Çözümlemesi ---> Kervansaray/Şehir Girişi.

###### **B. Meta Game Loop (Ekonomi, Casusluk \& Genişleme)**

Bölgesel Emtia Ticareti (Arz-Talep Arbitrajı) ---> İstihbarat Toplama & İdarecilere İletme---> Siyasi Nüfuz & Sermaye Birikimi ---> Osmanlı Mülkünde Sabit Pazar/Dükkân Açma (Pasif Gelir) ---> Daha Büyük Kervan & Ağır Teçhizatlı Muhafızlar.



#### **3. Coğrafya, Düğümler ve Rota Sistemi**



Oyun haritası düğüm (Node-based) yapısında çalışır.



###### **Düğüm Türleri**

1. **Kervansaraylar:**

   1. Dinlenme ve gün atlatma merkezleridir. Oyuncu dilediği gün kadar kalabilir.
   2. Gün başına sabit konaklama masrafı ödenir; karşılığında **Moral** ve **Yaralı İyileşmesi** hızlanır.
   3. Temel erzak ve yerel muhafız ikmali yapılır.
   4. Bazen çeşitli duyumlar alınabilir, Kervandan çıkıldığında istihbarat olarak görev kısmına kullanıcı kabul ederse eklenebilir.
2. **Şehirler (Osmanlı, Beylikler, Doğu Roma, Pontus):**

   1. Pazar etkileşimi, idareci/tekfur/bey görüşmeleri, istihbarat teslimatı ve casusluk faaliyetleri yapılır.
   2. Şehrin bağlı olduğu siyasi güçle olan ilişki (İtibar/Reputation) pazar vergilerini ve güvenliği doğrudan etkiler.
 

### 

###### **Rota Değişkenleri & Fiziksel Şartlar**

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

#### **5. Çift Yönlü İstihbarat & Casusluk Sistemi**



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

* **Takım Bazlı Tur:** Oyuncu Fazı ---> Düşman Fazı.
* **Kaynaklar:**

  * Stamina (Dayanıklılık): Hex hareketi ve vuruşlar stamina harcar.
  * Morale: Can azaldıkça veya yolda aç kalındıkça düşer; moral kritik seviyenin altına inerse muhafızlar kontrol dışı kaçar (Routing).
  * RNG \& Zar: İsabet oranı mesafe, çevre engelleri (ağaç, kaya) ve moral ile dinamik hesaplanır.
* **Kalıcı Yaralanma:** Ölen muhafızlar kervandan eksilir. Karakter canı sıfırlanırsa savaş kaybedilir; fidye ödeme ve kervan yükünü kaybetme mekanikleri tetiklenir.





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


### **1. Yatay Ekran (Landscape) Ergonomi ve Bölge Şeması**





###### Aşağıdaki şema, 1920x1080 yatay ekranda başparmak erişilebilirlik haritasını ve fonksiyonel panellerin dağılımını gösterir:



##### +---------------------------------------------------------------------------------------------------------------------------------------------------+

##### | \[Kayıt/Ayarlar]           			 \[ TUR / TUR SAYACI \& ÇATIŞMA ETİKETİ ]       					  \[Geri Çekil]  |

##### | (Sol Üst - Pasif)                  				 (Üst Orta - Pasif Başlık)            					    (Sağ Üst - Güvenli)  |

##### |                                                                                  																|

##### |                                                                                   																|

##### |                                                                                   																|

##### |                                                                                   																|

##### |                                                                                   																|

##### |                           					\[ ALTIĞEN SAVAŞ ALANI (HEX GRID) ]                      							|

##### |                                						(Dokun \& Seç Alanı)                                								|

##### |                                                                                   																|

##### |                                                                                   																|

##### |                                                                                   																|

##### |                                                                                   																|

##### | +-------------------------+                               												     +---------------------+ |

##### | |  AKTİF BİRİM KARTI      |                               										|  EYLEM DÜĞMELERİ    | |

##### | |  - Portre \& Sınıf Adı   |                               											|  	\[ Saldır / Yay ]   	     | |

##### | |  - HP / Zırh / Stamina  |                               										|  \[ Savunma / Siper ]      | |

##### | |  - Moral Göstergesi     |                               										|  ----------------------------  | |

##### | +-------------------------+                               											|  	\[ TURU BİTİR ]           | |

##### | (Sol Başparmak Bölgesi)                                   										| (Sağ Başparmak - Ana)| |

##### +---------------------------------------------------------------------------------------------------------------------------------------------------+





### **2. Arayüz Panelleri ve Yerleşim Özellikleri**



###### &#x09;	A. Sol Başparmak Bölgesi: Aktif Birim Bilgi Kartı (Unit HUD)

&#x09;		Sol elin alt köşesi, seçili askerin durumunu gözü yormadan takip etmek için ayrılmıştır.

&#x09;			- Konum: Sol alt köşe (Anchor: Bottom-Left, Offset: (32px, 32px)).

&#x09;			- Boyut: 420 X 180 px.

&#x09;			- İçerik Bileşenleri:

&#x09;					Portre Çerçevesi: 110 X 110px oval ahşap/bronz çerçeve. Can durumuna göre hafif kan lekeli hasar overlay'i.

&#x09;					Can (HP) Barı: Genişlik 260 px, Yükseklik 18px. Dolgu rengi koyu al/kırmızı (#8B261E).

&#x09;					Zırh (Armor) Barı: Genişlik 260px, Yükseklik 12 px. Hemen HP'nin altında; çelik gri/mavi (#4A4E51).

&#x09;					Dayanıklılık (Stamina) Puanları: 5 adet küçük altıgen veya damla şeklinde sarı/kehribar hücre göstergesi (#DAA520). Her hareket ve saldırıda birer birer söner.

&#x09;					Moral Durumu: Portrenin sağ üstünde hilal simgesi (Yüksek: Yeşil #2A4B37, Normal: Beyaz, Panik: Kırmızı titreyen kırık yay).



###### &#x09;	B. Sağ Başparmak Bölgesi: Eylem Komuta Kümesi (Action Command Cluster)

&#x09;		En sık kullanılan ve kritik kararların verildiği alandır. Yanlışlıkla basmayı önleyecek hiyerarşik büyüklükte tasarlanır.

&#x09;			- Konum: Sağ alt köşe (Anchor: Bottom-Right, Offset: (-32px, 32px)).

&#x09;			- Birincil Düğme — "Turu Bitir" (End Turn):

&#x09;				- Boyut: 180 x 64 px (Geniş basma alanı).

&#x09;				- Tasarım: Kabartmalı Selçuklu taş bordürlü buton zemini, altın sarısı çerçeve.

&#x09;				- Konum: Sağ başparmağın ekrana temas ettiği en doğal alt yay noktası.

&#x09;			- İkincil Eylem Düğmeleri (Aksiyon Çarkı / Dikey Dizi):

&#x09;				- "Turu Bitir" düğmesinin hemen üstünde dikey veya hafif kavisli (arc) biçimde dizilmiş $72 \\times 72\\text{ px}$ boyutunda 3 kare/yuvarlak düğme:

&#x09;					- Saldırı / Mod Değiştir (Kılıç <->Yay): Alp için menzilli/yakın dövüş geçişi.

&#x09;					- Siper Al (Defend): Kalan stamina puanını zırh sönümlemesine dönüştürür.

&#x09;					- Özel Yetenek / Nara At (Morale Boost / Power Shot): Varsa özel taktik hamle.



###### &#x09;	C. Üst Güvenli Bölge: Savaş Durumu \& Sayaç

&#x09;		Oyun alanının üst kısmı, başparmakların istemeden değemeyeceği (Dead Zone) en güvenli alandır.

&#x09;			- Üst Orta: Tur Numarası ve Faz Bilgisi ("3. Tur - Alp Fazı" veya "Düşman Hamlesi").

&#x09;			- Sağ Üst Köşe: Geri Çekil / Teslim Ol düğmesi. Kazara basılmasını önlemek için bas-çek değil, 1.5 saniye basılı tutma (Hold to Retreat) mekaniği ile çalışır.



#### 

#### **3. Altıgen Zemin Üzerinde Menzil \& İsabet Görsel Göstergeleri**



&#x09;	Mobil ekranda parmakla altıgene dokunulduğunda parmağın görüntüyü kapatmaması için göstergeler iki kademeli çalışır:



&#x09;	**Gösterge Türü											Görsel Efekt \& Renk																İşlev**

&#x09;	Yürüme Menzili										Şeffaf Turkuaz Dolgu (#008B9B, %35 Alpha)											Aktif birimin mevcut Stamina'sı ile gidebileceği tüm geçerli altıgenler.

&#x09;	Saldırı Menzili											İnce Kırmızı Altıgen Çerçeve (#8B261E, Kesikli Çizgi)								Birimin mevcut konumundan veya seçilen yürüme noktasından vurabileceği azami menzil.

&#x09;	Hedef Odak Balonu (Floating Combat Prediction)		Hedef düşmanın başının 40 px üzerinde açılan dinamik parşömen etiket			Hedeflenen düşmana dokunulduğunda çıkar: İsabet Şansı: %75, Tahmini Hasar: 14-18 HP / 6 Zırh.

&#x09;	Yol Güzergâhı (Path Preview)							Seçilen hedef hex'e doğru uzanan sarı noktalı ok izi								Oyuncu parmağını basılı tutup sürüklediğinde harcanacak Stamina miktarını adım adım gösterir.





#### **4. Dokunma Toleransları ve Mobil Girdi Güvenliği**



&#x09;	Minimum Dokunma Alanı (Touch Target): Tüm butonlar ve seçilebilir altıgen alanları en az 48 x 48 dp (Full HD ekranda yaklaşık 72 x72px) basma toleransına sahip olmalıdır.

&#x09;	Onay Mekaniği (Two-Tap Execution): Yanlış hamleleri önlemek adına tek dokunuş rotayı ve tahminleri gösterir (Preview), aynı hedefe ikinci dokunuş veya sağ alttaki yeşil onay ikonuna basış hareketi başlatır (Execute).

---

### **5. Mobil Çentik (Notch) & Safe Area Uyumluluğu**
* **Safe Area Filtresi:** Modern telefonlardaki kamera delikleri, çentikler ve alt gezinti çubuğu (Home Indicator) için Unity Canvas'ında `SafeAreaFitter` bileşeni kullanılmalıdır.
* **Kritik UI Elemanları:** Sol alt (Unit HUD) ve sağ alt (Action Buttons) kesinlikle Safe Area sınırları içinde tutulmalı; ekranın sıfır piksel kenarlarına yapışmamalıdır (En az 32px güvenli pay).
* **En-Boy Oranları Desteği:** 16:9'dan 21:9'a kadar (Ultra-wide mobil ekranlar) arayüz esneyebilir olmalı, ana savaş alanı merkezde `Anchor: Center` olarak kilitlenmelidir.

### **6. Diğer Ekran Yerleşimleri (Seyahat & Pazar)**
* **Seyahat Ekranı (2D Side-View):**
  * Üst Bölge: Erzak barı, Akçe miktarı, Tarih/Gün sayacı, Hava durumu ikonu.
  * Orta Bölge: Soldan sağa ilerleyen kervan animasyonu (Atlar, develer, yürüyen muhafızlar).
  * Alt Bölge: İlerleme çubuğu (Düğüm A'dan Düğüm B'ye mesafe), "Mola Ver / Kamp Kur" butonu.
* **Pazar & Şehir Ekranı (İzometrik / Parşömen):**
  * Sol Panel: Kervan Envanteri (Ağırlık kapasitesi, sahip olunan mallar, alış fiyatı).
  * Sağ Panel: Şehir Pazarı (Şehrin stokları, yerel satış fiyatı, kâr/zarar marjı indikatörü).
  * Orta/Alt: Hızlı miktar kaydırıcısı (Slider) ve "Satın Al / Sat" onay mühürleri.



---
# MİMARİ VE OYNANIŞ KARARLARI (11. Madde - Güncelleme)
1. **Platform:** Sadece mobil (yatay ekran - landscape) hedeflenmektedir.
2. **Oturum/Bağlantı:** Giriş ekranında Google Play, Apple Game Center ve Firebase Auth ikonları bulunacaktır.
3. **Sahne Politikası:** Builder scriptleri (otomatik sahne oluşturan kodlar) tamamen kaldırılmıştır. Sahneler Unity Editor üzerinden manuel oluşturulacaktır.
4. **Fraksiyonlar:** Oyunda 4 temel taraf (faction) bulunmaktadır: Osmanlı (Uç Beyliği), Bizans (Tekfurlar), Eşkıyalar, Vahşi Hayvanlar.
5. **Olay Çözümü:** Karşılaşılan olaylarda her zaman bir seçim penceresi (Kaç, Haraç Ver, Savaş) açılacaktır. "Savaş" seçilirse manuel veya otomatik savaşma seçenekleri sunulacaktır.
6. **Zaman:** Gerçek zamanlı akış yerine, rotalar üzerinde gün gün ilerleme (turn-based gibi) uygulanacaktır.
7. **Harita:** İlk sürümde sadece Anadolu/Bitinya haritası olacaktır, Balkanlar güncelleme ile eklenebilecek yapıdadır.
8. **Yorum Dili:** Koddaki açıklama satırları Türkçe karakter kullanılarak Türkçe yazılacaktır.
