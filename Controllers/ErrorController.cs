using Microsoft.AspNetCore.Mvc;
using BilgisayarMuhendisligiTasarimi.Models;
using BilgisayarMuhendisligiTasarimi.Services;

namespace BilgisayarMuhendisligiTasarimi.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILoggerService _logger;

        public ErrorController(ILoggerService logger)
        {
            _logger = logger;
        }

        [Route("Error/{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            var errorViewModel = new ErrorViewModel
            {
                StatusCode = statusCode,
                Title = statusCode switch
                {
                    404 => "Sayfa Bulunamadı",
                    403 => "Erişim Reddedildi",
                    500 => "Sistem Hatası",
                    400 => "Hatalı İstek",
                    401 => "Yetkisiz Erişim",
                    _ => "Sistem Hatası"
                },
                Description = statusCode switch
                {
                    404 => "Aradığınız sayfa taşınmış, silinmiş veya geçici olarak kullanım dışı olabilir.",
                    403 => "Bu sayfaya erişim yetkiniz bulunmamaktadır. Lütfen yöneticinize danışınız!",
                    500 => "İşleminiz gerçekleştirilirken bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.",
                    400 => "Gönderdiğiniz istek anlaşılamadı veya hatalı.",
                    401 => "Bu işlemi yapabilmek için giriş yapmanız gerekmektedir.",
                    _ => "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."
                }
            };

            if(statusCode >= 500 || statusCode == 429)
            {
                // Log the error
                _logger.LogError(
                    new Exception($"HTTP Error {statusCode} occurred. URL: {Request.Path}, Method: {Request.Method}"),
                    "Error",
                    "HandleError",
                    $"HTTP Error {statusCode} occurred. URL: {Request.Path}, Method: {Request.Method}"
                );
            }

            Response.StatusCode = statusCode;
            return View("Error", errorViewModel);
        }
    }
} 