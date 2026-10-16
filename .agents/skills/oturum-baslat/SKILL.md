---
name: oturum-baslat
description: Yeni bir sohbet başladığında veya kullanıcı "oturumu başlat / neredeyiz / devam edelim" dediğinde projenin güncel durumunu minimum token ile okuyup bağlamı kurar ve sıradaki adımı önerir.
---
# Oturum Başlatma Prosedürü

**Amaç:** En az okuma ile doğru bağlam. Tüm dokümanları okuma.

## Adımlar
1. **Yalnızca** `Docs/03-Proje-Durumu.md` dosyasını oku. (`AGENTS.md` zaten bağlamda.)
2. Diğer dokümanları bu aşamada **okuma**. Kullanıcı bir görev verdiğinde ilgili bölümü okursun.
3. Kullanıcıya en fazla 6 satırlık şu formatta yanıt ver:

```
Projeye bağlandım.
• Aşama: <ör. A2 — Dünya Haritası & Oyun Omurgası>
• Son tamamlanan: <ör. A2-UI Harita arayüzü>
• Sıradaki adım: <ör. A2.0 Hijyen — kısa hedef>
• Bekleyen kararlar: <KR-ID listesi veya "yok">
Sıradaki adımla mı devam edelim, yoksa farklı bir konu mu var?
```

4. Sıradaki adım bir kararla engelliyse (⛔ KR-x), önce o kararı kısa bir soruyla iste.
5. Kullanıcı sıradaki adımı onaylarsa → `ozellik-onerisi` skill'ine geç (o adımın `05-Yol-Haritasi.md` bölümünü okuyarak).

## Yapma
- `07-Degisiklik-Gunlugu.md`, `Arsiv/`, GDD'nin tamamı gibi büyük dosyaları açılışta okuma.
- Kod dosyalarını açılışta tarama.
