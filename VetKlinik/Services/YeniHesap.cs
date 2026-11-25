namespace VetKlinik.Services
{
    // Hekim eklenirken/düzenlenirken aynı anda oluşturulacak giriş hesabı bilgisi (isteğe bağlı)
    public record YeniHesap(string Email, string Sifre);
}
