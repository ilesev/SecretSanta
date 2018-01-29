using System;
using System.Collections.Generic;
using System.Data.Common;
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

        public RegistrationController(IConfiguration Configuration)
        {
            myConfig = Configuration["DbConnectionString"];
        }

        [Route("api")]
        public async Task<string> RegisterAsync()
        {
            return myConfig;
        }
    }
}
