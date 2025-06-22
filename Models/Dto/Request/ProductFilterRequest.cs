namespace DemoFYP.Models.Dto.Request
{
    public class ProductFilterRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public bool DisablePagination { get; set; } = false;
        public string? Search { get; set; }
        public int[]? CategoryId { get; set; }
    }

    public class AdminProductFilterRequest : PaginationRequest
    {
        public int? ProductID { get; set; }
        public string? ProductName { get; set; }
        public int? CategoryID { get; set; }
        public string? ProductCondition { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsActive { get; set; } = true;
        public Guid? BelongsTo { get; set; }
    }
}
