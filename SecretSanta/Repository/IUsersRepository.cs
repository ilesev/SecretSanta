using System;
using System.Threading.Tasks;

namespace SecretSanta.Repository
{
    public interface IUsersRepository : IRepository<User, string>
    {
        Task RegisterUser(string username, string displayName, string password);
    }
}
