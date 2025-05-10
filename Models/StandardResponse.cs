namespace DemoFYP.Models
{
    public class StandardResponse
    {
        public object Data { get; set; } = null!;
        public int Code { get; set; } = 200;
        public string Status { get; set; } = null!;
        public string Message { get; set; } = null!;
    }

    public class StandardResponse<T> : StandardResponse
    {
        public StandardResponse(T data)
        {
            base.Data = data;
        }
    }
}
