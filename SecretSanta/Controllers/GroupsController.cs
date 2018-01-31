using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SecretSanta.Filter;
using SecretSanta.Models;
using SecretSanta.Repository;

namespace SecretSanta.Controllers
{
    [ServiceFilter(typeof(AuthenticationFilter))]
    public class GroupsController : Controller
    {
        private IUsersRepository UsersRepository { get; set; }
        private IGroupsRepository GroupsRepository { get; set; }

        public GroupsController(IUsersRepository UsersRepository, IGroupsRepository GroupsRepository)
        {
            this.UsersRepository = UsersRepository;
            this.GroupsRepository = GroupsRepository;
        }

        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroupAsync([FromBody]Group group)
        {
            if (await GroupsRepository.groupExistsAsync(group.GroupName))
            {
                return StatusCode(StatusCodes.Status409Conflict, "Groupname already exists.");
            }

            StringValues authToken;
            Request.Headers.TryGetValue("AuthenticationToken", out authToken);
            string currentUser = await UsersRepository.getUsernameByAuthTokenAsync(authToken[0]);
            await GroupsRepository.CreateGroupAsync(group.GroupName, currentUser);
            return Created(Uri.UriSchemeHttp, new Group { GroupName = group.GroupName, Creator = currentUser });
        }
    }
}
