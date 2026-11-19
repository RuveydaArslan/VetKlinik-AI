namespace VetKlinik.Data
{
    // Rol adlarını tek bir yerde sabit olarak tutuyoruz.
    // Böylece [Authorize(Roles = Roles.Admin)] gibi yerlerde yazım hatası yapılmaz.
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Veteriner = "Veteriner";
        public const string Uye = "Uye";

        // Seed sırasında tüm rolleri dolaşmak için
        public static readonly string[] Tumu = { Admin, Veteriner, Uye };
    }
}
