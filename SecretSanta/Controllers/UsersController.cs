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
    public class UsersController : Controller
    {
        private IUsersRepository UsersRepository { get; set; }
        private IGroupsRepository GroupsRepository { get; set; }

        public UsersController(IUsersRepository Repository, IGroupsRepository GroupsRepository)
        {
            this.UsersRepository = Repository;
            this.GroupsRepository = GroupsRepository;
        }

        [HttpPost("users")]
        public async Task<IActionResult> RegisterAsync([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                if (await UsersRepository.userExistsAsync(user.Username)) 
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Username already exists");
                }
                await UsersRepository.RegisterUserAsync(user);
                return Created(Uri.UriSchemeHttp, new { displayname = user.DisplayName });
            }
            return BadRequest("Invalid registration request.");
        }

        ///users?skip={s}&take={t}&order={Asc|Desc}&search={phrase}
        [HttpGet("users")]
        [ServiceFilter(typeof(AuthenticationFilter))]
        public async Task<IActionResult> GetListOfUsers([FromQuery] string name, [FromQuery] int skip = 0, [FromQuery] int take = 1,
                                                        [FromQuery] string order = "Asc", [FromQuery] string type = "username")
        {
            if (DataIsValid(skip, take, order, type))
            {
                IEnumerable<UsersVM> users = await UsersRepository.getListOfUsers(name, skip, take, order.ToLower(), type.ToLower());
                return Ok(users);
            }

            return BadRequest("Bad data.");
        }

        //~/users/{username}
        [HttpGet("users/{username}")]
        [ServiceFilter(typeof(AuthenticationFilter))]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            if (!await UsersRepository.userExistsAsync(username))
            {
                return NotFound("Username not found.");
            }
            //should have only 1 user
            IEnumerable<UsersVM> users = await UsersRepository.getListOfUsers(username, 0, 1, "asc", "username");
            return Ok(users.FirstOrDefault());
        }

        private bool DataIsValid(int skip, int take, string order, string type)
        {
            if (! (type.Equals("username", StringComparison.OrdinalIgnoreCase) || 
                   type.Equals("displayname", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            if (!(order.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                   order.Equals("desc", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            if(skip < 0 || take < 0)
            {
                return false;
            }

            return true;
        }

        [HttpPost("users/{username}/invitations")]
        [ServiceFilter(typeof(AuthenticationFilter))]
        public async Task<IActionResult> SendInvitation(string username, [FromBody] Invitation invitation)
        {
            if (!await UsersRepository.userExistsAsync(username))
            {
                return NotFound("Username not found.");
            }
            if (invitation.Groupname == null)
            {
                return BadRequest("Groupname is missing.");
            }

            string authToken = getAuthToken(Request);
            invitation.DateCreated = DateTime.Now;
            string adminOfGroup = await GroupsRepository.getAdminOfGroupAsync(invitation.Groupname);
            string currentUser = await UsersRepository.getUsernameByAuthTokenAsync(authToken);

            if (!currentUser.Equals(adminOfGroup))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You are not the administrator of this group");
            }

            if (await GroupsRepository.UserHasInvitationForGroupAsync(invitation.Groupname, username))
            {
                return StatusCode(StatusCodes.Status409Conflict, "User already has an invite for this group.");
            }

            invitation.InvitationStatus = "pending";
            invitation.Username = username;
            invitation.Id = Guid.NewGuid().ToString();

            await GroupsRepository.sendInvitationAsync(invitation);
            return Created(Uri.UriSchemeHttp, new { id = invitation.Id });
        }

        ///users/{username}/invitations?skip={s}&take={t}&order={A|D} 
        [HttpGet("users/{username}/invitations")]
        [ServiceFilter(typeof(AuthenticationFilter))]
        public async Task<IActionResult> GetUserInvitations(string username, [FromQuery] int skip = 0,
                                                            [FromQuery] int take = 1, [FromQuery]string order = "asc")
        {
            if (!await UsersRepository.userExistsAsync(username))
            {
                return NotFound("Username does not exist.");
            }

            if (!isDataValid(skip, take, order))
            {
                return BadRequest("Invalid data");
            }

            string authToken = getAuthToken(Request);
            string currentUsername = await UsersRepository.getUsernameByAuthTokenAsync(authToken);
            if (!currentUsername.Equals(username))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You don't have access to this.");
            }

            IEnumerable<InvitationVM> invitations = await GroupsRepository.getPaginatedInvitationsAsync(username, skip, take, order.ToLower());
            return Ok(invitations);
        }

        [HttpGet("users/groups")]
        [ServiceFilter(typeof(AuthenticationFilter))]
        public async Task<IActionResult> GetUserGroups([FromQuery]int skip = 0, [FromQuery] int take = 1)
        {
            if (skip < 0 || take < 1)
            {
                return BadRequest("Invalid data.");
            }
            string authToken = getAuthToken(Request);
            string username = await UsersRepository.getUsernameByAuthTokenAsync(authToken);

            IEnumerable<object> groups = await GroupsRepository.GetUserGroups(username, skip, take);
            return Ok(groups);
        }

        [HttpGet("users/groups/{groupname}/links")]
        [ServiceFilter(typeof(AuthenticationFilter))]
        public async Task<IActionResult> IsUserConnectInGroup(string groupname)
        {
            if (!await GroupsRepository.groupExistsAsync(groupname))
            {
                return NotFound("Group not found");
            }

            string authToken = getAuthToken(Request);
            string currentUser = await UsersRepository.getUsernameByAuthTokenAsync(authToken);
            IEnumerable<GroupMember> members = await GroupsRepository.getGroupMembers(groupname);
            GroupMember member = members.FirstOrDefault(x => x.Username.Equals(currentUser));

            if (member == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "User not part of group.");
            }

            string giftsTo = member.GiftsTo;
            if (giftsTo == null)
            {
                return NotFound("Gift process hasn't started yet.");
            }

            return Ok(new { receiver = giftsTo });
        }

        private bool isDataValid(int skip, int take, string order)
        {
            if (skip < 0 || take < 1)
            {
                return false;
            }
            if (!(order.Equals("asc", StringComparison.OrdinalIgnoreCase) || order.Equals("desc", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            return true;
        }

        private string getAuthToken(HttpRequest request)
        {
            StringValues authToken;
            request.Headers.TryGetValue("AuthenticationToken", out authToken);
            return authToken[0];
        }

    }
}
