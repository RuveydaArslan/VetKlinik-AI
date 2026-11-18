# Yapay Zeka Destekli Veteriner Kliniği Randevu ve Hasta Takip Sistemi

**Öğrenci No:** B241210100<br>
**Ad Soyad:** Rüveyda ARSLAN<br>
**Ders Grubu:** 2. Öğretim A Grubu<br>

---

## Proje Hakkında

**VetKlinik-AI**, ASP.NET Core MVC ile geliştirilen bir veteriner kliniği randevu ve hasta takip sistemidir. Uygulama ile:

- Veteriner kliniklerinin çalışma saatleri ve sundukları hizmetler (muayene, aşı, kısırlaştırma, diş temizliği, tıraş ve bakım vb.) süre ve ücretleriyle birlikte yönetilebilir.
- Veteriner hekimler uzmanlık alanları, verebildikleri hizmetler ve müsaitlik saatleriyle sisteme tanımlanabilir.
- Hayvan sahipleri sisteme üye olup birden fazla evcil hayvan kaydedebilir; her hayvanın aşı geçmişi ve muayene notları takip edilebilir.
- Hayvan sahipleri hekimin müsaitliğine ve mevcut randevulara göre çakışma kontrolü yapılan randevular alabilir; randevular Admin veya ilgili veteriner tarafından onaylanır ya da reddedilir.

Uygulama ayrıca bir **MCP (Model Context Protocol) sunucusu** içerir. Böylece bir yapay zeka asistanı uygulamanın işlevlerini araç olarak çağırabilir. Örneğin hayvan sahibi asistana *"Pamuk'un bir sonraki aşısı ne zaman?"* diye sorduğunda cevap doğrudan bu uygulamadan alınır.

## Planlanan Özellikler

- [ ] Klinik, hizmet ve çalışma saati yönetimi
- [ ] Veteriner hekim, uzmanlık alanı ve müsaitlik yönetimi
- [ ] Üyelik sistemi ve evcil hayvan kaydı
- [ ] Aşı kayıtları ve muayene notları
- [ ] Çakışma kontrollü randevu sistemi ve onay mekanizması
- [ ] Rol bazlı yetkilendirme (Admin, Veteriner, Üye)
- [ ] LINQ sorguları kullanan REST API (DTO ile)
- [ ] Yapay zeka asistanları için MCP sunucusu

## Kullanılan Teknolojiler

- ASP.NET Core MVC (.NET 10 LTS), C#
- Entity Framework Core (Code First, Migration), LINQ
- SQL Server
- ASP.NET Core Identity
- ModelContextProtocol.AspNetCore (MCP C# SDK)
- Bootstrap 5, HTML5, CSS3, JavaScript, jQuery

## Proje Durumu

🚧 Geliştirme aşamasındadır. Rapor bölümleri (ER diyagramı, ekran görüntüleri, API uçları ve MCP araçları) proje ilerledikçe eklenecektir.