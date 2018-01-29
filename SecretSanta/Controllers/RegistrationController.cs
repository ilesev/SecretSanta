using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace SecretSanta.Controllers
{
    public class RegistrationController : Controller
    {
        public String myConfig {get;set;}
        public IConfiguration Configuration
        {
            get;
            set;
        }

        public RegistrationController(IConfiguration Configuration)
        {
            this.Configuration = Configuration;
            myConfig = Configuration["DbConnectionString"];
        }

        [Route("api")]
        public String Register()
        {
            using(var conn = new MySqlConnection(myConfig)) 
            {
                conn.OpenAsync();
                MySqlCommand command = new MySqlCommand("SELECT * FROM accounts", conn);
                using(MySqlDataReader reader = command.ExecuteReader()) {
                    while(reader.Read()) {
                        Console.WriteLine(reader.GetString("username"));
                    }
                }
            }

            return myConfig;
        }
    }
}
