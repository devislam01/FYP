using DemoFYP.Models.Dto.Request;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IDropdownRepository
    {
        Task InsertPaymentMethod(PaymentMethodDropdownRequest payload, Guid curUserID);
    }
}
