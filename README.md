# EgeFire

**Windows masaüstü kısayol yöneticisi** — Sık kullanılan uygulamaları, klasörleri ve web aramalarını tek tıkla (veya global hotkey ile) başlatın.

![EgeFire Screenshot](assets/screenshot.png)

## ✨ Özellikler

### 🚀 Hızlı Başlatma
- **Uygulama kısayolları**: `.exe`, `.lnk`, `.bat`, `.cmd`, `.ps1`, `.url` — argüman desteği
- **Hızlı erişim klasörleri**: Bu Bilgisayar, Belgeler, Masaüstü, İndirilenler (varsayılan) + özel klasörler
- **Global Hotkey**: `Ctrl+Alt+D` (ayarından değiştirilebilir) — pencereyi anında göster/gizle

### 🎨 Modern Arayüz (Custom Chrome)
- **Borderless pencere**: Standart Windows title bar'ı kaldırıldı, tamamen özelleştirilmiş chrome
- **Sürükleme**: Title bar boş alanı sürükleyerek pencereyi taşıma
- **Yeniden boyutlandırma**: Sağ alt köşedeki grip ile boyut değiştirme
- **Min/Max/Close butonları**: Kendi stillerimizle

### 🌗 Tema Sistemi
| Tema | Açıklama |
|------|----------|
| **Light** | Açık, temiz Windows arayüzü |
| **Dark** | Koyu mod, göz yormaz |
| **System** | Windows tema ayarını takip eder |

Title bar'daki 🌙/☀️ butonu ile anında değiştirme, seçim `config.json`'a kaydedilir.

### 🔍 Web Arama Kısayolları
Arama kutusuna prefix yazıp Enter'a basın:

| Prefix | Motor | Örnek |
|--------|-------|-------|
| `g ` | Google | `g egefire wpf` |
| `yt ` | YouTube | `yt wpf tutorial` |
| `gh ` | GitHub | `gh mserhangulhan/EgeFire` |
| `so ` | Stack Overflow | `so wpf custom chrome` |
| `wk ` | Wikipedia | `wk model-view-viewmodel` |

> **İpucu**: About penceresinde (❓ butonu) tüm kısayollar listelenir.

### 📝 Not Alma (Scratchpad)
- Title bar'daki 📝 butonu veya Tray menüsünden açılır
- **Otomatik kaydet**: Yazdıkça `config.json`'a yazar, uygulama kapanınca kaybolmaz
- Çok satırlı, Tab desteği, kaydet/iptal butonları

### 🖥️ Çoklu Monitör Desteği
- Pencere konumu, boyutu ve maximize durumu **her monitör için ayrı** hatırlanır
- Monitör `DeviceName` ile eşleştirilir, takılı değilse merkeze düşer
- DPI-aware koordinatlar

### 🔔 Sistem Tepsisi (Tray) Entegrasyonu
- **Çift tık**: Pencereyi göster/gizle
- **Sağ tık menü**:
  - Aç / Gizle
  - Notlar 📝
  - Başlangıçta Çalış (toggle, Registry)
  - Hakkında ℹ️
  - Kapat
- Uygulama X butonuna basınca **kapanmaz, tray'e küçülür**

### 🔄 Otomatik Güncelleme
- GitHub Releases API (`/repos/mserhangulhan/EgeFire/releases/latest`) ile kontrol
- Yeni sürüm varsa banner/toast bildirimi
- Tek tıkla indirme sayfası açılır
- `--autorun` parametresiyle Windows başlangıcında sessiz başlatma

---

## 📥 Kurulum

