# Değişiklik Günlüğü (CHANGELOG)

Tüm önemli değişiklikler bu dosyada belgelenir.

Format: [Keep a Changelog](https://keepachangelog.com/tr/1.0.0/),  
Versioning: [Semantic Versioning](https://semver.org/lang/tr/).

---

## [3.1.0] - 2026-11-03

### Eklenen
- **Custom Window Chrome**: Borderless pencere, sürüklenebilir title bar, resize grip, min/max/close butonları
- **Tema Sistemi**: Light / Dark / System (ResourceDictionary runtime swap)
- **Web Arama Kısayolları**: `g ` (Google), `yt ` (YouTube), `gh ` (GitHub), `so ` (Stack Overflow), `wk ` (Wikipedia)
- **Not Alma Alanı (Scratchpad)**: Title bar/Tray'den erişim, auto-save, çok satırlı, Tab desteği
- **Çoklu Monitör Hatırlama**: DeviceName bazlı konum/boyut/maximize kaydı, DPI-aware
- **Otomatik Güncelleme**: GitHub Releases API kontrolü, banner/toast bildirimi, indirme linki
- **About Dialog Güncelleme**: Web arama kısayolları rehberi eklendi
- **GitHub Actions Release Workflow**: Tag push / manual dispatch ile otomatik build + release asset upload

### Değiştirilen
- Uygulama adı: **LeonDesk → EgeFire** (görünen ad, ikon, title, config yolu)
- Config yolu: `C:\EasyTaskStarter\config.json`
- Pencere boyutu varsayılan: 300×400
- Opacity slider: Sadece mouse release'te commit (`PreviewMouseUp`)
- Tray ikonu: WndProc hook + ExtractAssociatedIcon (çift icon sorunu çözüldü)
- X butonu: Pencereyi kapatmaz, tray'e küçültür
- Windows Light tema (varsayılan) — SystemColors ile uyumlu sekmeler

### Düzeltilen
- CS0104 ambiguous reference hataları (KeyEventArgs, Clipboard, MessageBox, OpenFileDialog, DragDrop, DataObject, ListBox, Point, DragDropEffects, DataFormats) — WPF alias pattern ile çözüldü
- `Screen` sınıfı bulunamadı — `<UseWindowsForms>true</UseWindowsForms>` eklendi
- XAML parse hatası: `&` karakterleri `&` olarak escape edildi
- Çift tray icon sorunu
- Şeffaflık bug'ı (ValueChanged yerine PreviewMouseUp)

### Altyapı
- .NET 10 WPF target
- NuGet: Newtonsoft.Json, Microsoft.Web.WebView2
- `<UseWindowsForms>true</UseWindowsForms>` for Screen class
- GitHub repo: `mserhangulhan/EgeFire`

---

## [2.0.0] - 2026-08-27 (LeonDesk olarak yayınlandı)

### Eklenen
- Temel WPF projesi
- Config yönetimi (C:\EasyTaskStarter)
- Kısayol ekleme/kaldırma (Gözat butonu, otomatik ad)
- Hızlı erişim klasörleri (Bu Bilgisayar, Belgeler, Masaüstü, İndirilenler)
- Global hotkey (Ctrl+Alt+D, ayarlanabilir)
- Opacity slider
- Always-on-top toggle
- Sistem tepsisi (Tray) — çift tık aç, sağ tık menü
- Tek instance (Mutex)
- Uygulama ikonu (kelepçe)
- Başlangıçta çalış (Registry, tray menüsünden toggle)
- `--autorun` parametresi (gizli başlatma)

---

## [1.0.0] - İlk prototip

- Temel kısayol listesi
- Basit config kaydet/yükle
- Tray icon

---

## Sürümleme Stratejisi

| Sürüm | Anlam |
|-------|-------|
| MAJOR (X.0.0) | Breaking change, mimari yenileme, config formatı değişimi |
| MINOR (x.Y.0) | Yeni özellik, UI iyileştirme, yeni ayar |
| PATCH (x.y.Z) | Bug fix, küçük düzeltme, çeviri |

---

## Release Süreci

1. `git tag vX.Y.Z` → push
2. GitHub Actions workflow tetiklenir
3. Windows-latest runner: restore → build → publish (self-contained, single-file)
4. `softprops/action-gh-release` ile Release oluşturulur
5. `EgeFire.exe` asset olarak eklenir
6. UpdateChecker API'si yeni `tag_name` ve asset URL'sini görür

---

## İlgili Bağlantılar

- [Releases](https://github.com/mserhangulhan/EgeFire/releases)
- [Issues](https://github.com/mserhangulhan/EgeFire/issues)
- [GitHub Actions](https://github.com/mserhangulhan/EgeFire/actions)
