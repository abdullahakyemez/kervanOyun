---
name: kod-inceleme
description: Bir adım uygulandıktan sonra görev kapatılmadan önce veya kullanıcı "kodu incele / review et" dediğinde, değişen kodu proje standartlarına göre kontrol listesiyle denetler.
---
# Kod İnceleme Prosedürü

**Kapsam:** Sadece bu adımda değişen/eklenen dosyalar (`git diff --stat` ve `git diff` ile bul). Tüm projeyi inceleme.
**Referans:** `Docs/04-Mimari-ve-Kod-Standartlari.md` (gerekli başlıklar).

## Kontrol Listesi
**Mimari**
- [ ] Bağımlılık yönü doğru (View → Systems → State → Data); UI içinde iş kuralı/hesaplama/kayıt yok.
- [ ] Yeni `static Instance` Singleton açılmamış; kalıcı durum `GameSession`'da.
- [ ] Sistemler arası iletişim EventBus/Services ile; Subscribe ↔ Unsubscribe çift.
- [ ] Sahne geçişi `SceneLoader` üzerinden (A2.6 sonrası).

**Doğruluk**
- [ ] Kimlik eşleştirmesi ASCII ID ile; GameObject adı / görünen isim / `ToLower` karşılaştırması yok.
- [ ] Eski `UnityEngine.Input` kullanımı yok.
- [ ] Sabit oyun değeri (tarih, fiyat, mesafe, başlangıç akçesi) gömülü değil.
- [ ] Sahte/test verisi UI'da gösterilmiyor; boş durum var.
- [ ] Kayda giden veride SO referansı değil ID var.

**Unity / Performans**
- [ ] `Update()` içinde `Find*`, `GetComponent`, LINQ, string birleştirme yok.
- [ ] Eksik Inspector referansı için tek açıklayıcı `LogError`; sessiz `Find` fallback zinciri yok.
- [ ] `Resources.Load` yok (Resources klasörü kullanılmıyor).
- [ ] Debug/test kodu `#if UNITY_EDITOR || DEVELOPMENT_BUILD` ile korunmuş.

**Stil**
- [ ] İsimler İngilizce; yorumlar ve `<summary>` Türkçe karakterli Türkçe.
- [ ] `[SerializeField] private _camelCase`, `[Header]`/`[Tooltip]` Türkçe.
- [ ] Dosya ~400 satırı aşıyorsa bölme önerisi not edildi.

## Çıktı Formatı (kısa)
```
**İnceleme: <Adım-ID>** — <N> dosya
🔴 Düzeltilmeli: (madde, dosya:satır) — yoksa "yok"
🟡 Öneri (sonraya bırakılabilir): ...
✅ Uygun: (tek satır özet)
```
- 🔴 varsa düzeltme için onay iste; 🟡 maddeler kullanıcı isterse `06-Bilinen-Sorunlar.md`'ye TD olarak eklenir.
- Kod değiştirmeden önce onay al (AGENTS.md §2).
