namespace DemoFYP.Models.Dto.Request
{
    public class ProductFilterRequest : PaginationRequest
    {
        public string? Search { get; set; }
        public int[]? CategoryId { get; set; }
    }
}
