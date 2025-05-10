namespace DemoFYP.Exceptions
{
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "You have no access to this.")
            : base(message, 403) { }
    }
}
