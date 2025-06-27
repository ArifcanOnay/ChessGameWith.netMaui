# Satranç Oyunu (.NET MAUI & Web API)

Bu proje, .NET MAUI ile geliştirilmiş, çok katmanlı mimariye sahip modern bir satranç oyunudur. Hem masaüstü hem de mobil cihazlarda çalışır. Oyun motoru ve veri yönetimi için .NET Web API kullanılmıştır.

## Özellikler

- **Gerçek Zamanlı Satranç Tahtası:** 8x8 grid üzerinde taşların canlı takibi ve hamle yapabilme
- **Kullanıcı Yönetimi:** Kayıt olma, giriş/çıkış, şifre değiştirme
- **Oyun Yönetimi:** Yeni oyun başlatma, hamle yapma, piyon terfisi, rok, şah-mat ve pat kontrolü
- **Hamle Geçmişi:** Oyun ve hamle geçmişini görüntüleme
- **Güvenlik:** Şifre değiştirme sonrası otomatik çıkış, temel doğrulama
- **Modern Arayüz:** Kullanıcı dostu ve responsive tasarım

## Proje Yapısı

```
SatrancOyunu/
├── SatrancApi/           # .NET Web API (Oyun motoru, servisler, veri tabanı)
├── SatranOyunumApp/      # .NET MAUI istemci uygulaması (UI, kullanıcı işlemleri)
```

### Katmanlar

- **API Katmanı:** Oyun kuralları, hamle doğrulama, kullanıcı işlemleri, veri tabanı yönetimi
- **Uygulama Katmanı:** Kullanıcı arayüzü, API ile iletişim, veri bağlama (binding)

## Kurulum

### 1. Veritabanı

- SQL Server'da `SatrancPlayDb` isimli bir veritabanı oluşturun.
- Migration ve tablolar ilk çalıştırmada otomatik oluşacaktır.

### 2. API'yi Başlatın

```bash
cd SatrancOyunu/SatrancApi
dotnet run
```

### 3. MAUI Uygulamasını Başlatın

```bash
cd SatrancOyunu/SatranOyunumApp
dotnet build
dotnet maui run
```

## Kullanım

1. Uygulamayı açın, kayıt olun veya giriş yapın.
2. Yeni oyun başlatabilir, hamle yapabilir, geçmiş oyunlarınızı görebilirsiniz.
3. Şifrenizi değiştirebilir veya çıkış yapabilirsiniz.

## Önemli Dosyalar

- **SatrancApi/Program.cs:** API endpoint'leri ve iş mantığı
- **SatranOyunumApp/Views/GamePage.xaml:** Satranç tahtası ve taşların görselleştirilmesi
- **SatranOyunumApp/Views/SifreDegistirPage.xaml.cs:** Şifre değiştirme işlemleri
- **SatranOyunumApp/Services/SatrancApiService.cs:** API ile iletişim servisleri

## Katkı ve Lisans

Bu proje eğitim amaçlıdır. Katkıda bulunmak için pull request gönderebilirsiniz.

---

**Hazırlayan:**  
Arif  
2025
