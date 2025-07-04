﻿using User.Api.Model;

namespace User.Api.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task CreateUser(Users user);
        Task UpdateUser(Users user);
        Task<Users> GetByIdAsync(Guid id);
        Task<Users> GetUserByIdWithAccountsAsync(Guid userId);
    }
}
