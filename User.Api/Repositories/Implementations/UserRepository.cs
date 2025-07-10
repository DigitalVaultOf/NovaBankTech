using Microsoft.EntityFrameworkCore;
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
            // await _context.SaveChangesAsync();
        }
        
        public async Task<Users> GetByIdAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task UpdateUserAsync(Users user)
        {
            _context.Users.Update(user);
            await Task.CompletedTask;
        }

        public async Task<Users?> GetUserByIdWithAccountsAsync(Guid userId)
        {
            return await _context.Users.Include(u => u.Accounts).FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<Users> GetUserByCpfAsync(string cpf)
        {
            return await _context.Users
                                 .Include(u => u.Accounts)
                                 .FirstOrDefaultAsync(u => u.Cpf == cpf);
        }

        public async Task<Users> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                                 .Include(u => u.Accounts) 
                                 .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
    
}
