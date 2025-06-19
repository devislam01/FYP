using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Services.IServices;
using Microsoft.Extensions.Options;

namespace DemoFYP.Services
{
    public class CommonService : ICommonServices
    {
        private readonly IWebHostEnvironment _environment;
        private readonly Cloudinary _cloudinary;

        public CommonService(IWebHostEnvironment environment, IOptions<CloudinarySettings> config) {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            var account = new Account(
                   config.Value.CloudName,
                   config.Value.ApiKey,
                   config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }

        #region Upload Image

        public async Task<string> UploadImage(IFormFile file, string fileName, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("No file received");

            try
            {
                using var stream = file.OpenReadStream();
                var safefileName = string.IsNullOrWhiteSpace(fileName) ? file.FileName : fileName;

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(safefileName, stream),
                    Folder = folderName,
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                    throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");

                return uploadResult.SecureUrl.AbsoluteUri;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to upload to Cloudinary. " + ex.Message);
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
