using AutoMapper;
using DemoFYP.EF;
using DemoFYP.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace DemoFYP.Repositories.IRepositories
{
    public class EmailRepository : IEmailRepositories
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IMapper _mapper;

        public EmailRepository(IDbContextFactory<AppDbContext> factory, IMapper mapper) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task InsertEmailLog(EmailLog emailLog)
        {
            var context = _factory.CreateDbContext();
            IDbContextTransaction tran = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                await context.EmailLogs.AddAsync(emailLog);
                await context.SaveChangesAsync();
                await tran.CommitAsync();
            }
            catch
            {
                if (tran != null)
                {
                    await tran.RollbackAsync();
                }
                throw;
            }
            finally
            {
                if (tran != null)
                {
                    await tran.DisposeAsync();
                }
            }
        }
    }
}
