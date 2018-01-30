using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SecretSanta.Repository;

namespace SecretSanta.Controllers
{
    public class RegistrationController : Controller
    {
        private String myConfig {get;set;}
        private IUsersRepository Repository { get; set; }

        public RegistrationController(IConfiguration Configuration, IUsersRepository Repository)
        {
            this.Repository = Repository;
            this.myConfig = Configuration["DbConnectionString"];
        }

        [HttpPost("users")]
        public async Task<IActionResult> RegisterAsync([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                if (await Repository.userExists(user.Username)) 
                    {
                        return StatusCode(StatusCodes.Status409Conflict, "Username already exists");
                    }
                    await Repository.RegisterUser(user);
                return Created(Uri.UriSchemeHttp, new { displayname = user.DisplayName });
            }
            return BadRequest("Invalid registration request.");
        }
    }
}
