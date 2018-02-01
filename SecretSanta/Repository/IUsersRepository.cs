using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecretSanta.Models;

namespace SecretSanta.Repository
{
    public interface IUsersRepository : IRepository<User, string>
    {
        Task RegisterUserAsync(User user);
        Task<bool> UserExistsAsync(string username);
        Task<bool> PasswordsMatchAsync(string username, string password);
        Task SignInUserAsync(string username, string guid);
        Task<bool> IsUserSignedInAsync(string username);
        Task<bool> IsGuidPresentAsync(string guid);
        Task DeleteSignedInUserAsync(string username);
        Task<string> GetGuidForSignedInUserAsync(string username);
        Task<IEnumerable<UsersVM>> GetListOfUsers(string v1, int skip, int take, string v2, string v3);
        Task<string> GetUsernameByAuthTokenAsync(string authToken);
    }
}
