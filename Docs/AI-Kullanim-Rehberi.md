# 🤖 Antigravity ile Yapay Zekâ Kullanım Rehberi (İnsan İçin)

> Bu dosya **proje sahibi için** hazırlanmıştır. Yapay zekâ bu dosyayı okumaz (AGENTS.md'de yönlendirilmemiştir).

---

## 1. Sistem Nasıl Çalışıyor?

```text
AGENTS.md  ───────────► Her mesajda otomatik yüklenir (anayasa: kurallar + doküman haritası)
   │
   ├─ .agents/skills/  ► İhtiyaç olunca devreye giren prosedürler (oturum aç, planla, kapat…)
   │
   └─ Docs/            ► Bilgi kaynağı; AI sadece gereken BÖLÜMÜ okur
        03 Pano (her oturum) · 05 Yol Haritası · 06 Sorunlar · 04 Mimari
        01 GDD · 02 Görsel · 07 Günlük · Arsiv/
```

| Dosya | Kim günceller? | Ne zaman? |
|---|---|---|
| `03-Proje-Durumu.md` | AI (`gorev-kapat`) | Her adım sonunda |
| `05-Yol-Haritasi.md` | AI (`asama-planla`, `gorev-kapat`) | Aşama başı / adım sonu |
| `06-Bilinen-Sorunlar.md` | AI | Sorun bulununca / çözülünce |
| `07-Degisiklik-Gunlugu.md` | AI | Her adım sonunda |
| `01`, `02`, `04` | Siz karar verirsiniz, AI önerir | Tasarım/kural değişince |

---

## 2. Skill'ler (Yetenekler) — Ne Zaman Hangisi?

Antigravity, `.agents/skills/` altındaki skill'leri **açıklamalarına bakarak otomatik seçer.** İsterseniz adını yazarak zorlayabilirsiniz (ör. *"oturum-baslat skill'ini kullan"*).

| Skill | Tetikleyen cümle örneği | Ne yapar? |
|---|---|---|
| `oturum-baslat` | "Oturumu başlat", "Neredeyiz?" | Sadece `03`'ü okur, 6 satır özet + sıradaki adım |
| `asama-planla` | "A3 Seyahat aşamasını planla" | Aşamayı tek oturumluk adımlara böler, `05`'e yazar |
| `ozellik-onerisi` | "A2.0'a başla", "Şu özelliği ekle" | Dosya tablosu + Unity adımları + test senaryosu ile plan |
| `arayuz-tasarimi` | "TopBar için tasarım öner" | Mizanpaj + varlık listesi + AI görsel prompt'ları |
| `varlik-entegrasyonu` | "Görselleri ekledim" | İsim/klasör/import kontrolü + Unity bağlama adımları |
| `hata-ayikla` | "Console'da şu hata var: …" | Kanıt ister, kök neden + en küçük düzeltme |
| `kod-inceleme` | "Kodu incele" | Değişen dosyaları standart listesine göre denetler |
| `gorev-kapat` | "Çalışıyor, kapat" | 03/05/06/07'yi günceller, commit mesajı önerir |

---

## 3. Standart Çalışma Döngüsü (Bir Adım)

```text
1. YENİ SOHBET AÇ
   Siz: "Oturumu başlat"
   AI : Aşama, son yapılan, sıradaki adım, bekleyen kararlar

2. PLAN
   Siz: "A2.0 ile devam" (veya kararları verin: "KR-1: sadece Bitinya")
   AI : Plan tablosu (dosyalar, Unity'de elle yapılacaklar, test senaryosu)

3. ONAY
   Siz: "Onaylıyorum" / "Uygula"   (değişiklik isterseniz: "X dosyasına dokunma, şunu da ekle")

4. UYGULAMA
   AI : Kodu yazar → değişen dosyalar + Unity adımları + test senaryosu

5. UNITY'DE SİZ
   - Unity'ye dönün, derlemeyi bekleyin
   - AI'nin verdiği Inspector/sahne adımlarını yapın
   - Test senaryosunu oynayın

6. SONUÇ
   Hata varsa → Console'daki İLK kırmızı hatayı aynen kopyalayıp yapıştırın (hata-ayikla)
   Çalışıyorsa → "Çalışıyor, kapat"  (gorev-kapat)

7. COMMIT
   AI'nin önerdiği mesajla Git commit (isterseniz AI'ye "commit at" diyebilirsiniz)

8. SOHBETİ KAPAT → Sonraki adım için yeni sohbet
```

---

## 4. Token Tasarrufu İçin Altın Kurallar

1. **Her adım için yeni sohbet açın.** Eski sohbet geçmişi her mesajda yeniden işlenir; bağlam zaten dokümanlarda kayıtlı.
2. **Sohbet uzarsa `/compact`** komutunu kullanın (bağlamı özetleyip küçültür).
3. **Hata bildirirken** sadece ilk kırmızı hatayı + stack trace'in ilk satırlarını yapıştırın; tüm Console'u değil.
4. **Dosya gösterirken `@` ile işaretleyin** (ör. `@WorldMapUI.cs`) — AI aramakla vakit kaybetmez.
5. **"Her şeyi analiz et" demeyin**; adım ID'si verin ("A2.4'e başla"). Geniş analiz sadece aşama başında (`asama-planla`) gerekir.
6. **Kararları toplu verin:** "KR-1: Bitinya, KR-2: GDD'deki 4 fraksiyon, KR-4: DOTween evet, diğerleri hayır."
7. **Kısa onaylar yeterli:** "Onay", "Uygula", "Kapat".

---

## 5. Antigravity Özellikleri — Ne İşe Yarar?

| Özellik | Bu projede kullanımı |
|---|---|
| `@dosya` / `@klasör` | İlgili dosyayı doğrudan bağlama ekler (ör. `@Docs/05-Yol-Haritasi.md`). |
| `/plan` | Büyük adımlarda (ör. A2.2 GameSession) AI'nin önce ayrıntılı plan çıkarmasını sağlar. |
| `/grill-me` | Bir tasarım kararını (ör. KR-1 harita kapsamı) soru-cevapla netleştirmek için. |
| `/compact` | Uzayan sohbeti özetleyip token'ı azaltır. |
| `/learn` | AI'ye bir düzeltme yaptıysanız ("bundan sonra hep şöyle yap") kalıcı kural haline getirmek için. Kural uygunsa `AGENTS.md`'ye eklenmesini isteyin. |
| `/goal` | Uzun, kesintisiz işler (ör. A7'de tüm varlık temizliği) için; günlük adımlarda gerekmez. |
| `/schedule` | Hatırlatıcı/periyodik iş (ör. "her cuma 06'daki açık 🔴'leri raporla"). |
| Model seçimi | Plan/mimari (A2.2, A2.3 gibi) için güçlü model; basit doküman/isim düzeltme için hızlı model yeterli. |

---

## 6. Örnek Komutlar (Kopyala-Yapıştır)

```text
Oturumu başlat.
```
```text
KR-1: Sadece Bitinya (Söğüt, Bilecik, Domaniç, İnegöl, Yenişehir, İznik, Bursa, Karacahisar, Eskişehir, Kütahya).
KR-2: GDD'deki 4 fraksiyon. KR-4: DOTween ve Newtonsoft evet, Localization A7'de.
```
```text
A2.0 Hijyen adımına başla. Önce Console teyidi: evet, "UnityEngine.Input" hatası görünüyor.
```
```text
Onaylıyorum, uygula.
```
```text
Console hatası: NullReferenceException: Object reference not set ...
  at Kervan.UI.WorldMapUI.Start () (at Assets/_Project/Scripts/TradeAndTravel/WorldMapUI.cs:87)
Söğüt'e tıklayınca oluyor.
```
```text
Test senaryosu geçti, Console temiz. Kapat.
```
```text
A3 Seyahat aşamasını planla.
```
```text
TopBar için arayüz tasarımı öner. Elimde UI_ToolbarBG, toolbarAkceIcon, toolbarErzakIcon var.
```
```text
Assets/_Project/Art altına yeni görseller ekledim, entegre et.
```

---

## 7. Sık Yapılan Hatalar

| Hata | Doğrusu |
|---|---|
| "Tüm oyunu yap" gibi büyük istek | Adım ID'si ile ilerleyin; AI zaten adımı ~5 dosyayla sınırlar. |
| Unity'de test etmeden "kapat" demek | Bitti Tanımı: Console temiz + test senaryosu geçti. |
| Sahne/prefab'ı AI'ye düzenletmek | Yasak (YAML bozulur). AI adım adım tarif eder, siz Unity'de yaparsınız. |
| Varlıkları dosya gezgininden taşımak | Unity Project penceresinden taşıyın (`.meta`/referanslar korunur). |
| Aynı sohbette birden fazla adım | Her adım = yeni sohbet. Bağlam dokümanlarda. |
| Karar vermeden geçmek | ⛔ KR-x olan adımlar karar verilmeden başlamaz. |

---

## 8. Bakım

- `03-Proje-Durumu.md` ~80 satırı aşarsa AI'den "03'ü sadeleştir, geçmişi 07'ye taşı" isteyin.
- Aşama bitince: "A2 aşamasını kapat ve özet çıkar" → tag + günlük kaydı.
- Yeni bir kalıcı kural doğarsa (ör. "tüm popup'lar alttan açılsın"): `AGENTS.md` (kısa kural) veya `04` (detay) güncellemesini isteyin.
