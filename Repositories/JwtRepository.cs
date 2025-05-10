using AutoMapper;
using DemoFYP.EF;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace DemoFYP.Repositories
{
    public class JwtRepository : IJwtRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IMapper _mapper;
        public JwtRepository(IDbContextFactory<AppDbContext> factory, IMapper mapper) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task AddUserToken(Usertoken userToken)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var newData = _mapper.Map<Usertoken>(userToken);

                await context.Usertokens.AddAsync(newData);
                await context.SaveChangesAsync();
            }
            catch (Exception ex) {
                throw new InvalidOperationException("Insert token failed");
            }
        }
    }
}
