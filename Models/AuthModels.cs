using System.ComponentModel.DataAnnotations;
using BilgisayarMuhendisligiTasarimi.Data.Entities;

namespace BilgisayarMuhendisligiTasarimi.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [MaxLength(100, ErrorMessage = "Email adresi en fazla 100 karakter olabilir.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [MaxLength(50, ErrorMessage = "Şifre en fazla 50 karakter olabilir.")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [MaxLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
        [Display(Name = "Ad Soyad")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [MaxLength(100, ErrorMessage = "Email adresi en fazla 100 karakter olabilir.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [MaxLength(50, ErrorMessage = "Şifre en fazla 50 karakter olabilir.")]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        [Display(Name = "Şifre Tekrar")]
        public string ConfirmPassword { get; set; }

        public RoleEnum Role { get; set; } = RoleEnum.User;
        
        [Display(Name = "Stüdyo Adı (Opsiyonel)")]
        [MaxLength(100, ErrorMessage = "Stüdyo adı en fazla 100 karakter olabilir.")]
        public string? StudioName { get; set; }
    }

    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [MaxLength(100, ErrorMessage = "Email adresi en fazla 100 karakter olabilir.")]
        public string Email { get; set; }
    }
} 