namespace DemoFYP.Models.Dto.Response
{
    public class UserDetailResponse
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string UserGender { get; set; }

        public string Address { get; set; }

        public string ResidentialCollege { get; set; }

        public string PaymentQRCode {  get; set; }
    }

    public class UserJwtClaims
    {
        public Guid UserID { get; set; }
        public string Role {  get; set; }
        public string Email { get; set; }
        public List<string> Permissions { get; set; }
    }

    public class UserPermissionResponse
    {
        public List<string> Permissions { get; set; }
    }

    public class ShoppingCartObj
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductCondition { get; set; }
        public double ProductPrice { get; set; }
        public int Quantity { get; set; }
        public string ProductImage { get; set; }
        public string CategoryName { get; set; }
    }

    #region AdminDto

    public class UserListResponse
    {
        public Guid UserID { get; set; }
        public string Email { set; get; } = null!;
        public double? Ratings { get; set; }
        public string? UserName { get; set; }   
        public string? Gender {  get; set; }
        public string? PhoneNumber { set; get; }
        public string Status {  get; set; } = null!;
        public string? QRCode {  set; get; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy {  get; set; }
    }

    #endregion
}
