using System.ComponentModel.DataAnnotations;

namespace DemoFYP.Models.Dto.Request
{
    public class AddProductRequest
    {
        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        public int CategoryID { get; set; } = 0;

        public string ProductCondition { get; set; }

        public IFormFile ProductImage { get; set; }

        public double ProductPrice { get; set; }

        public int StockQty { get; set; } = 1;

        public sbyte IsActive { get; set; } = 1;

        public Guid? UserID { get; set; }
    }

    public class UpdateProductRequest
    {
        public int ProductID { get; set; }

        public string? ProductName { get; set; }

        public string? ProductDescription { get; set; }

        public int? CategoryID { get; set; }

        public string? ProductCondition { get; set; }

        public IFormFile? ProductImageFile { get; set; }
        public string? ProductImageUrl { get; set; } 

        public double? ProductPrice { get; set; }

        public int? StockQty { get; set; }

        public sbyte? IsActive { get; set; }

        public Guid? UserID { get; set; }
    }

    public class DeleteProductRequest
    {
        public int ProductID { get; set; }
    }

    public class PublishProductRequest
    {
        public int ProductID { get; set; }
    }
}
