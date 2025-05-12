namespace DemoFYP.Services.IServices
{
    public interface ICommonServices
    {
        Task<string> UploadImage(IFormFile file, string fileName);
    }
}
