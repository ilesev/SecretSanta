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
            if (await GroupsRepository.GroupExistsAsync(group.GroupName))
            {
                return StatusCode(StatusCodes.Status409Conflict, "Groupname already exists.");
            }

            string authToken = getAuthToken(Request);
            string currentUser = await UsersRepository.GetUsernameByAuthTokenAsync(authToken);
            await GroupsRepository.CreateGroupAsync(group.GroupName, currentUser);
            return Created(Uri.UriSchemeHttp, new Group { GroupName = group.GroupName, Creator = currentUser });
        }

        ///groups/{groupName}/participants
        [HttpPost("groups/invitations")]
        public async Task<IActionResult> AcceptInvitation([FromQuery]string id)
        {
            Invitation inv = await GroupsRepository.GetInvitationByIdAsync(id);
            if (inv == null)
            {
                return BadRequest("No id match.");
            }

            await GroupsRepository.AddGroupMemberAsync(inv.Groupname, inv.Username);
            await GroupsRepository.DeleteInvitationAsync(id);
            return Created(Uri.UriSchemeHttp, new {Groupname = inv.Groupname, Username = inv.Username});
        }

        [HttpDelete("groups/invitations")]
        public async Task<IActionResult> DeleteInvitation([FromQuery]string id)
        {
            Invitation inv = await GroupsRepository.GetInvitationByIdAsync(id);
            if (inv == null)
            {
                return BadRequest("No id match.");
            }

            await GroupsRepository.DeleteInvitationAsync(id);
            return NoContent();
        }

        [HttpPost("groups/{groupname}/links")]
        public async Task<IActionResult> LinkPeople(string groupname)
        {
            if (!await GroupsRepository.GroupExistsAsync(groupname))
            {
                return NotFound("Group not found.");
            }

            string authToken = getAuthToken(Request);
            string currentUser = await UsersRepository.GetUsernameByAuthTokenAsync(authToken);
            string admin = await GroupsRepository.GetAdminOfGroupAsync(groupname);

            if(currentUser == null || admin == null) 
            {
                return BadRequest("Invalid data.");
            }
            else if (!currentUser.Equals(admin))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You aren't the administrator of the group.");
            }

            IEnumerable<GroupMember> groupMembers = await GroupsRepository.GetGroupMembersAsync(groupname);

            if (groupMembers.Count() < 2)
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed, "Can't start gift giving with less than 2 people in group.");
            }

            if (groupMembers.Any(x => x.GiftsTo != null))
            {
                return BadRequest("Gift process has already been started once.");
            }

            await GroupsRepository.StartGiftGivingAsync(groupname, groupMembers);
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpGet("groups/{groupname}/participants")]
        public async Task<IActionResult> GetAllMembers(string groupname)
        {
            if (!await GroupsRepository.GroupExistsAsync(groupname))
            {
                return NotFound("Group not found.");
            }

            string authToken = getAuthToken(Request);
            string currentUser = await UsersRepository.GetUsernameByAuthTokenAsync(authToken);
            string admin = await GroupsRepository.GetAdminOfGroupAsync(groupname);

            if (admin == null || currentUser == null)
            {
                return BadRequest("Bad data.");
            }
            else if (!admin.Equals(currentUser))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You are not the admin of this group");
            }

            IEnumerable<GroupMember> members = await GroupsRepository.GetGroupMembersAsync(groupname);
            return Ok(members);
        }

        [HttpDelete("groups/{groupname}/participants/{username}")]
        public async Task<IActionResult> DeleteMember(string groupname, string username)
        {
            if (!await GroupsRepository.GroupExistsAsync(groupname))
            {
                return NotFound("Group doesn't exist.");
            }

            string authToken = getAuthToken(Request);
            string currentUser = await UsersRepository.GetUsernameByAuthTokenAsync(authToken);
            string admin = await GroupsRepository.GetAdminOfGroupAsync(groupname);

            if (admin == null || currentUser == null)
            {
                return BadRequest("Bad data.");
            }
            else if (!admin.Equals(currentUser))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You are not the admin of this group");
            }

            IEnumerable<GroupMember> members = await GroupsRepository.GetGroupMembersAsync(groupname);
            GroupMember toDelete = members.FirstOrDefault(x => x.Username.Equals(username));

            if (toDelete == null)
            {
                return NotFound("User does not belong to group.");
            }

            await GroupsRepository.DeleteMemberAsync(toDelete);

            return NoContent();
        }

        private string getAuthToken(HttpRequest request)
        {
            StringValues authToken;
            request.Headers.TryGetValue("AuthenticationToken", out authToken);
            return authToken[0];
        }
    }
}
