namespace BilgisayarMuhendisligiTasarimi.Data.Entities
{
    public class Settings : BaseEntity
    {
        public string Logo { get; set; }
        public string Icon { get; set; }
        public string Slogan { get; set; }
        public string PhoneNumber { get; set; }
        public string? Email { get; set; }
    }
}
