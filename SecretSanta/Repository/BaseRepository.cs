using System;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace SecretSanta.Repository
{
    public class BaseRepository
    {
        private string connectionString { get; set; }

        public BaseRepository(IConfiguration Configuration)
        {
            connectionString = Configuration["DbConnectionString"];
        }

        protected MySqlConnection getConnection() 
        {
            return new MySqlConnection(connectionString);
        }
    }
}
