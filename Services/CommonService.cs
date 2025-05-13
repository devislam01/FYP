using DemoFYP.Exceptions;
using DemoFYP.Services.IServices;

namespace DemoFYP.Services
{
    public class CommonService : ICommonServices
    {
        private readonly IWebHostEnvironment _environment;

        public CommonService(IWebHostEnvironment environment) {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        #region Upload Image

        public async Task<string> UploadImage(IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0) throw new BadRequestException(" No File received ");

            string directoryPath = Path.Combine(_environment.WebRootPath, "ProductImages");
            string safeFileName = string.IsNullOrEmpty(fileName) ? file.FileName : fileName;
            string filePath = Path.Combine(directoryPath, safeFileName);

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"ProductImages/{safeFileName}";
            }
            catch
            {
                throw new Exception("Failed to save file. Please try again.");
            }
        }

        #endregion

        #region Generate Password

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public string GenerateTemporaryPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion
    }
}
