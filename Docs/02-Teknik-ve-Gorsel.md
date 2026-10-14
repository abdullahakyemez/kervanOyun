### **Mobil ekranda hem bu tarihsel ağırlığı hissettirmek hem de görsel karmaşayı önlemek için kullanılacak renk ve stil standartları aşağıda yapılandırılmıştır.**



#### 

#### **1. Fraksiyon \& Bölge Renk Paletleri (Faction Color Tokens)**



Her güç odağının arayüzde (UI bordürleri, asker pelerinleri, şehir sancakları) anında tanınmasını sağlayacak temel

HEX kodları:



###### **A. Erken Osmanlı \& Gazi Uç Beylikleri (Sade, Kararlı, Bozkır/Orman)**



Koyu Al (Gazi Kırmızısı): 				#8B261E — Sancaklar, alp pelerinleri, ana vurgular.

Sancak Yeşili (İslam/Hilal Yeşili):		 #2A4B37 — İttifak göstergeleri,



**Osmanlı idareci UI panelleri.**

Deri / Keçe Kahvesi: 					#5C3A21 — Zırh altlığı, börk, eğer takımları.

Dövme Demir (Zırh Gri):				 #4A4E51 — Kılıç namluları, kalkan kenarları, zincir zırh.

###### 

###### **B. Selçuklu Mirası \& Kervansaray Kültürü (Geometrik Taş ve Çini**)

###### 

Selçuklu Turkuazı:			 			#008B9B — Kervansaray çinileri, güvenli bölge UI vurguları, su/kuyu noktaları.

Karatay Laciverti:						#162846 — Gece haritası, Selçuklu çini bordürleri, prestij ticaret malları.

Kalker Taşı / Kumtaşı: 					#D4B886 — Kervansaray kesme taş duvarları, han zeminleri, UI panel arka planı.



###### **C. Doğu Roma (Bizans) \& Pontus Rum (İmparatorluk, Saray ve Kilise)**

İmparatorluk Erguvanı (Tyrian Purple):		 #581845 — Tekfur sarayları, elit Bizans birimleri, mühürlü fermanlar.

Bizans Altını (Mozaik Sarısı): 				#C8963E — Sikke/akçe ikonları, Ortodoks ikonografi çerçeveleri, yüksek kademe unvanlar.

Mermer Beyazı: 							#E6E2D8 — Şehir surları, kitabeler, diyalog kutusu zeminleri.



###### **D. Çevre, Doğa ve Tehlike (Harita \& Karşılaşmalar)**



Bozkır / Anadolu Toprağı:			 #8C7355 — 2D yan görünüm seyahat yolu, altıgen ızgara toprak zemin.

Puslu Çam Yeşili: 					#1E352F — Haydut pususu ormanları, dağ geçitleri.

Eşkıya / Haydut Külü: 				#2B2321 — Haydut kıyafetleri, yanmış kervan kalıntıları, düşman HP barı.



## **2. Arayüz (UI) Fonksiyonel Renk Standartları**



Mobil arayüzde oyuncunun gözünü yormamak için nötr parşömen tabanı ve yüksek kontrastlı bildirim tonları kullanılır:



###### **Arayüz Unsuru				   |HEX Kodu			   |Kullanım Alanı**

Parchment Base (Parşömen)			#F2E6CE						Envanter, pazar listesi ve diyalog penceresi iç zeminleri

Dark Ink (Mürekkep Siyahı)				#1E1815						Okunabilir tüm gövde metinleri, başlıklar ve çerçeve çizgileri

Gold / Akçe								#DAA520						Para birimi, kâr marjı artışları, onay butonları

Morale High (Yüksek Moral)			#2E7D32						Moral barı dolu seviyesi, başarılı takas göstergesi

Morale Low / Danger					#B71C1C						Düşük moral, panik hali, zararlı pazar fiyatı, can barı

Hex Highlight (Seçili/Yol)				#4A90E2 (Alpha %50)			Altıgen savaşta hareket edilebilir menzil vurgusu



###### 

###### **Çizim Tekniği (Illustration \& Shading)**



