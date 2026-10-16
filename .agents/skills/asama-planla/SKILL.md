---
name: asama-planla
description: Yeni bir aşamaya (ör. A3 Seyahat) başlanırken veya kullanıcı büyük bir özelliği adımlara bölmek istediğinde, aşamayı tek oturumluk, test edilebilir adımlara ayırıp yol haritasına yazmak için kullanılır.
---
# Aşama Planlama Prosedürü

## Hazırlık
1. `Docs/03-Proje-Durumu.md` (zaten okunduysa tekrar okuma) → aşamanın ön koşulları tamam mı? Önceki aşamada açık 🔴 kayıt varsa uyar.
2. `Docs/05-Yol-Haritasi.md` → sadece ilgili aşamanın taslak bölümü.
3. `Docs/01-Oyun-Tasarimi.md` → aşamayla ilgili maddeler (ör. Seyahat için §3, §6 "Seyahat Ekranı", §11).
4. `Docs/06-Bilinen-Sorunlar.md` → "Çözecek Adım" sütununda bu aşamayı ara.
5. Mevcut ilgili kodu sadece **listele** (dosya adları + satır sayısı); içerik okumaya plan onaylandıktan sonra adım adım geç.

## Bölme Kuralları
- 1 adım = 1 oturum = en fazla ~5 dosya = tek commit = tek test senaryosu.
- Sıra: **Tasarım önerisi → Veri/Mantık (saf C#) → Görünüm (UI) → Entegrasyon/Akış → Cila**.
- Her adım kendi başına çalışır durumda bırakır (yarım kalmış, derlenmeyen ara durum yok).
- Görsel varlık gereken adımın öncesine bir "Varlık hazırlığı" adımı koy (`arayuz-tasarimi` çıktısı).
- Karar gerektiren noktaları KR-ID olarak çıkar.

## Çıktı (kullanıcıya önce sun, onaydan sonra 05'e yaz)
```
## Aşama Planı: A<n> — <Başlık>
**Hedef (oyuncu gözünden):** ...
**Ön koşullar:** ✅/❌ listesi
**Bekleyen kararlar:** KR-x ...
| Adım | Başlık | Tür | Çözdüğü kayıtlar | Varlık gerekir mi? |
|---|---|---|---|---|
| A<n>.1 | ... | Tasarım | — | Evet |
| A<n>.2 | ... | Mantık | BUG-05 | Hayır |
**Aşama bitti tanımı:** 3–5 madde (oyuncunun yapabildiği şeyler)
```

## Onaydan Sonra
1. `05-Yol-Haritasi.md`'de aşamanın taslak bölümünü, A2 adımlarıyla **aynı formatta** (Amaç, Çözer, Muhtemel dosyalar, Unity'de elle, Test) yaz.
2. `03-Proje-Durumu.md`'de aşamayı ⏳ yap, adım listesini ve "Sıradaki Adım"ı güncelle; yeni kararları "Açık Kararlar"a ekle.
3. Kod yazma. İlk adım için `ozellik-onerisi`'ne geç.
