namespace DemoFYP.Models.Dto.Response
{
    public class JwtAuthResult
    {
        public string accessToken {  get; set; }
        public string RefreshToken {  get; set; }
    }

    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime Expiry { get; set; }
    }
}