Hat Sanatı ve Kontur (Linework): Piksel sanatı yerine el çizimi hissi veren, kenarları hafif dokulu (ink outline) 1.5–2 piksel kalınlığında koyu kahverengi/siyah konturlar (#1E1815). Saf siyah konturdan (#000000) kaçının; dönemin mürekkep dokusunu bozmamak gerekir.



Gölgeleme: Gerçekçi degrade (gradient) yerine düz (flat) alanlar ve 2 kademeli sert gölge (Cel-shading / Minyatür usulü katmanlama). Bu yaklaşım, mobil cihazlarda düşük çözünürlükte bile silüetlerin ayırt edilmesini sağlar.



Doku (Texture Overlay): Tüm 2D sahne ve UI pencerelerinin en üst katmanında hafif bir eskitilmiş kağıt/parşömen gürültüsü (Noise / Parchment grain, %5–8 opaklık) bulunmalıdır.



**Tipografi Hiyerarşisi**



Başlıklar \& Şehir İsimleri: Geniş aralıklı, taş kitabe veya erken dönem latin/kiril-arap harmanını andıran serif display fontlar (Ör. Cinzel Decorative, IM Fell English).



Sayısal Değerler \& İstatistikler: Küçük ekranda basamakların kaymaması için sabit genişlikli (monospaced) veya net sans-serif fontlar (Ör. Cinzel başlık altına Inter Semi-Bold).

---

## **3. Kullanıcı Arayüzü (UI) Varlık & 9-Slice Standartları**
* **9-Slice Parşömen Panelleri:** Pencere boyutları değiştikçe kenar bordürlerinin bozulmaması için tüm parşömen ve taş zeminler Unity Sprite Editor'de 9-Slice (border padding örn: L:24, R:24, T:24, B:24) olarak dilimlenmelidir.
* **Buton Durumları (Button States):** Her buton için 3 ayrı görsel durum üretilir:
  * *Normal:* Mat taş/bronz/altın kabartmalı çerçeve.
  * *Pressed (Basılı):* 2 piksel içeri gömülmüş (hafif koyulaşmış) durum.
  * *Disabled (Pasif):* Gri/desatüre (#4A4E51) tonlu durum.
* **İkon Standartları:** 64x64 px ve 128x128 px, kare veya dairesel Selçuklu geometrik madalyonu içine oturan net silüetler.

## **4. Yapay Zeka Görsel Üretim Formülü (Style Prompt Formula)**
Minyatür ve 14. yüzyıl erken dönem el yazması hissini yakalamak için görsel üretim araçlarında (Midjourney / FLUX) kullanılacak standart stil parametreleri:
> `14th century Ottoman Anatolian miniature painting style, historical illuminated manuscript art, bold dark brown ink outlines, 2-stage flat cel-shading, matte gouache and natural earth pigments, aged parchment paper texture, Islamic and Byzantine historical art style, clean isometric game asset, neutral background --no 3d render, modern vector, shiny plastic, photorealistic`


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


# 🎨 Sanat & Varlık (Asset) Üretim Rehberi: Yapay Zeka Promptları ve Araçlar

> **Proje:** Uç Beyliği Kervanı  
> **Sanat Tarzı:** 14. Yüzyıl Erken Osmanlı & Anadolu Minyatürü, Mürekkep Konturlu El Çizimi (Ink Outlines), 2 Kademeli Cel-Shade, Parşömen Dokusu.

---

## 🛠️ 1. Önerilen Programlar ve Araç Seti

| Araç Türü | Önerilen Programlar | Kullanım Amacı | Maliyet |
| :--- | :--- | :--- | :--- |
| **Yapay Zeka Görsel Üretimi** | Midjourney v6, FLUX.1 (Schnell / Dev), DALL-E 3 | Minyatür karakter, karo ve UI konseptlerinin üretimi | Ücretli / Açık Kaynak |
| **2D Çizim & Rötuş** | Krita, Photoshop, Photopea (Tarayıcı tabanlı) | Renk düzeltmeleri, piksel temizliği, 9-slice dilimleme | Ücretsiz / Ücretli |
| **Piksel & Spritesheet** | Aseprite, TexturePacker | Kareleri birleştirme, animasyon önizleme, atlas oluşturma | Düşük Ücretli |
| **Arka Plan Temizleme** | rembg (Python/Web), Clipdrop, Photoshop Quick Action | Üretilen sprite'ların transparan PNG'ye dönüştürülmesi | Ücretsiz / Freemium |
| **UI & Vektör Tasarımı** | Figma, Inkscape | Butonlar, paneller, ikon yerleşimleri ve prototip tasarımı | Ücretsiz |

---

## 📜 2. Temel Stil Formülü (Master Style Modifier)

Yapay zekaya gireceğin **her promptun sonuna** stil tutarlılığını sağlamak için bu anahtar blok eklenmelidir:

```text
style: 14th century early Ottoman Anatolian miniature painting, historical illuminated manuscript art, dark brown ink linework (#1E1815), 2-stage flat cel-shading, matte gouache and natural earth pigments, aged parchment paper texture, clean silhouette, isometric tactical game asset, solid white background, sharp focus --no 3d render, modern vector, photorealistic, glossy plastic, gradients, messy noise
```

---

## 🏹 3. Karakter Üretim Promptları (256x384 px Hedef)

> **İpucu:** Görselleri üretirken her zaman `solid neutral white/grey background` (düz beyaz/gri zemin) ile üret. Bu sayede arka planı silmek (rembg veya sihirli değnek) 1 saniye sürer.

### A. Kıdemli Alp (Ana Karakter)
```text
Anatolian Ottoman warrior Alp merchant, 40 years old, noble bearded face, wearing wolf-fur trimmed crimson cap (bork), riveted lamellar leather armor over tunic, worn red cape, holding a curved Turkish kilij sword, bow and quiver on back, 14th century historical miniature style, isometric front-right 45-degree angle view, full body standing combat pose, bold ink contours, cel-shaded, parchment texture, plain white background
```

### B. Bozkır Okçusu (Gazi)
```text
Turkoman steppe archer gazi, agile pose, felt kaftan, leather chest guard, archer thumb ring, drawing a recurve composite Turkish bow, quiver at waist, 14th century Ottoman Anatolian miniature manuscript style, isometric 45-degree angle, full body tactical character sprite, clean ink outlines, flat gouache colors, solid white background
```

### C. Kervan Muhafızı (Kalkanlı Piyade)
```text
Heavy caravan mercenary guard, round wooden shield with iron rim, conical iron helmet, chainmail collar, holding a short spear, defensive brace posture, 14th century Seljuk Anatolian guard style, isometric view, game unit sprite, bold dark ink outline, flat cel-shading, white background
```

### D. Dağ Haydudu / Eşkıya (Düşman)
```text
14th century Anatolian mountain bandit, rugged bearded outlaw, ragged sheepskin vest, patched clothing, turban scarf wrapped around head, holding an iron battle axe, aggressive hunched combat stance, historical miniature painting style, isometric 45-degree, game sprite, clean dark contours, plain white background
```

---

## 🧱 4. İzometrik Zemin Altıgenleri (Hex Tiles - 256x128 px Hedef)

Hex karoları 2:1 oranında yatay elips şeklinde olmalıdır.

### Toprak / Bozkır Yolu Karosu
```text
Single isolated isometric 2D hexagonal ground tile, arid Anatolian steppe earth and small pebble stones, 14th century miniature art style, 2:1 isometric ratio, flat top surface, stone-textured rim edge, clean dark ink outline, aged parchment look, solid white background --no perspective angle distortion
```

### Taş Yol / Kervansaray Taş Zemin Karosu
```text
Single isolated isometric 2D hexagonal tile, aged Seljuk limestone cobbles, weathered stone pavement, 14th century miniature style, 2:1 isometric ratio, flat top, hand-painted ink linework, solid white background
```

---

## 🏛️ 5. Arayüz (UI) Parşömen & Buton Promptları

### Parşömen Bilgi Penceresi (Panel)
```text
Empty medieval manuscript parchment scroll banner, 14th century Seljuk geometric turquoise and gold border trim, flat aged vellum paper texture in center, clean rectangular UI game window frame, blank center for text, hand-drawn ink ornament, white background
```

### Selçuklu Kabartmalı Taş / Bronz Buton
```text
Carved Seljuk stone push button for UI game, engraved geometric star border, embossed bronze center, historical oriental tactile button, rectangular 180x64 proportion, flat game asset, clean edges, white background
```

---

## 🔄 6. Üretimden Unity'ye 4 Adımlı İş Akışı

1. **Üretim (AI):** Yukarıdaki promptlarla 3-4 varyasyon üret ve en temiz silüetli olanı seç.
2. **Arka Plan Temizleme (Transparent PNG):** `rembg` (ücretsiz) veya Photoshop ile arka planı sıfırla.
3. **Kırpma ve Boyutlandırma:**
   - Karakter: `256 x 384 px` (Canvas içi ortala, ayak tabanını alt kenardan ~45 piksel yukarıya konumlandır).
   - Hex Karo: `256 x 128 px`.
4. **Unity Ayarları:**
   - Texture Type: `Sprite (2D and UI)`
   - Pixels Per Unit: `128`
   - Pivot: Custom `(X: 0.5, Y: 0.12)` (Karakterler için ayak basma noktası)
   - Filter Mode: `Point` veya `Bilinear`
   - Compression: `ASTC 6x6`

---

## 🏛️ 7. Profesyonel Klasör Hiyerarşisi ve Varlık İsimlendirme Anayasası

Tüm görsel ve işitsel varlıklar, proje kökünde veya rastgele ekran klasörlerinde dağınık bırakılamaz. Standart yapı:

```text
Assets/_Project/
├── Art/
│   ├── Fonts/                                  # Yazı tipleri (.ttf, .otf)
│   ├── Sprites/
│   │   ├── UI/
│   │   │   ├── Backgrounds/                    # UI_BG_<Adı>.png (Örn: UI_BG_MainMenu.png)
│   │   │   ├── Buttons/                        # UI_Btn_<Adı>.png (Örn: UI_Btn_Wood_Large.png)
│   │   │   ├── Frames/                         # UI_Frame_<Adı>.png (Örn: UI_Frame_Market.png, UI_Frame_Divan.png)
│   │   │   ├── Icons/
│   │   │   │   ├── Login/                      # Icon_Login_<Sağlayıcı>.png (Örn: Icon_Login_Google.png)
│   │   │   │   └── Utility/                    # Icon_Util_<Fonksiyon>.png (Örn: Icon_Util_Gear.png, Icon_Util_ArrowLeft.png)
│   │   ├── Units/                              # Unit_<BirimAdı>.png (Örn: Unit_Alp.png, Unit_Haydut.png)
│   │   └── Items/                              # Item_<EmtiaAdı>.png (Örn: Item_Ipek.png, Item_Demir.png)
│   └── UI/
│       └── Mockups/                            # Ekran tasarımı referans ve plan görselleri
├── Audio/
│   ├── Music/                                  # BGM_<ParçaAdı>.* (Örn: BGM_Main.mp3, BGM_Travel.mp3)
│   └── SFX/                                    # SFX_<EfektAdı>.* (Örn: SFX_ButtonClick.wav, SFX_PaperRustle.wav)
├── Prefabs/
│   └── UI/                                     # Prefab pencereler (Örn: Popup_Settings.prefab, Panel_UtilityDrawer.prefab)
├── Scenes/                                     # Unity sahneleri (.unity)
└── Scripts/                                    # C# scriptleri (UI/, Audio/, Core/, Combat/, Trade/)
```

### Standart Ön Ekler (Prefix Rules)
- Arka planlar: `UI_BG_`
- Buton zeminleri: `UI_Btn_`
- Çerçeve / Pencereler: `UI_Frame_`
- İkonlar: `Icon_Login_` / `Icon_Util_` / `Icon_Map_`
- Oyun Başlığı / Logolar: `UI_Logo_`
- Müzikler: `BGM_`
- Ses Efektleri: `SFX_`