### Seçenek 1: GitHub Releases (Önerili)
1. [Releases sayfasına gidin](https://github.com/mserhangulhan/EgeFire/releases/latest)
2. `EgeFire.exe` indirin
3. Çalıştırın — kurulum gerektirmez (portable)

### Seçenek 2: Kaynak Koddan Derleme
```bash
git clone https://github.com/mserhangulhan/EgeFire.git
cd EgeFire
dotnet build LeonDesk.csproj --configuration Release
# Çıktı: bin/Release/net10.0-windows/win-x64/EgeFire.exe
```

**Gereksinimler**: .NET 10 Runtime (veya self-contained publish ile runtime'sız)

---

## ⚙️ Konfigürasyon

Config dosyası: `C:\EasyTaskStarter\config.json`  
(Yazılamazsa: `%APPDATA%\EasyTaskStarter\config.json`)

```json
{
  "Shortcuts": [
    { "Name": "Not Defteri", "ExePath": "notepad.exe", "Arguments": "" },
    { "Name": "VS Code", "ExePath": "C:\Program Files\Microsoft VS Code\Code.exe", "Arguments": "" }
  ],
  "QuickAccess": [
    { "Name": "Belgeler", "Path": "C:\Users\Murat\Documents" }
  ],
  "Hotkey": "Ctrl+Alt+D",
  "Opacity": 0.95,
  "AlwaysOnTop": true,
  "AutoRun": false,
  "Theme": "System",
  "Notes": "İş listesi:
- [x] EgeFire yap
- [ ] Test et",
  "CheckForUpdates": true,
  "LastUpdateCheck": "2026-11-03T12:00:00Z",
  "LastKnownVersion": "3.1.0",
  "MonitorPositions": [
    { "DeviceName": "\\.\DISPLAY1", "X": 100, "Y": 100, "Width": 300, "Height": 400, "IsMaximized": false }
  ]
}
```

---

## ⌨️ Klavye Kısayolları

| Kısayol | İşlem |
|---------|-------|
| `Ctrl+Alt+D` | Pencere göster/gizle (global) |
| `Esc` | Pencereyi gizle (tray'e) |
| `Enter` (SearchBox) | Uygulama başlat / Web arama |
| `↑/↓` (SearchBox) | Liste gezinme |
| `Ctrl+N` | Yeni kısayol ekle (dialog) |

---

## 🏗️ Mimari

```
EgeFire/
├── App.xaml / App.xaml.cs           # Giriş noktası, Mutex (tek instance), tema bootstrap
├── MainWindow.xaml / .cs             # Ana pencere: custom chrome, search, notes, tray
├── Models/
│   ├── AppShortcut.cs               # Uygulama kısayolu modeli
│   ├── QuickAccessItem.cs           # Hızlı erişim klasörü modeli
│   ├── AppConfig.cs                 # Tüm ayarlar (JSON serialize)
│   └── MonitorWindowPosition.cs     # Monitör bazlı pencere durumu
├── Services/
│   ├── ConfigManager.cs             # Config okuma/yazma (C:\EasyTaskStarter)
│   ├── ProcessLauncher.cs           # Process.Start wrapper (UAC, args, url)
│   ├── StartupManager.cs            # Registry Run key (autorun)
│   ├── HotkeyListener.cs            # Global hotkey (RegisterHotKey P/Invoke)
│   ├── UpdateChecker.cs             # GitHub Releases API kontrol
│   └── ClipboardMonitor.cs          # Panoya kopyalanan yolu kısayola çevir
├── Helpers/
│   ├── WindowHelper.cs              # Opacity, AlwaysOnTop, DPI
│   ├── TrayIcon.cs                  # NotifyIcon + ContextMenu (WndProc hook)
│   ├── DragDropHelper.cs            # ListBox sürükle-bırak sıralama
│   ├── IconHelper.cs                # ExtractAssociatedIcon
│   └── ShortcutResolver.cs          # .lnk hedefini çöz
├── Dialogs/
│   ├── AddAppDialog.xaml/.cs        # Kısayol ekleme (Gözat, auto-name)
│   ├── EditQuickAccessDialog.xaml/.cs
│   ├── HotkeyDialog.xaml/.cs        # Hotkey ayarlama (Key+Modifiers)
│   ├── AboutDialog.xaml/.cs         # Versiyon, kısayol rehberi, linkler
│   └── NotesDialog.xaml/.cs         # Scratchpad not penceresi
├── Themes/
│   ├── LightTheme.xaml              # 40+ ResourceDictionary brush key
│   └── DarkTheme.xaml               # Koyu varyant
├── EgeFireIcon.ico                  # Uygulama ikonu (kelepçe)
└── LeonDesk.csproj                  # .NET 10 WPF, UseWindowsForms=true
```

---

## 🛠️ Geliştirme

### Derleme
```bash
dotnet build LeonDesk.csproj
```

### Çalıştırma
```bash
dotnet run --project LeonDesk.csproj
# Veya gizli başlatma (autorun):
dotnet run --project LeonDesk.csproj -- --autorun
```

### Yayınlama (Self-contained, Single-file)
```bash
dotnet publish LeonDesk.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

### Hata Ayıklama
- Crash log: `%APPDATA%\EasyTaskStarter\error.log`
- `App.xaml.cs` → `DispatcherUnhandledException` handler

---

## 📄 Lisans

MIT License — Ayrıntılar için [LICENSE](LICENSE) dosyasına bakın.

---

## 🤝 Katkı

1. Fork edin
2. Feature branch: `git checkout -b feature/amazing-feature`
3. Commit: `git commit -m "feat: amazing feature"`
4. Push: `git push origin feature/amazing-feature`
5. Pull Request açın

---

## 📞 İletişim

**EgeLeon Bilişim** — Murat Serhan Gülhan  
📧 iletisim@egeleon.com  
🐙 [github.com/mserhangulhan](https://github.com/mserhangulhan)

---

## 🙏 Teşekkürler

- [.NET](https://dotnet.microsoft.com/) / [WPF](https://github.com/dotnet/wpf)
- [Newtonsoft.Json](https://www.newtonsoft.com/json)
- [Microsoft.Web.WebView2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)
- GitHub Actions / Releases (CI/CD)

---

> **Not**: Proje içi namespace `EasyTaskStarter` olarak kalır, sadece görünen uygulama adı **EgeFire**'dir.
