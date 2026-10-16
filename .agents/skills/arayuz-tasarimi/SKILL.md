---
name: arayuz-tasarimi
description: Yeni bir ekran, panel, popup veya görsel sahne (ör. TopBar, Seyahat ekranı, Pazar) eklenecekken kod yazmadan önce mobil yatay ekran için profesyonel mizanpaj önerisi ve üretilecek varlık listesi (isim, boyut, AI prompt) hazırlamak için kullanılır.
---
# Arayüz / Görsel Tasarım Önerisi Prosedürü

AGENTS.md §5 gereği: **önce tasarım önerisi, sonra varlık üretimi (kullanıcı), sonra entegrasyon.**

## Hazırlık (minimum okuma)
- `Docs/01-Oyun-Tasarimi.md`: ilgili ekranın tarifi (ör. "Diğer Ekran Yerleşimleri", Combat HUD bölümleri).
- `Docs/02-Teknik-ve-Gorsel.md`: §2 renkler, §3 9-slice/buton durumları, §7 isimlendirme, §9 import, §10 renk token'ları. Stil prompt'u için "Temel Stil Formülü".
- Mevcut varlıkları yeniden kullanmak için `Assets/_Project/Art/Sprites/UI/` altını **listele** (dosya adları yeterli). Var olan varlık tekrar ürettirilmez.

## Çıktı Şablonu
```
## Tasarım Önerisi: <Ekran adı> (<Adım-ID>)
**Oyuncu amacı:** Bu ekranda oyuncu ne yapar, ne karar verir? (1–2 cümle)

### 1. Yerleşim (1920×1080, landscape)
ASCII taslak: bölgeler + başparmak erişimi (sol alt / sağ alt birincil eylem, üst bölge pasif bilgi)

### 2. Bileşenler
| Bileşen | Konum / Anchor | Boyut (px) | Davranış | Varlık |
|---|---|---|---|---|

### 3. Etkileşim & Geri Bildirim
Açılış/kapanış animasyonu, SFX (`SFX_...`), boş durum metni, hata/uyarı durumu, Android Geri tuşu davranışı.

### 4. Varlık Listesi
| Dosya adı (§7 standardı) | Boyut | 9-slice border | Durumlar | Mevcut mu? |
|---|---|---|---|---|
+ eksik her varlık için AI üretim prompt'u (sonuna "Temel Stil Formülü" eklenir).

### 5. Teknik notlar
Canvas ayrımı (statik/dinamik), prefab adı (`Prefabs/UI/...`), veri kaynağı (GameSession/SO/EventBus olayı).
```

## Kurallar
- Mobil kuralları: Safe area içinde, kenardan min 32 px, dokunma hedefi min 48 dp (~72 px), kritik eylem başparmak bölgesinde, tehlikeli eylem (Geri Çekil, Sil) basılı tutma veya onay ile.
- Renk yalnızca §10 token'larından; metin okunurluğu için koyu zeminde krem, açık zeminde mürekkep siyahı.
- Tarihsel üslup: 14. yy minyatür, parşömen, Selçuklu geometrik motif; modern/parlak/gradyan yok.
- Sonda sor: **"Bu tasarımı onaylıyor musunuz? Onaylarsanız varlıkları üretip `Assets/_Project/Art/...` altına ekleyin; ardından `varlik-entegrasyonu` ile devam edelim."**
- Bu skill kod yazmaz.
