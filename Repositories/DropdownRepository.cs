using AutoMapper;
using DemoFYP.EF;
using DemoFYP.Enums;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace DemoFYP.Repositories
{
    public class DropdownRepository : IDropdownRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IMapper _mapper;

        public DropdownRepository(IDbContextFactory<AppDbContext> factory, IMapper mapper) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task InsertPaymentMethod(PaymentMethodDropdownRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var newData = _mapper.Map<PaymentMethod>(payload);

                newData.CreatedAt = DateTime.Now;
                newData.CreatedBy = curUserID;

                await context.PaymentMethods.AddAsync(newData);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to Insert Payment Method", ex);
            }
            finally
            {
                await context.DisposeAsync();
            }
        }
    }
}
