namespace DemoFYP.Exceptions
{
    public class BusinessException : AppException
    {
        public BusinessException(string message = "Business Error")
           : base(message, 400) { }
    }
}
