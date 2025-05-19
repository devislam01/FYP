namespace DemoFYP.Models.Dto.Request
{
    public class PaymentMethodDropdownRequest
    {
        public string PaymentMethodName { get; set; } = null!;
        public bool IsActive { get; set; } = false;
    }
}
