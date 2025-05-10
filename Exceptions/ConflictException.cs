namespace DemoFYP.Exceptions
{
    public class ConflictException : AppException
    {
        public ConflictException(string message = "Conflict occurred.")
            : base(message, 409) { }
    }
}
