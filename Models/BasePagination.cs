namespace DemoFYP.Models
{
    public class BasePagination
    {
        public BasePagination()
        {
            PageNumber = 1;
            PageSize = 10;
        }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalRecord { get; set; }

        public int PageCount => (int)Math.Ceiling((double)TotalRecord / PageSize);

        public bool DisablePagination { get; set; } = false;
    }

    public class PaginationRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool DisablePagination { get; set; } = false;
    }

    public class PaginationResponse
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalRecord { get; set; }

        public int PageCount => (int)Math.Ceiling((double)TotalRecord / PageSize);
    }

    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public PaginationResponse Pagination { get; set; } = new();
    }

}
