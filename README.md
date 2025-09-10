# Pusula – Öğrenci Otomasyon Sistemi

> **Deploy:** _link eklenecek_

## Tech Stack
- **Backend:** .NET 9, EF Core, Identity, PostgreSQL
- **Frontend:** Blazor Server

## Özellikler (Uygulanan Gereksinimler)
- **Kullanıcı Yönetimi:** Admin; öğretmen, öğrenci, ders,  oluşturabilir, güncelleyebilir. Derse öğretmen atayabilir.
- **Öğrenciler:** Öğrenci kendi profilini görüntüleyebilir
- **Öğretmenler:** Öğretmen, kendi dersi ile ilgili not girişi, yorum, devamsızlık gibi girişler sağlayabilir. 

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
