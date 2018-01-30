using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using SecretSanta.Models;

namespace SecretSanta.Repository
{
    public class UsersRepository : BaseRepository, IUsersRepository
    {
        private Encryptor Encryptor
        { get; set; }

        public UsersRepository(IConfiguration Configuration) : base(Configuration)
        {
            Encryptor = new Encryptor();
        }

        public async Task RegisterUser(User user)
        {
            using(var connection = getConnection()) 
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                var encryptedPassword = Encryptor.encryptPassword(user.Password);
                command.CommandText = "INSERT INTO Accounts(username, displayname, password)" +
                                       "VALUES (?username, ?displayname, ?password)";
                command.Parameters.Add("?username", MySqlDbType.VarChar).Value = user.Username;
                command.Parameters.Add("?displayname", MySqlDbType.VarChar).Value = user.DisplayName;
                command.Parameters.Add("?password", MySqlDbType.VarChar).Value = encryptedPassword;
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<bool> userExists(string username)
        {
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT username FROM Accounts";
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        string currentUsername = reader.GetString(0);
                        if (currentUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
               
            }
            return false;
        }

        public async Task<bool> passwordsMatch(string username, string password)
        {
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT password FROM Accounts WHERE username = @username";
                command.Parameters.AddWithValue("?username", username);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if(reader.Read())
                    {
                        var encryptedPassword = Encryptor.encryptPassword(password);
                        if (encryptedPassword.Equals(reader.GetString(0)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public async Task signInUser(string username, string guid)
        {
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO LoggedInUsers(username, guid) VALUES(?username, ?guid)";
                command.Parameters.Add("?username", MySqlDbType.VarChar).Value = username;
                command.Parameters.Add("?guid", MySqlDbType.VarChar).Value = guid;
                await command.ExecuteNonQueryAsync();
            }
        }

        async Task<List<SignedInUsersVM>> getAllSignedInUsers()
        {
            List<SignedInUsersVM> users = new List<SignedInUsersVM>();
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM LoggedInUsers";
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        users.Add(new SignedInUsersVM
                                {
                                    Username = reader.GetString(0),
                                    Guid = reader.GetString(1)
                                });
                    }
                }
                return users;
            }
        }

        public async Task<bool> isUserSignedIn(string username)
        {
            List<SignedInUsersVM> users = await getAllSignedInUsers();
            return users.Any(x => x.Username.Equals(username));
        }

        public async Task<bool> isGuidPresent(string guid)
        {
            List<SignedInUsersVM> users = await getAllSignedInUsers();
            return users.Any(x => x.Guid.Equals(guid));
        }

        public async Task deleteSignedInUser(string username)
        {
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "DELETE FROM LoggedInUsers WHERE username=?username";
                command.Parameters.Add("?username", MySqlDbType.VarChar).Value = username;
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
