namespace DemoFYP.Models.Dto.Request
{
    public class ProductFilterRequest : BasePagination
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
    }
}
