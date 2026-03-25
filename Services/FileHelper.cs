namespace BilgisayarMuhendisligiTasarimi.Services
{
    public class FileHelper
    {
        public static async Task<string> CreateFile(IFormFile file, string folder = "uploads")
        {
            var format = Path.GetExtension(file.FileName);
            var fileName = file.FileName.Replace(format, "").Replace(" ", "");
            var randomName = $"{fileName}_{Guid.NewGuid()}{format}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);
            
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var path = Path.Combine(folderPath, randomName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return randomName;
        }
        public static async Task<string> CreatePdfFile(IFormFile file)
        {
            var format = Path.GetExtension(file.FileName);
            var fileName = file.FileName.Replace(format, "");
            fileName = fileName.Replace(" ", "");
            var randomName = string.Format($"{fileName}_{Guid.NewGuid()}{format}");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\pdfs", randomName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return randomName;
        }
        public static async Task<string> ReplaceFile(string? oldImgName, IFormFile file)
        {
            if(oldImgName!= null && oldImgName != "no-image.png")
            {
                DeleteFile(oldImgName);
            }
            var ImgName = await CreateFile(file);
            return ImgName;
        }
        public static void DeleteFile(string ImgName)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/", ImgName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static void DeletePdfFile(string PdfName)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/pdfs/", PdfName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }


        public static bool CheckPdfFileType(IFormFile file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        // IFormFile'ı MemoryStream'e kopyala
                        file.CopyTo(stream);

                        // MemoryStream'den byte dizisine dönüştür
                        byte[] pdfData = stream.ToArray();

                        // İlk 4 byte'ı kontrol et
                        if (pdfData.Length >= 4 &&
                            pdfData[0] == 0x25 &&
                            pdfData[1] == 0x50 &&
                            pdfData[2] == 0x44 &&
                            pdfData[3] == 0x46)
                        {
                            return true; // Dosya bir PDF dosyasıdır
                        }
                    }
                }

                return false; // Dosya bulunamadı veya PDF dosyası değil
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return false;
            }
        }
    }
}
