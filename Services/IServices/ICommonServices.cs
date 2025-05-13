namespace DemoFYP.Services.IServices
{
    public interface ICommonServices
    {
        Task<string> UploadImage(IFormFile file, string fileName);
        string HashPassword(string password);
        string GenerateTemporaryPassword(int length = 8);
    }
}
