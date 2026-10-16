---
name: varlik-entegrasyonu
description: Kullanıcı projeye yeni görsel/ses varlığı eklediğinde ("görselleri ekledim", "asset koydum") veya varlık klasörü düzenlemesi istendiğinde; isimlendirme, klasör, import ayarı ve Unity'ye bağlama adımlarını standartlaştırmak için kullanılır.
---
# Varlık (Asset) Entegrasyon Prosedürü

**Referans:** `Docs/02-Teknik-ve-Gorsel.md` §7 (klasör/önek), §8 (hijyen), §9 (import tablosu).

## 1. Tespit (salt okunur)
1. Yeni eklenen dosyaları bul: `git status --short Assets/` (veya kullanıcının verdiği klasör). Sadece dosya adlarını listele.
2. Her dosya için kontrol et:
   - İsim: önek, PascalCase, ASCII, yazım hatası yok
   - Klasör: §7 yapısına uygun mu (ekran adlı kök klasör yok)
   - Mükerrer: aynı/çok benzer isim veya aynı boyutta dosya zaten var mı?
   - Ham kaynak mı (`.ai`, mockup) → `Assets/` dışına (KR-5)
3. `06-Bilinen-Sorunlar.md` AST kayıtlarıyla ilişkiliyse not et.

## 2. Öneri Tablosu (onay iste)
```
| Mevcut yol | Önerilen yol / isim | Sebep | Import ayarı (§9) |
|---|---|---|---|
```
- Taşıma/yeniden adlandırma/silme **kullanıcı onayı olmadan yapılmaz** (AGENTS.md §3).

## 3. Uygulama Yöntemi
- **Tercih edilen:** Kullanıcıya Unity Project penceresinde yapılacak numaralı adımları ver (sürükle-bırak, F2 ile yeniden adlandır). Böylece `.meta` GUID'leri ve sahne referansları korunur.
- **AI'nin yapabileceği (onayla):** Henüz hiçbir sahne/prefab tarafından kullanılmayan dosyalarda, dosya **ve** `.meta` dosyası birlikte taşınabilir. Kullanılan bir varlıksa mutlaka Unity içinden taşınmalı.
- Unity'den önce: `git` ile commit alınmış olması önerilir (geri dönüş noktası).

## 4. Import Ayarları (kullanıcıya Unity adımları)
Her varlık türü için §9 tablosundaki değerleri numaralı adımlarla ver:
`Inspector → Texture Type → Sprite (2D and UI) → Max Size → Android override → Format ASTC 6×6 → Apply`
9-slice gerekiyorsa: `Sprite Editor → Border L/R/T/B` değerleri.

## 5. Unity'ye Bağlama
- Hangi prefab/objede, hangi bileşenin hangi alanına sürükleneceğini numaralı adımlarla yaz (ör. `TopBarHUD prefab → Image_Akce → Source Image ← Icon_Akce`).
- Kodda yeni `[SerializeField]` alan gerekiyorsa bu bir kod değişikliğidir → `ozellik-onerisi`.

## 6. Kapanış
Kullanıcı teyidinden sonra: çözülen AST kayıtlarını `06`'da kapat; `07`'ye kısa kayıt (`chore(art): ...`).
