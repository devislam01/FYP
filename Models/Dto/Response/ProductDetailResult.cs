namespace DemoFYP.Models.Dto.Response
{
    public class ProductDetailResult
    {
        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        public string CategoryName { get; set; }

        public string ProductCondition { get; set; }

        public string ProductImage { get; set; }

        public double ProductPrice { get; set; }

        public double ProductRating { get; set; }

        public int StockQty { get; set; } = 1;
    }
}
