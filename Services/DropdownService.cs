using DemoFYP.Exceptions;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services.IServices;

namespace DemoFYP.Services
{
    public class DropdownService : IDropdownServices
    {
        private readonly IDropdownRepository _dropdownRepository;
        public DropdownService(IDropdownRepository dropdownRepository) { 
            _dropdownRepository = dropdownRepository ?? throw new ArgumentNullException(nameof(dropdownRepository));
        }

        public async Task InsertPaymentMethod(PaymentMethodDropdownRequest payload, Guid curUserID)
        {
            if (payload == null) throw new BadRequestException("Payload is required");
            if (string.IsNullOrEmpty(payload.PaymentMethodName)) throw new BadRequestException("Payment Method Name is required");
            
            try
            {
                await _dropdownRepository.InsertPaymentMethod(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }
    }
}
