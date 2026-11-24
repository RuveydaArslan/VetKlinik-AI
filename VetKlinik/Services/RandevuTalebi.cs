namespace VetKlinik.Services
{
    // Randevu oluşturmak için gereken bilgiler.
    // MVC formu, REST API ve MCP aracı bu nesneyi doldurup AppointmentService.CreateAsync'e verir.
    public class RandevuTalebi
    {
        public int PetId { get; set; }
        public int VeterinarianId { get; set; }
        public int ServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public string? OwnerNote { get; set; }
    }
}
