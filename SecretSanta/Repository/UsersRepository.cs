using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace SecretSanta.Repository
{
    public class UsersRepository : BaseRepository, IUsersRepository
    {
        public UsersRepository(IConfiguration Configuration) : base(Configuration)
        {
        }

        public async Task RegisterUser(string username, string displayName, string password)
        {
            var connection = getConnection();
            MySqlCommand cmd = new MySqlCommand();
            await cmd.ExecuteReaderAsync();
        }
    }
}
