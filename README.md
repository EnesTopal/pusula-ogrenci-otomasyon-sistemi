# Pusula – Öğrenci Otomasyon Sistemi

Bu proje, temel kullanıcı ve ders yönetimi işlevlerini barındıran basit bir **öğrenci otomasyon sistemi**dir.  
Amaç; kullanıcı rolleri (Admin, Öğretmen, Öğrenci) üzerinden ders, not ve devamsızlık süreçlerini yönetebilmektir.

> **Deploy:**  <a href="http://51.20.188.237:8081/" target="_blank">http://51.20.188.237:8081/</a>

## Tech Stack
- **Backend:** .NET 9 + Entity Framework Core  
- **Frontend:** Blazor   
- **Veritabanı:** PostgreSQL  
- **Versiyon Kontrol:** GitHub
- **Deploy** AWS 

## Özellikler (Uygulanan Gereksinimler)
- **Kullanıcı Yönetimi**
  - Admin; öğretmen, öğrenci ve ders oluşturabilir, güncelleyebilir.
  - Admin; derslere öğretmen atayabilir.
- **Öğrenciler**
  - Öğrenci kendi profilini görüntüleyebilir.
- **Öğretmenler**
  - Öğretmen, kendi dersleri için:
    - Not girişi
    - Yorum ekleme
    - Devamsızlık kaydı
    - Öğrencileri derse ekleme/çıkarma işlemleri yapabilir.

## Kurulum
Github Linki ile kullandığınız uygulama üzerinden kopyalayabilir veya termianlden
   ```bash
   git clone https://github.com/EnesTopal/pusula-ogrenci-otomasyon-sistemi.git
   ```
   komutu ile projeyi kopyalayın.

## Çalıştırma
1. PostgreSQL çalışır durumda olmalı; gerekirse `Pusula.Api/appsettings*.json` bağlantı dizesini güncelleyin.
2. Çözüm klasöründen:
   ```bash
   # API
   dotnet run --project Pusula.Api

   # UI
   dotnet run --project Pusula.UI
   ```
   İle proje çalışır duruma gelmeli.


## Test Kullanıcı Bilgileri
Aşağıda 3 farklı rol türünde de kullanıcıların hesap bilgileri verilmiştir.

- **Admin**
  - Email: ...
  - Password: ...

- **Teacher**
  - Email: ...
  - Password: ...

- **Student**
  - Email: ...
  - Password: ...


## Yapılmış olan bonus görevler

- Kullanıcı arama ve filtreleme özelliği eklemek: Öğretmen/Admin not-devamsızlık-not sayfalarında öğrenci seçerek ona ait bilgileri görebilir.
- UI/UX düzenlemeleri: Responsive sayfa tasarımı yapıldı.
- GPA Grafikleri: Öğrenci ana sayfasında ders bazında ortalama notları gösteren interaktif bar grafik eklendi (Alt başlangıç çizgisi doğru ayarlanamadığı için sütun beklenenden yüksekte gözüküyor).
  


