### **Bu teknik şartname, Unity'de 2D altıgen ızgara üzerinde kullanılacak karakter varlıklarının ölçek standardını, durum makinelerini (State Machine) ve görsel hiyerarşisini belirler.**





#### **1. Sprite \& Canvas Çözünürlük Standartları**

###### 

###### Mobil ekranlarda (hedef: Full HD 1920x1080 Landscape) izometrik altıgen hücrelerin okunabilirliği için taban karo ve karakter boyutları:



* Zemin Altıgen Hücresi (Hex Tile): Genişlik: 256 px, Yükseklik: 128 px (Klasik 2:1 izometrik oran).
* Karakter Canvas / Sprite Boyutu: 256 x 384 px (Tek kare çerçevesi).	

&#x09;	- Karakterin altıgen üzerindeki ayak izi (Pivot noktası): Alt orta merkez (Pivot: X: 0.5, Y: 0.12).

&#x09;	- Karakter yüksekliği altıgenin dikey ekseninin yaklaşık 2.5 katı kadar yukarı taşarak izometrik derinlik kazandırır.

* Yön (Orientations): Taktiksel mobil oyun optimizasyonu için 4 İzometrik Açı (İleri-Sağ, İleri-Sol, Geri-Sağ, Geri-Sol) yeterlidir. Simetri kullanılarak sol yönler sağdan SpriteRenderer.flipX ile türetilebilir (Yalnızca kalkan/kılıç elde tutuş asimetrisi sorun teşkil etmiyorsa).







#### **2. Temel Animasyon Durumları (State Machine Matrix)**



###### Tüm birimlerin C# animator kontrolcüsüne bağlanacak standart kare sayıları ve kare hızları:



###### **Durum Adı						|Kare Sayısı (Frames)					|FPS						|Döngü (Loop)							|Teknik / Görsel Açıklama**

Idle										4 – 6 kare											8 fps								Evet													Ağırlık aktarımı, hafif nefes alma, silahı hazır tutma.

Move (Yürüme/Koşma)						6 – 8 kare											12 fps								Evet													Hex merkezinden diğer hex merkezine geçiş adımı.

Attack (Saldırı)								5 – 7 kare											14 fps								Hayır													Rüzgâr alma >>>  Hızlı vuruş >>> Geri toparlanma (3. kare hasar anı tetikleyicisi).

Hit / Flinch (Darbe)							3 kare												12 fps								Hayır													Geriye irkilme ve zırh parçacığı efekti.

Down / Defeated							4 kare												10 fps								Hayır													Dize çökme veya yere düşme (Son karede yerde sabit kalır).





#### **3. Birim Görsel Özellikleri ve Tasarım Kartları**



###### &#x09;**A. Alp (Ana Karakter / Kıdemli Savaşçı)**



&#x09;	**- Siluet \& Boyut:** Geniş omuzlu, dik duruşlu, heybetli gazi silueti.

&#x09;	**- Giyim \& Zırh:**

&#x09;				- Başta kurt kürkünden kenarlıklı koyu kızıl börk.

&#x09;				- Göğüste perçinli deri zırh üzerine hafif çelik plaka takviyeleri; kolda çelik kolçaklar.

&#x09;				- Arkada rüzgârda dalgalanan kısa, solgun al (#8B261E) renkte pelerin.

&#x09;	**- Silah Teçhizatı:**

&#x09;				- Belde kavisli Türk kılıcı (kılınç), sırtta deri tirkeş (okluk) ve kompozit yay.

&#x09;	**- Animasyon Özelliği:**

&#x09;				- Attack: Çift modlu (Menzile göre kılıç savurma veya yay çekme).

&#x09;				- Idle: Soğukkanlı, temkinli ve kılıcın kabzasına hafifçe dokunan güvenli duruş.



###### &#x09;**B. Okçu (Gazi / Bozkır Avcısı)**



&#x09;	- **Siluet \& Boyut:** Çevik, zayıf-orta yapılı, hafif eğik yay çekme duruşu.

&#x09;	- **Giyim \& Zırh:**

&#x09;		**-** Çift katlı keçe kaftan, deri yelek ve yumuşak deri çizmeler.

&#x09;		- Zırh seviyesi görsel olarak düşüktür; metal plaka yerine sertleştirilmiş deri koruyucular bulunur.

&#x09;		- Renkler: Toprak tonları, zeytin yeşili ve açık bej.

&#x09;	- **Silah Teçhizatı:**

&#x09;		**-** İki elle tutulan büyük refleks yay; belde yakın dövüşe girildiğinde çekilen kısa kama.

&#x09;	- **Animasyon Özelliği:**

&#x09;		**-** Attack: Yayı germe anında 1-2 karelik duraksama (vuruş hissiyatı), ardından hızlı ok salınımı.



