namespace DemoFYP.Models.Dto.Request
{
    public class UserUpdateDetailRequest
    {
        public Guid? UserID { get; set; }

        public string? UserName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? UserGender { get; set; }

        public string? Address { get; set; }

        public IFormFile? QRCode { get; set; }

        public string? QRCodePath { get; set; }
    }

    public class UserDetail
    {
        public string? UserName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? UserGender { get; set; }

        public string? Address { get; set; }

        public IFormFile? QRCode { get; set; }

        public string? QRCodePath {  get; set; }
    }

    public class RevokeUserRequest
    {
        public Guid UserID { get; set; }
    }

    public class ReinstateUserRequest
    {
        public Guid UserID { get; set; }
    }

    public class ShoppingCartRequest
    {
        public int ProductID { get; set; }

        public int Quantity {  get; set; }
    }

    #region Admin Dto

    #endregion
}
