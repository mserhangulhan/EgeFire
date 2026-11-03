# Katkıda Bulunma Rehberi

EgeFire projesine katkıda bulunmak isteyenler için teşekkürler! 🎉

## Başlamadan Önce

- [Issues](https://github.com/mserhangulhan/EgeFire/issues) sayfasını kontrol edin — hata/özellik zaten raporlanmış olabilir
- Büyük değişiklikler için önce issue açarak tartışın
- Kod stilini mevcut projeye uygun tutun

## Geliştirme Ortamı

### Gereksinimler
- Visual Studio 2022+ veya VS Code + C# Dev Kit
- .NET 10 SDK
- Windows 10/11 (WPF hedef platform)

### Kurulum
```bash
git clone https://github.com/mserhangulhan/EgeFire.git
cd EgeFire
dotnet restore
dotnet build LeonDesk.csproj
```

### Çalıştırma
```bash
dotnet run --project LeonDesk.csproj
# Gizli başlatma (autorun test):
dotnet run --project LeonDesk.csproj -- --autorun
```

## Kod Standartları

### C# Stil
- **Naming**: PascalCase (public), camelCase (private/local), `_camelCase` (fields)
- **using alias pattern** for ambiguous types (WinForms vs WPF):
  ```csharp
  using WpfClipboard = System.Windows.Clipboard;
  using WpfMessageBox = System.Windows.MessageBox;
  using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;
  ```
- **Async/await** — I/O işlemlerinde sync-over-async yok
- **Nullability** — `#nullable enable` proje seviyesinde açık

### XAML
- `x:Name` yerine `x:FieldModifier="private"` tercih edilmez, code-behind'da `FindName` kullanılmaz
- ResourceDictionary key'ler: PascalCase (örn. `WindowBackgroundBrush`)
- Event handler naming: `ElementName_EventName` (örn. `BtnTheme_Click`)

### Git Commit Mesajları
[Conventional Commits](https://www.conventionalcommits.org/tr/v1.0.0/) formatı:

```
<type>(<scope>): <açıklama>

[body]

[footer]
```

**Tipler**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `ci`

**Örnekler**:
```
feat(ui): add dark theme support
fix(config): resolve double tray icon bug
docs(readme): update web search shortcuts table
refactor(dragdrop): rewrite with WPF aliases
ci(release): add github actions workflow
```

## Pull Request Süreci

1. **Fork** edin ve feature branch oluşturun:
   ```bash
   git checkout -b feat/amazing-feature
   ```

2. **Değişiklikleri** yapın, test edin:
   ```bash
   dotnet build
   dotnet run --project LeonDesk.csproj
   ```

3. **Commit** atın (conventional commits):
   ```bash
   git add .
   git commit -m "feat(ui): add amazing feature"
   ```

4. **Push** yapın ve PR açın:
   ```bash
   git push origin feat/amazing-feature
   ```

5. PR şablonunu doldurun:
   - Ne değişti?
   - Nasıl test edildi?
   - Ekran görüntüleri (UI değişikliği varsa)
   - Breaking change var mı?

6. **Review** bekleyin — en az 1 onay gerekir

## Test Etme

### Manuel Test Senaryoları
- [ ] Uygulama başlatılıyor, tray'de görünüyor
- [ ] Global hotkey (Ctrl+Alt+D) pencereyi açıyor/kapatıyor
- [ ] Kısayol ekleme dialog'u çalışıyor (Gözat, auto-name)
- [ ] Kısayol başlatma (exe, args, url)
- [ ] Hızlı erişim klasörleri açılıyor
- [ ] Tema değiştirme (Light/Dark/System) anlık uygulanıyor ve kaydediliyor
- [ ] Opacity slider mouse release'te commit ediliyor
- [ ] Always-on-top toggle çalışıyor
- [ ] Not alma penceresi açılıyor, yazılıyor, kaydediliyor, kapanıp açıldığında duruyor
- [ ] Web arama kısayolları (g, yt, gh, so, wk) tarayıcıda açılıyor
- [ ] Çoklu monitörde pencere konumu hatırlanıyor
- [ ] X butonu → tray'e küçülme
- [ ] Tray menüsü: Aç/Gizle, Notlar, Başlangıçta Çalış, Hakkında, Kapat
- [ ] Autorun toggle (Registry) çalışıyor
- [ ] `--autorun` parametresiyle gizli başlatma
- [ ] Güncelleme kontrolü (banner görünür mü)
- [ ] Config dosyası `C:\EasyTaskStarter\config.json` oluşturuluyor

### Otomatik Testler
Henüz unit test projesi yok. Katkı olarak test projesi eklenebilir.

## Issue Raporlama

### Bug Report
Şablonu kullanın (`.github/ISSUE_TEMPLATE/bug_report.md`):
- Açıklama
- Yeniden üretim adımları
- Beklenen / Gerçek davranış
- Ekran görüntüsü / log
- Ortam: Windows sürümü, .NET sürümü, EgeFire versiyonu

### Feature Request
Şablonu kullanın (`.github/ISSUE_TEMPLATE/feature_request.md`):
- Problem ne?
- Önerilen çözüm
- Alternatifler
- Ek bağlam / mockup

## Kod İnceleme (Code Review) Kriterleri

- [ ] Kod derleniyor mu? (`dotnet build` exit 0)
- [ ] Uygulama çalışıyor mu? (manuel smoke test)
- [ ] Breaking change varsa: config migration / versiyon atlama notu var mı?
- [ ] Yeni string'ler hardcoded mi? (ResourceDictionary / localization için ayırt edilmeli)
- [ ] Ambiguous reference riski var mı? (WPF/WinForms çakışması)
- [ ] XAML parse hatası riski var mı? (& escape, namespace)
- [ ] Performans: UI thread bloklanmıyor mu? (async/await doğru mu?)
- [ ] Memory leak: Event handler unsubscribe, IDisposable?

## Sürüm Yayınlama (Maintainer)

```bash
# 1. Versiyon güncelle (AssemblyInfo.cs, CHANGELOG.md)
# 2. Tag oluştur
git tag v3.2.0
# 3. Push
git push origin v3.2.0
# 4. GitHub Actions workflow otomatik release oluşturur
```

## İletişim

Sorular için: [Issues](https://github.com/mserhangulhan/EgeFire/issues) veya iletisim@egeleon.com

---

**Teşekkürler!** Her katkı EgeFire'i daha iyi hale getirir. 🚀
