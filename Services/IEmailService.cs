using System.Threading.Tasks;

namespace BilgisayarMuhendisligiTasarimi.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendTaskAssignmentEmailAsync(string to, string userName, string taskTitle, string creatorName);
        Task SendTaskStatusChangeEmailAsync(string to, string userName, string taskTitle, string newStatus);
    }
}
