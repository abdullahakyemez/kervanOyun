---
name: ozellik-onerisi
description: Kullanıcı bir özellik, düzeltme veya yol haritası adımı (ör. "A2.0'a başla") istediğinde, kod yazmadan önce kapsamı net, onaylanabilir bir uygulama planı sunmak için kullanılır.
---
# Özellik / Adım Önerisi Prosedürü

Doğrudan kod yazma. Önce hazırlık, sonra aşağıdaki şablonla plan sun.

## Hazırlık (minimum okuma)
1. İstek bir yol haritası adımıysa `Docs/05-Yol-Haritasi.md` içinde **sadece o adımın başlığını** oku (`### A2.x`).
   Yol haritasında yoksa: hangi aşamaya ait olduğunu belirle; gerekirse yeni adım ID'si öner (ör. `A2.9` veya revizyon `A1.R1`).
2. Dokunulacak her dosyanın adını `Docs/06-Bilinen-Sorunlar.md` içinde **ara**; ilgili kayıtları plana ekle.
3. İlgili kod dosyalarında önce grep ile metodu bul, sonra yalnızca o satır aralığını oku.
4. Kod içeriyorsa `Docs/04-Mimari-ve-Kod-Standartlari.md`'de ilgili başlığa bak (ör. §3 ID, §4 Input, §6 Ses).
5. Adım bir açık karara (KR-x) bağlıysa planı sunmadan önce kararı sor.

## Plan Şablonu
```
## Öneri: <Adım-ID> — <Başlık>
**Amaç (oyuncu gözünden):** ...
**Çözdüğü kayıtlar:** BUG-xx, TD-xx
**Uygulama mantığı:** 3–6 madde
**Dosyalar:**
| İşlem | Dosya | Ne değişecek |
|---|---|---|
| Yeni | `Scripts/.../X.cs` | ... |
| Değişecek | `Scripts/.../Y.cs` | sadece `MetotAdı` |
**Unity'de elle yapılacaklar:** numaralı adımlar (obje ekle, bileşen sürükle, SO oluştur…)
**Test senaryosu:** kullanıcının Play modunda deneyeceği 2–4 madde
**Riskler / yan etkiler:** ...
**Kapsam dışı (bilinçli olarak yapılmayacak):** ...
```

## Kurallar
- **Boyut sınırı:** En fazla ~5 dosya. Aşarsa adımı `A2.xa / A2.xb` diye böl ve böyle sun.
- `.unity/.prefab/.asset` dosyaları "Dosyalar" tablosunda **asla** yer almaz; onlar "Unity'de elle" bölümüne yazılır.
- UI/görsel içeriyorsa önce `arayuz-tasarimi` skill'ini kullan.
- Planın sonunda sor: **"Bu planı onaylıyor musunuz? Onaylarsanız uygulamaya geçeceğim."**

## Onaydan Sonra (UYGULA)
1. Yalnızca tablodaki dosyalara dokun. Yeni bir dosya gerekirse dur ve sor.
2. Kod `04` standartlarına uygun; yorumlar Türkçe karakterli.
3. Bitince kısa çıktı: değişen dosyalar listesi + "Unity'de elle yapılacaklar" + test senaryosu.
4. Son cümle: **"Unity Console'da hata var mı? Test senaryosu geçti mi?"**
5. Kullanıcı hata bildirirse → `hata-ayikla`. "Çalışıyor" derse → `kod-inceleme` (hızlı) → `gorev-kapat`.
