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
