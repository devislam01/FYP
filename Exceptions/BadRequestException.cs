namespace DemoFYP.Exceptions
{
    public class BadRequestException : AppException
    {
        public BadRequestException(string message = "Bad Request.")
            : base(message, 400) { }
    }
}
