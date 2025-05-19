using DemoFYP.Models.Dto.Request;

namespace DemoFYP.Services.IServices
{
    public interface IDropdownServices
    {
        Task InsertPaymentMethod(PaymentMethodDropdownRequest payload, Guid curUserID);
    }
}
