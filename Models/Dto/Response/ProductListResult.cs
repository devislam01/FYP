namespace DemoFYP.Models.Dto.Response
{
    public class ProductListResult : BasePagination
    {
        public int ProductID {  get; set; }
        public string ProductName {  get; set; }
        public string ProductDescription { get; set; }
        public int CategoryID { get; set; }
        public string ProductCondition {  get; set; }
        public string ProductImage { get; set; }
        public double ProductPrice { get; set; }

    }

    public class FilteredProductListResult : BasePagination
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
}
