using User.Api.Data;
using User.Api.Model;
using User.Api.Repositories.Interfaces;

namespace User.Api.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task CreateUser(Users user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
    
}
