using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecretSanta.Models;

namespace SecretSanta.Repository
{
    public interface IUsersRepository : IRepository<User, string>
    {
        Task RegisterUser(User user);
        Task<bool> userExists(string username);
        Task<bool> passwordsMatch(string username, string password);
        Task signInUser(string username, string guid);
        Task<bool> isUserSignedIn(string username);
        Task<bool> isGuidPresent(string guid);
        Task deleteSignedInUser(string username);
    }
}
