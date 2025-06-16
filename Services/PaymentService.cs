using DemoFYP.Enums;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services.IServices;

namespace DemoFYP.Services
{
    public class PaymentService : IPaymentServices
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICommonServices _commonServices;

        public PaymentService(IPaymentRepository paymentRepository, ICommonServices commonServices) {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
        }

        public async Task<PagedResult<PaymentListResponse>> GetPaymentList(PaymentListFilterRequest filter)
        {
            try
            {
                return await _paymentRepository.GetPaymentList(filter);
            }
            catch
            {
                throw;
            }
        }

        public async Task<PaymentListResponse> GetPaymentDetail(int paymentID)
        {
            return await _paymentRepository.GetPaymentDetail(paymentID);
        }

        public async Task UpdatePayment(UpdatePaymentRequest payload, Guid curUserID)
        {
            try
            {
                if (payload.ReceiptFile != null && payload.ReceiptFile.Length > 0)
                {
                    payload.Receipt = await _commonServices.UploadImage(payload.ReceiptFile, "", FolderName.Receipt.ToString());
                }

                await _paymentRepository.UpdatePayment(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }
    }
}
