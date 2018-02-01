using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Filter;
using SecretSanta.Repository;

namespace SecretSanta.Controllers
{
    public class LoginController : Controller
    {
        private IUsersRepository UsersRepository { get; set; }

        public LoginController(IUsersRepository UsersRepository)
        {
            this.UsersRepository = UsersRepository;
        }

        [HttpPost("logins")]
        public async Task<IActionResult> Login([FromBody]User user)
        {
            if (!await UsersRepository.UserExistsAsync(user.Username))
            {
                return NotFound(String.Format("Username {0} was not found", user.Username));
            }

            if (!await UsersRepository.PasswordsMatchAsync(user.Username, user.Password))
            {
                return StatusCode(StatusCodes.Status401Unauthorized ,"Incorrect password");
            }

            if (await UsersRepository.IsUserSignedInAsync(user.Username))
            {
                return Ok(new {AuthenticationToken = await UsersRepository.GetGuidForSignedInUserAsync(user.Username)});
            }

            string guid = Guid.NewGuid().ToString();
            await UsersRepository.SignInUserAsync(user.Username, guid);

            return Created(Uri.UriSchemeHttp, new {AuthenticationToken = guid});
        }

        [HttpDelete("logins/{username}")]
        [ServiceFilter(typeof(AuthenticationFilter))]
        public async Task<IActionResult> Logout(string username)
        {
            if (await UsersRepository.IsUserSignedInAsync(username))
            {
                await UsersRepository.DeleteSignedInUserAsync(username);
                return NoContent();
            }

            return NotFound();
        }
    }
}
