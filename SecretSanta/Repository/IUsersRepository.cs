using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecretSanta.Models;

namespace SecretSanta.Repository
{
    public interface IUsersRepository : IRepository<User, string>
    {
        Task RegisterUserAsync(User user);
        Task<bool> userExistsAsync(string username);
        Task<bool> passwordsMatchAsync(string username, string password);
        Task signInUserAsync(string username, string guid);
        Task<bool> isUserSignedInAsync(string username);
        Task<bool> isGuidPresentAsync(string guid);
        Task deleteSignedInUserAsync(string username);
        Task<string> getGuidForSignedInUserAsync(string username);
        Task<IEnumerable<UsersVM>> getListOfUsers(string v1, int skip, int take, string v2, string v3);
        Task<string> getUsernameByAuthTokenAsync(string authToken);
    }
}
