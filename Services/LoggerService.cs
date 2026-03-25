using System.Text;

namespace BilgisayarMuhendisligiTasarimi.Services
{
    public interface ILoggerService
    {
        void LogError(Exception ex, string controller = "", string action = "", string additionalInfo = "");
    }

    public class LoggerService : ILoggerService
    {
        private readonly string _logPath;
        private readonly object _lockObj = new object();

        public LoggerService(IWebHostEnvironment env)
        {
            _logPath = Path.Combine(env.ContentRootPath, "wwwroot/Logs");
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
        }

        public void LogError(Exception ex, string controller = "", string action = "", string additionalInfo = "")
        {
            var logBuilder = new StringBuilder();
            logBuilder.AppendLine($"[ERROR] - {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            logBuilder.AppendLine($"Exception Type: {ex.GetType().Name}");
            logBuilder.AppendLine($"Message: {ex.Message}");
            
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                logBuilder.AppendLine($"Additional Info: {additionalInfo}");
            }

            if (ex.InnerException != null)
            {
                logBuilder.AppendLine($"Inner Exception: {ex.InnerException.Message}");
            }
            logBuilder.AppendLine(new string('-', 50));

            WriteToFile(logBuilder.ToString(), "error");
        }

        private void WriteToFile(string logMessage, string type)
        {
            var fileName = "ERROR.txt";
            var filePath = Path.Combine(_logPath, fileName);

            lock (_lockObj)
            {
                try
                {
                    const long maxFileSize = 10 * 1024 * 1024 ; // 10MB

                    if (File.Exists(filePath))
                    {
                        var fileInfo = new FileInfo(filePath);
                        if (fileInfo.Length > maxFileSize)
                        {
                            string content = File.ReadAllText(filePath);
                            
                            int charsToRemove = (int)(content.Length * 0.5);
                            content = content.Substring(charsToRemove);
                            
                            File.WriteAllText(filePath, content);
                        }
                    }

                    // Yeni log mesajını ekle
                    File.AppendAllText(filePath, logMessage + Environment.NewLine);
                }
                catch
                {
                    // Logging failed - could add fallback logging here
                }
            }
        }
    }
} 