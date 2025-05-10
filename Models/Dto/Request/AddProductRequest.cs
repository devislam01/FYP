using System.ComponentModel.DataAnnotations;

namespace DemoFYP.Models.Dto.Request
{
    public class AddProductRequest
    {
        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        public int CategoryID { get; set; } = 0;

        public string ProductCondition { get; set; }

        public string ProductImage { get; set; }

        public double ProductPrice { get; set; }

        public int StockQty { get; set; } = 1;

        public sbyte IsActive { get; set; } = 1;

        public byte[] UserID { get; set; }
    }
}
