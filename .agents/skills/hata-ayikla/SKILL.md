---
name: hata-ayikla
description: Kullanıcı Unity Console hatası, uyarısı, beklenmeyen davranış veya "çalışmıyor" bildirdiğinde hatayı en az okuma ile bulup en küçük düzeltmeyi önermek için kullanılır.
---
# Hata Ayıklama Prosedürü

**İlke:** Tahmin etme, kanıt iste. En küçük düzeltme. Kapsamı genişletme.

## 1. Kanıt Topla (eksikse sor, tek mesajda)
- Console'daki **ilk** kırmızı hatanın tam metni + stack trace'in ilk 3–5 satırı (dosya:satır).
- Hangi sahne, hangi eylemden sonra (ör. "Haritada Söğüt'e tıklayınca").
- Beklenen davranış ↔ gerçekleşen davranış.
- Hata Editörde mi, cihazda/build'de mi?

> Not: Sonraki hatalar çoğu zaman ilk hatanın sonucudur. Önce ilk hata çözülür.

## 2. Konumla (minimum okuma)
1. Stack trace'teki dosya:satır → yalnızca o metodu oku (±30 satır).
2. Dosya adını `Docs/06-Bilinen-Sorunlar.md` içinde ara — bilinen bir kayıtsa (ör. BUG-01) onu referans ver.
3. Gerekirse referansları grep ile ara (çağıranlar, abonelikler). Tüm projeyi okuma.

## 3. Sınıflandır
| Tür | Tipik belirti | İlk bakılacak yer |
|---|---|---|
| Derleme hatası (CS####) | Play'e basılamıyor | Hata satırı, eksik `using`, asmdef referansı |
| NullReference | Inspector alanı boş | Sahnedeki obje bağlantısı → **kullanıcıya Unity'de kontrol adımı ver** |
| MissingReference | Sahne değişiminden sonra | Yok edilen objeye kalan abonelik (EventBus Unsubscribe) |
| InvalidOperation (Input) | Eski Input API | `04` §4 |
| Davranış hatası | Hata yok ama yanlış iş | Veri (SO alanları), ID eşleşmesi (`04` §3) |

## 4. Düzeltme Önerisi
```
**Kök neden:** 1–2 cümle
**Kanıt:** dosya:satır
**Düzeltme:** değişecek dosya(lar) + kısa diff/kod
**Unity'de kontrol:** (varsa Inspector adımı)
**Doğrulama:** kullanıcının tekrar deneyeceği adım
```
- Düzeltme yalnızca Inspector bağlantısı ise **kod değiştirme**, Unity adımı ver.
- Kod değişikliği 1–2 dosyayı aşıyorsa veya mimari değişiklik gerektiriyorsa → hızlı yama yerine `ozellik-onerisi` ile plan sun.
- Onay olmadan dosya değiştirme (AGENTS.md §2).

## 5. Sonra
- Düzeldiyse: aktif bir adımın parçasıysa adıma devam; bağımsız bir düzeltmeyse `gorev-kapat` (06'ya "Çözüldü").
- Kök neden yeni ve hemen çözülmeyecekse: `06-Bilinen-Sorunlar.md`'ye yeni ID ile kaydet.
- 2 denemede çözülmediyse: varsayımları listele, kullanıcıdan ek kanıt iste (Debug.Log ekleme önerisi dahil).
