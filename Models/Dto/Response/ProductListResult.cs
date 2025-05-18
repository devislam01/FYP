namespace DemoFYP.Models.Dto.Response
{
    public class ProductListResult
    {
        public int ProductID {  get; set; }
        public string ProductName {  get; set; }
        public string ProductDescription { get; set; }
        public int CategoryID { get; set; }
        public string ProductCondition {  get; set; }
        public string ProductImage { get; set; }
        public double ProductPrice { get; set; }

    }

    public class FilteredProductListResult
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public int CategoryID { get; set; }
        public string ProductCondition { get; set; }
        public string ProductImage { get; set; }
        public double ProductPrice { get; set; }
        public int StockQty { get; set; }
    }

    public class AdminProductListResult
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public int CategoryID { get; set; }
        public string ProductCondition { get; set; }
        public string? ProductImage { get; set; }
        public double ProductPrice { get; set; }
        public int StockQty { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public Guid BelongsTo { get; set; }
    }
}
