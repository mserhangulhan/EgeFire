# LeonDesk - Proje Dokümantasyonu

## Genel Bakış
LeonDesk, Windows için masaüstü kısayol yöneticisidir. Sık kullanılan uygulamaları ve klasörleri tek tıkla açmak için tasarlanmıştır. .NET 10 WPF ile yazılmıştır ve ismi **EasyTaskStarter** olarak yenilenmiştir.

## Mimari

### Proje Yapısı
```
EasyTaskStarter/
├── App.xaml / App.xaml.cs          - Uygulama giriş noktası, Mutex (tek instance)
├── MainWindow.xaml / .cs            - Ana pencere UI ve mantık
├── Models/
│   ├── AppShortcut.cs               - Uygulama kısayolu (Name, ExePath, Arguments)
│   ├── QuickAccessItem.cs           - Hızlı erişim klasörü (Name, Path)
│   └── AppConfig.cs                 - Tüm ayarlar (Shortcuts, QuickAccess, Hotkey, Opacity, AlwaysOnTop, Autorun, WindowPosition, WindowSize)
├── Services/
│   ├── ConfigManager.cs             - C:\EasyTaskStark\config.json okuma/yazma (AppData fallback)
│   ├── ProcessLauncher.cs           - Uygulama/klasör açma
│   ├── StartupManager.cs            - Registry başlangıç ayarı
│   └── HotkeyListener.cs            - Global hotkey (Ctrl+Alt+D)
├── Helpers/
│   ├── WindowHelper.cs              - Opacity ve Always-on-top ayarları
│   └── TrayIcon.cs                  - Sistem tepsisi (sağ tık menüsü)
├── Dialogs/
│   ├── AddAppDialog.xaml/.cs        - Kısayol ekleme (Gözat, otomatik ad)
│   └── HotkeyDialog.xaml/.cs        - Hotkey ayarı
└── EasyTaskStarterIcon.ico        - Uygulama ikonu (kelepçe)
```

### Config Dosyası
- Konum: `C:\EasyTaskStarter\config.json` (yazılamazsa `%APPDATA%\EasyTaskStarter\config.json`)
- İçerik: Shortcuts, QuickAccess, Hotkey, Opacity, AlwaysOnTop, Autorun, WindowPosition, WindowSize

### Önemli Davranışlar
1. **X butonu**: Pencereyi kapatmaz, sistem tepsisine küçültür (varsayılan davranış)
2. **Global Hotkey**: Ctrl+Alt+D → pencereyi göster/gizle
3. **Tray**: Çift tık → Aç, Sağ tık → menü (Aç/Kapat / Başlangıçta Çalış / Kapat)
4. **Tek Instance**: Mutex ile ikinci açılış engellenir
5. **Varsayılan**: Pencere üstte (Topmost=True), 300×400 boyut, Light tema

## Yapılanlar
- [x] Temel WPF projesi kuruldu
- [x] Config yönetimi (C:\EasyTaskStarter)
- [x] Kısayol ekleme/kaldırma
- [x] Hızlı erişim (Bu bilgisayar, Belgeler, Masaüstü, İndirilenler)
- [x] Global hotkey (ayarlanabilir)
- [x] Opacity slider
- [x] Always-on-top
- [x] Sistem tepsisi (tray icon)
- [x] Tek instance (Mutex)
- [x] Uygulama ikonu (kelepçe)
- [x] Boyut 300×400 (revize edildi)
- [x] "İzle" → "Gözat" + otomatik ad (revize edildi)
- [x] Windows Light tema (varsayılan renkler, sekmeler uyumlu)
- [x] Tray sağ tık menüsü: Aç / Başlangıçta Çalış / Kapat
- [x] Hata yönetimi ve graceful restart (Mutex, environment exit)

## Bilinen Sorunlar / Bekleyen
- [x] Başlangıç (autorun) ayarı: Tray sağ tık menüsünde "Başlangıçta Çalış" (checkbox)
- [x] Tray sağ tık menüsünde "Başlangıçta çalış" ayarı mevcut
- [x] Şeffaflık bug'ı çözüldü (OpacitySlider sadece mouse release'te değişir)
- [x] Karanlık mod → Light tema (SystemColors)
- [x] Çift tray icon sorunu çözüldü (silo UI + message window)
- [x] Başlangıç parametresi: `--autorun` ile gizli başlatma + Tray menüsünde "Başlangıçta Çalış"
- [x] Opacity bug'ı: `PreviewMouseUp` ile mouse release'te commit (ValueChanged yerine)

## Geliştirme Notları
- Build: `dotnet build`
- Test: `bin\Debug\net10.0-windows\EasyTaskStarter.exe`
- Crash debug: App.xaml.cs DispatcherUnhandledException log → `%APPDATA%\EasyTaskStarter\error.log`
- Çift tray icon öncesi: `NIM_DELETE` + `NIM_ADD` sırası, sahip pencere handle'ı
- `AllowsTransparency=True` + `WindowStyle=None` DWM'de pencereyi render etmiyor → kaldırıldı
- Config boyutu güncellenince eski config.json silinmeli (ApplyConfig Math.Max ile minimum garanti)
- Process çalışırken build edilemez (exe kilitli) → önce process'i kapat
- Icon: Embedded resource (LeonDeskIcon.ico → EasyTaskStarterIcon.ico)

## Kullanıcı İstenilenler (Revizyon Listesi)
1. ✅ Boyut 300×400
2. ✅ Karanlık mod → Normal Windows renkleri (Light tema)
3. ✅ Başlangıç parametresi: `--autorun` ile gizli başlatma + Tray menüsünde "Başlangıçta Çalış"
4. ✅ Şeffaflık bug'ı: `PreviewMouseUp` ile mouse release'te commit
5. ✅ Tray ikon + sağ tık menüsü: WndProc hook + ExtractAssociatedIcon
6. ✅ Ad değişikliği: LeonDesk → EasyTaskStarter (tüm dosyalar, registry, title, namespace)

## Sürüm Bilgisi
- Sürüm: 2.0 (Yeniden adlandırma ve tüm revizyonlar tamamlandı)
- Tarih: 27.08.2026
- Geliştirici: Murat