###### &#x09;**C. Kervan Muhafızı (Kalkanlı / Ağır Piyade)**

###### 

&#x09;	**-** Siluet \& Boyut: Tıkız, kalkan arkasına gizlenmiş savunma odaklı kütle.

&#x09;	- Giyim \& Zırh:

&#x09;			- Boynu ve omuzları örten demir zırh halkaları (örme zırh / zereh).

&#x09;			- Başta konik demir miğfer.

&#x09;			- Renkler: Dövme demir grisi (#4A4E51) ve Selçuklu kumtaşı tonları.

&#x09;	- Silah Teçhizatı:

&#x09;			- Sol kolda geniş yuvarlak ahşap/demir kalkan; sağ elde kısa mızrak veya düz kılıç.

&#x09;	- Animasyon Özelliği:

&#x09;			- Idle: Kalkanı göğüs hizasında tutarak siper alma pozisyonu.

&#x09;			- Hit: Darbeyi kalkanla karşılama ve kalkanın titreme animasyonu.



###### &#x09;**D. Eşkıya / Dağ Haydudu (Düşman)**



&#x09;	- Siluet \& Boyut: Düzensiz, vahşi, saldırgan ve kambur duruş.

&#x09;	- Giyim \& Zırh:

&#x09;			- Yırtık kürk detayları, yamalı kumaşlar, yüzü kısmen örten sarık veya paçavra.

&#x09;			- Renkler: Yanık kül kahvesi (#2B2321) ve pas tonları.

&#x09;	- Silah Teçhizatı:Ağır satır, balta veya çivili gürz; zırhsız veya yağmalanmış parçalı zırh artıkları.

&#x09;	- Animasyon Özelliği:

&#x09;			- Attack: Geniş açılı, kontrolsüz ve savurgan balta/satır hamlesi.

&#x09;			- Defeated: Darbeyi alınca savrularak sırtüstü düşme.



#### **4. Durum İkonları ve Vurgu Katmanları (Feedback Layers)**



&#x09;Her birim spritesheet'inin üzerinde Unity'de dinamik olarak açılıp kapanacak 3 görsel gösterge katmanı tasarlanmalıdır:

&#x09;	1. Seçim Halkası (Selection Ellipse): Ayakların hemen altındaki altıgen tabanında yer alan elips ışık (Dost için yeşil #2A4B37, Düşman için kırmızı #8B261E).

&#x09;	2. Stat Çubukları (Floating UI): Başın 20 px üzerinde yer alan mini barlar:

&#x09;			- Üst bar: Can (Yeşil / Kırmızı dolgu).

&#x09;			- İnce alt bar: Zırh (Gri / Mavi dolgu).

&#x09;	3. Moral / Panik İkonu: Baş hizasında beliren minyatür alev/hilal (Yüksek moral) veya kırık hançer (Panik/Kaçış hali).

---

#### **5. Unity 2D İzometrik Render & Sıralama (Sorting) Standartları**
* **Pixels Per Unit (PPU):** Tüm çevre ve karakter sprite'ları için **`PPU = 128`** olarak sabitlenmelidir.
  * Zemin hücresi (256x128 px): Tam olarak 2 x 1 Unity dünya birimi (Unit) kaplar.
  * Karakter canvas (256x384 px): 2 x 3 Unity dünya birimi kaplar.
* **İzometrik Derinlik Sıralaması (Transparency Sort Axis):**
  * `Project Settings -> Graphics -> Transparency Sort Mode: Custom Axis`
  * Değerler: **`X: 0, Y: 1, Z: -0.5`** (Öndeki birimin arkadakini doğru örtmesi için kritik matematik).
* **Sorting Layer Hiyerarşisi:**
  1. `Background` (Seyahat arka planları, uzak dağlar)
  2. `HexGrid` (Zemin altıgen karoları)
  3. `GridHighlights` (Menzil, yol izi, seçim halkası)
  4. `Entities` (Muhafızlar, haydutlar, kervan hayvanları - Pivot: Y Sorting)
  5. `VFX` (Ok izleri, toz, kan damlası, kılıç savurma efektleri)
  6. `WorldUI` (Birimlerin tepesindeki mini HP barları, hedef balonları)
  7. `ScreenUI` (Canvas HUD, menüler, diyalog pencereleri)

#### **6. Bellek & Doku Paketleme (Sprite Atlas Standartları)**
* **Atlas Boyutu:** Maksimum 2048x2048 px.
* **Sıkıştırma Formatı:** Android için `ASTC 6x6` veya `ASTC 8x8` (Performans ve görsel kalite dengesi).
* **Sprite Ayarları:** `Generate Mip Maps: Unchecked` (2D piksel netliği için kapalı olmalıdır), `Filter Mode: Point veya Bilinear` (Minyatür çizgilerinin bozulmaması için).


