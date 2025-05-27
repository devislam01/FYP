namespace DemoFYP.Models.Dto.Response
{
    public class ProductDetailResult
    {
        public int ProductID {  get; set; }
        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        public int CategoryID { get; set; }

        public string CategoryName { get; set; }

        public string ProductCondition { get; set; }

        public string ProductImage { get; set; }

        public double ProductPrice { get; set; }

        public double ProductRating { get; set; }

        public int StockQty { get; set; } = 1;
    }

    public class SellerDetailResult
    {
        public Guid SellerID { get; set; }
        public string SellerName { get; set; }
        public double? RatingMark { get; set; }
        public int CompletedOrders { get; set; }
        public string JoinTime { get; set; }
    }

    public class ProductDetailResponse
    {
        public ProductDetailResult ProductDetail { get; set; }
        public SellerDetailResult SellerDetail { get; set; }
    }
}
