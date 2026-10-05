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


