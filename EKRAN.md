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


