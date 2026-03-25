using System.ComponentModel.DataAnnotations;

namespace BilgisayarMuhendisligiTasarimi.Models
{
    public class SettingsListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Logo")]
        public string Logo { get; set; }

        [Display(Name = "İkon")]
        public string Icon { get; set; }

        [Display(Name = "Slogan")]
        public string Slogan { get; set; }

        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; }

        [Display(Name = "E-posta")]
        public string? Email { get; set; }
    }

    public class UpdateSettingsModel
    {
        [Required(ErrorMessage = "ID alanı zorunludur.")]
        public int Id { get; set; }

        [Display(Name = "Logo")]
        public string? Logo { get; set; }

        [Display(Name = "İkon")]
        public string? Icon { get; set; }

        [Required(ErrorMessage = "Slogan alanı zorunludur.")]
        [Display(Name = "Slogan")]
        public string Slogan { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [MaxLength(15, ErrorMessage = "Telefon numarası en fazla 15 karakter olabilir.")]
        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [MaxLength(100, ErrorMessage = "E-posta adresi en fazla 100 karakter olabilir.")]
        [Display(Name = "E-posta")]
        public string? Email { get; set; }
    }
}
