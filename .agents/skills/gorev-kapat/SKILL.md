---
name: gorev-kapat
description: Bir yol haritası adımı veya özellik Unity'de hatasız çalıştığı kullanıcı tarafından teyit edildiğinde ("çalışıyor", "tamam", "kapat") dokümantasyonu günceller ve commit mesajı önerir.
---
# Görev Kapatma Prosedürü

**Ön koşul:** Kullanıcı Console'un temiz olduğunu ve test senaryosunun geçtiğini teyit etti. Teyit yoksa önce sor.

## Adımlar
1. **`Docs/03-Proje-Durumu.md`**
   - İlgili adımın kutusunu işaretle `[x]`.
   - "▶️ Sıradaki Adım" bölümünü yol haritasındaki bir sonraki adımla değiştir (hedef, çözdüğü kayıtlar, ön teyit sorusu — 4 satırı geçmesin).
   - Bölüm 1 tablosunda durum değiştiyse güncelle; sorun sayısını güncelle.
   - Verilen kararları "Açık Kararlar"dan çıkar (kararın kendisini `01-Oyun-Tasarimi.md` §12'ye "Karar:" satırı olarak yaz).
2. **`Docs/05-Yol-Haritasi.md`** — adım başlığındaki durumu `✅` yap. Uygulamada plandan sapma olduysa adımın altına 1–2 satır "Uygulama notu" ekle.
3. **`Docs/06-Bilinen-Sorunlar.md`** — çözülen kayıtların Durum'unu "Çözüldü" yap ve "✅ Çözülenler" listesine `ID — tarih — adım — not` olarak ekle. Uygulama sırasında yeni sorun bulunduysa yeni ID ile ekle.
4. **`Docs/07-Degisiklik-Gunlugu.md`** — en üste (başlık altındaki ilk `---`'ten sonra) kayıt ekle:
   ```
   ## [GG.AA.YYYY] <Adım-ID> — <Başlık>
   - Değişen/eklenen dosyalar ve kısa açıklama
   - Unity'de elle yapılanlar
   - Commit: `<önerilen mesaj>`
   ```
5. **Aşama bittiyse** (aşamanın tüm adımları ✅): `03` çizelgesinde aşamayı ✅ yap, `07`'ye aşama özeti ekle, Git tag öner (ör. `v0.2.0-alpha`).
6. Kullanıcıya kısa mesaj:
   ```
   ✅ <Adım-ID> kapatıldı. Dokümanlar güncellendi (03, 05, 06, 07).
   Önerilen commit: <type>(<scope>): <Türkçe kısa açıklama>
   Sıradaki adım: <ID — başlık>. Yeni sohbette "oturumu başlat" diyerek devam edebilirsiniz.
   ```

## Commit Mesajı Formatı (Conventional Commits)
`feat` yeni özellik · `fix` hata düzeltme · `refactor` davranış değiştirmeyen düzenleme · `docs` doküman · `chore` temizlik/ayar · `test` test.
Örnek: `fix(map): pinch-zoom yeni Input System'e taşındı (BUG-01)`

## Not
Dokümanları güncellerken dosyaları baştan yazma; sadece ilgili satırları düzenle.
