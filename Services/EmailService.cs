using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace BilgisayarMuhendisligiTasarimi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var settings = _configuration.GetSection("EmailSettings");
                var host = settings["SmtpHost"];
                var port = int.Parse(settings["SmtpPort"] ?? "587");
                var enableSsl = bool.Parse(settings["EnableSsl"] ?? "true");
                var userName = settings["SmtpUsername"];
                var password = settings["SmtpPassword"];
                var fromEmail = settings["FromEmail"];
                var fromName = settings["FromName"];

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(userName, password),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                _logger.LogInformation($"Sending email to {to} with subject '{subject}'");
                await client.SendMailAsync(mailMessage); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {to}");
            }
        }

        public async Task SendTaskAssignmentEmailAsync(string to, string userName, string taskTitle, string creatorName)
        {
            var baseUrl = _configuration["EmailSettings:BaseUrl"] ?? "https://localhost:7291";
            var body = GetPremiumTemplate("Yeni Görev Atandı", $@"
                <p>Merhaba <strong>{userName}</strong>,</p>
                <p>Sizin için yeni bir görev oluşturuldu ve atandı.</p>
                <div style='background: rgba(255,255,255,0.05); border-radius: 12px; padding: 20px; margin: 20px 0; border-left: 4px solid #4680ff;'>
                    <h3 style='margin: 0; color: #4680ff;'>{taskTitle}</h3>
                    <p style='margin: 10px 0 0 0; font-size: 14px; opacity: 0.8;'>Atayan: {creatorName}</p>
                </div>
                <p>Görevin detaylarını görmek ve çalışmaya başlamak için paneli ziyaret edebilirsiniz.</p>
                <div style='text-align: center; margin-top: 30px;'>
                    <a href='{baseUrl}/Panel' style='background: linear-gradient(135deg, #4680ff 0%, #3f6ad8 100%); color: white; padding: 12px 30px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block; box-shadow: 0 4px 15px rgba(70, 128, 255, 0.3);'>Paneli Aç</a>
                </div>
            ");

            await SendEmailAsync(to, $"Yeni Görev: {taskTitle}", body);
        }

        public async Task SendTaskStatusChangeEmailAsync(string to, string userName, string taskTitle, string newStatus)
        {
            var baseUrl = _configuration["EmailSettings:BaseUrl"] ?? "https://localhost:7291";
            var statusColor = newStatus == "Done" ? "#2ed8b6" : "#4680ff";
            var body = GetPremiumTemplate("Görev Durumu Güncellendi", $@"
                <p>Merhaba <strong>{userName}</strong>,</p>
                <p>Takip ettiğiniz bir görevin durumu güncellendi.</p>
                <div style='background: rgba(255,255,255,0.05); border-radius: 12px; padding: 20px; margin: 20px 0; border-left: 4px solid {statusColor};'>
                    <h3 style='margin: 0; color: {statusColor};'>{taskTitle}</h3>
                    <p style='margin: 10px 0 0 0; font-size: 14px;'>Yeni Durum: <span style='background: {statusColor}; color: white; padding: 2px 8px; border-radius: 4px; font-size: 12px;'>{newStatus}</span></p>
                </div>
                <p>Görevin son durumunu incelemek için paneli ziyaret edebilirsiniz.</p>
                <div style='text-align: center; margin-top: 30px;'>
                    <a href='{baseUrl}/Panel/Board' style='background: linear-gradient(135deg, #4680ff 0%, #3f6ad8 100%); color: white; padding: 12px 30px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block; box-shadow: 0 4px 15px rgba(70, 128, 255, 0.3);'>Detayları Gör</a>
                </div>
            ");

            await SendEmailAsync(to, $"Görev Güncellemesi: {taskTitle}", body);
        }

        private string GetPremiumTemplate(string title, string content)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {{ font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; line-height: 1.6; color: #e1e1e1; background-color: #0f172a; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 40px 20px; }}
                    .card {{ background: #1e293b; border-radius: 20px; padding: 40px; box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04); border: 1px solid rgba(255,255,255,0.05); }}
                    .header {{ text-align: center; margin-bottom: 30px; }}
                    .logo {{ font-size: 24px; font-weight: 800; background: linear-gradient(135deg, #4680ff 0%, #2ed8b6 100%); -webkit-background-clip: text; -webkit-text-fill-color: transparent; margin-bottom: 10px; display: inline-block; }}
                    h1 {{ color: #ffffff; font-size: 22px; margin-bottom: 20px; font-weight: 700; text-align: center; }}
                    p {{ margin-bottom: 15px; color: #94a3b8; }}
                    .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #64748b; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div class='logo'>STUDIO BOARDS</div>
                    </div>
                    <div class='card'>
                        <h1>{title}</h1>
                        {content}
                    </div>
                    <div class='footer'>
                        &copy; {DateTime.Now.Year} Studio Boards. Tüm hakları saklıdır.<br>
                        Bu bir sistem bildirimidir, lütfen yanıtlamayınız.
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}
