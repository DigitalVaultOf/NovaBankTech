using User.Api.Model;

namespace User.Api.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task CreateUser(Users user);
    }
}
